using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using NexOrder.ProductService.Application.Products.Common;
using NexOrder.ProductService.Shared.Common;
using Polly;
using Polly.RateLimiting;
using Polly.Registry;

namespace NexOrder.ProductService.Application.Products.QuickAddProduct
{
    internal class QuickAddProductHandler : RequestHandlerBase<QuickAddProductCommand, CustomResponse<AddProductResult>>
    {
        private readonly ILogger<QuickAddProductHandler> logger;
        private readonly ResiliencePipeline pipeline;
        private readonly Kernel kernel;
        private readonly IChatCompletionService chatCompletionService;

        public QuickAddProductHandler(ILogger<QuickAddProductHandler> logger, ResiliencePipelineProvider<string> pipelineProvider, Kernel kernel)
        {
            this.logger = logger;
            this.pipeline = pipelineProvider.GetPipeline(ProductServiceConstants.OpenAIResiliencePipeline);
            this.kernel = kernel;
            this.chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        }
        protected override async Task<CustomResponse<AddProductResult>> ExecuteCommandAsync(QuickAddProductCommand command)
        {
            try
            {
                this.logger.LogInformation("QuickAddProductHandler: ExecuteCommandAsync execution started");
                return await this.pipeline.ExecuteAsync(async response => {
                    var chatMessages = new ChatHistory();
                    OpenAIPromptExecutionSettings settings = new()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        ResponseFormat = typeof(CustomResponse<AddProductResult>),
                    };
                   
                    chatMessages.AddSystemMessage("You are a product addition assistant. Use the add-product function to add products based on the user's query.");
                    chatMessages.AddUserMessage($"Add product: {command.ProductAddMessage}");

                    var result = await this.chatCompletionService.GetChatMessageContentAsync(chatMessages, executionSettings: settings, kernel: this.kernel);
                    
                    // Configure JsonSerializerOptions to handle enums as strings and case-insensitive property names
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    };
                    
                    var addResponse = System.Text.Json.JsonSerializer.Deserialize<CustomResponse<AddProductResult>>(result.Content ?? string.Empty, jsonOptions);
                    if (addResponse == null)
                    {
                        return CustomHttpResult.BadRequest<AddProductResult>("Failed to add product using quick add. Please check the input message and try again.", null);
                    }
                    return addResponse;
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
