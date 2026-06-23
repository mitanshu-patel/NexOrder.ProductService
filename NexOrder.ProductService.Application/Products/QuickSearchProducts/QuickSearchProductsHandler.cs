using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NexOrder.Framework.Core.Common;
using NexOrder.ProductService.Application.Products.Common.DTOs;
using NexOrder.ProductService.Shared.Common;
using Polly;
using Polly.RateLimiting;
using Polly.Registry;
public class QuickSearchProductsHandler : RequestHandlerBase<QuickSearchProductsQuery, CustomResponse<QuickSearchProductsResult>>
{
    private readonly ILogger<QuickSearchProductsHandler> logger;
    private readonly ResiliencePipeline pipeline;
    private readonly Kernel kernel;

    private readonly IChatCompletionService chatCompletionService;

    public QuickSearchProductsHandler(ILogger<QuickSearchProductsHandler> logger, ResiliencePipelineProvider<string> pipelineProvider, Kernel kernel)
    {
        this.logger = logger;
        this.pipeline = pipelineProvider.GetPipeline(ProductServiceConstants.OpenAIResiliencePipeline);
        this.kernel = kernel;
        this.chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    protected async override Task<CustomResponse<QuickSearchProductsResult>> ExecuteCommandAsync(QuickSearchProductsQuery command)
    {
        try
            {
               this.logger.LogInformation("QuickSearchProductsHandler: ExecuteCommandAsync execution started");
               return await this.pipeline.ExecuteAsync(async response => {
                   var chatMessages = new ChatHistory();
                   OpenAIPromptExecutionSettings settings = new()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        ResponseFormat = typeof(List<SearchProductsDto>),
                    };
                   chatMessages.AddSystemMessage("You are a product search assistant. Use the search-product function to find products based on the user's query.");
                   chatMessages.AddUserMessage($"Search for products: {command.SearchMessage}");
                //    chatMessages.AddDeveloperMessage($"Return only list in JSON format with type List<{nameof(SearchProductsDto)}>");

                   var result = await this.chatCompletionService.GetChatMessageContentAsync(chatMessages, executionSettings: settings, kernel: this.kernel);
                   var productsList = System.Text.Json.JsonSerializer.Deserialize<List<SearchProductsDto>>(result.Content ?? string.Empty) ?? [];

                   return CustomHttpResult.Ok(new QuickSearchProductsResult(productsList));
               });
            }
            catch (RateLimiterRejectedException rex)
            {
                // Reason behind manually handling this exception is here is that centrally registering this on middleware needs fixed return type however in this architecture, type is defined based on mediator used on Function.
                // So, to return appropriate response for this specific scenario, we are handling this exception here in handler itself.
                this.logger.LogWarning(rex, "QuickSearchProductsHandler: request was rate limited with message:{message}", rex.Message);
                return CustomHttpResult.TooManyRequests<QuickSearchProductsResult>("Too many requests. Please try again later.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "QuickSearchProductsHandler: exception occurred with message:{message}", ex.Message);
                throw;
            }
    }

    protected override CustomResponse<QuickSearchProductsResult> GetValidationFailedResult(ValidationResult validationResult)
    {
        return validationResult.GetValidationResult<QuickSearchProductsResult>();
    }

    protected override IValidator<QuickSearchProductsQuery> GetValidator()
    {
        var validator = new InlineValidator<QuickSearchProductsQuery>();
        validator.RuleFor(v => v.SearchMessage).NotEmpty().WithMessage("Search message cannot be empty");
        return validator;
    }
}