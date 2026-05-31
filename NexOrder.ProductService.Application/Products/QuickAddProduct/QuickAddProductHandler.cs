using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using NexOrder.ProductService.Application.Products.AddProduct;
using NexOrder.ProductService.Application.Products.Common;
using NexOrder.ProductService.Domain.Entities;
using NexOrder.ProductService.Shared.Common;
using Polly;
using Polly.RateLimiting;
using Polly.Registry;
using System.Reflection;
using System.Text;

namespace NexOrder.ProductService.Application.Products.QuickAddProduct
{
    internal class QuickAddProductHandler : RequestHandlerBase<QuickAddProductCommand, CustomResponse<AddProductResult>>
    {
        private readonly IOpenAIService openAIService;
        private readonly IProductRepo productRepo;
        private readonly IMediator mediator;
        private readonly ILogger<QuickAddProductHandler> logger;
        private readonly ResiliencePipeline pipeline;

        public QuickAddProductHandler(IOpenAIService openAIService, IProductRepo productRepo, IMediator mediator, ILogger<QuickAddProductHandler> logger, ResiliencePipelineProvider<string> pipelineProvider)
        {
            this.openAIService = openAIService;
            this.productRepo = productRepo;
            this.mediator = mediator;
            this.logger = logger;
            this.pipeline = pipelineProvider.GetPipeline(ProductServiceConstants.OpenAIResiliencePipeline);
        }
        protected override async Task<CustomResponse<AddProductResult>> ExecuteCommandAsync(QuickAddProductCommand command)
        {
            try
            {
                this.logger.LogInformation("QuickAddProductHandler: ExecuteCommandAsync execution started");
                Type type = typeof(AddProductCommand);
                PropertyInfo[] properties = type.GetProperties();
                this.openAIService.InitializeOpenAIService();
                this.openAIService.SetSystemMessage("You are a helpful assistant for quickly adding a product to the system. You will receive a message describing the product details, and you need to generate a JSON object with the appropriate properties and their types for creating a product in our system.");
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Following is the message received for quick adding a product:");
                messageBuilder.AppendLine(command.ProductAddMessage);
                messageBuilder.AppendLine("Generate a JSON object with the following properties and their types for creating a product:");
                messageBuilder.AppendLine("{");
                foreach (PropertyInfo property in properties)
                {
                    messageBuilder.AppendLine($"\"{property.Name}\": \"{property.PropertyType}\",");
                }
                messageBuilder.AppendLine("}");
                messageBuilder.AppendLine("In case of any missing or invalid properties, please provide default values, don't generate any details by yourself, strictly stick to what user has provided");
                messageBuilder.AppendLine("For example if price is missing keep it's default value as per datatype which would be 0.00 if float/double, similary if name not mentioned then keep as empty string.");
                this.logger.LogInformation("QuickAddProductHandler: ExecuteCommandAsync execution started");
                return await this.pipeline.ExecuteAsync(async response =>
                {
                    var result = await this.openAIService.GenerateResponseAsyc(messageBuilder.ToString());
                    if (!string.IsNullOrEmpty(result))
                    {
                        var deserializedResult = System.Text.Json.JsonSerializer.Deserialize<AddProductCommand>(result);
                        if (deserializedResult == null)
                        {
                            return CustomHttpResult.BadRequest<AddProductResult>("Failed to add product using quick add. Please check the input message and try again.", null);
                        }
                        return await this.mediator.SendAsync<AddProductCommand, CustomResponse<AddProductResult>>(deserializedResult);
                    }

                    return CustomHttpResult.BadRequest<AddProductResult>("Failed to add product using quick add. Please check the input message and try again.", null);
                });
            }
            catch (RateLimiterRejectedException rex)
            {
                // Reason behind manually handling this exception is here is that centrally registering this on middleware needs fixed return type however in this architecture, type is defined based on mediator used on Function.
                // So, to return appropriate response for this specific scenario, we are handling this exception here in handler itself.
                this.logger.LogWarning(rex, "QuickAddProductHandler: request was rate limited with message:{message}", rex.Message);
                return CustomHttpResult.TooManyRequests<AddProductResult>("Too many requests. Please try again later.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "QuickAddProductHandler: exception occurred with message:{message}", ex.Message);
                return CustomHttpResult.BadRequest<AddProductResult>("An error occurred while processing the quick add product request. Please check the input message and try again.", null);
            }
        }

        protected override CustomResponse<AddProductResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<AddProductResult>();
        }

        protected override IValidator<QuickAddProductCommand> GetValidator()
        {
            var validator = new InlineValidator<QuickAddProductCommand>();
            validator.RuleFor(x => x.ProductAddMessage).NotEmpty().WithMessage("Product add message cannot be empty.");
            return validator;
        }
    }
}
