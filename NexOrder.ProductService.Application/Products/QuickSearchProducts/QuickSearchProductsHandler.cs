using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using NexOrder.ProductService.Application;
using NexOrder.ProductService.Application.Plugins;
using NexOrder.ProductService.Application.Products.Common.DTOs;
using NexOrder.ProductService.Shared.Common;
using Polly;
using Polly.RateLimiting;
using Polly.Registry;
using System.Reflection;
using System.Text;
public class QuickSearchProductsHandler : RequestHandlerBase<QuickSearchProductsQuery, CustomResponse<QuickSearchProductsResult>>
{
    private readonly IProductRepo productRepo;
    private readonly ILogger<QuickSearchProductsHandler> logger;
    private readonly ResiliencePipeline pipeline;
    private readonly Kernel kernel;

    private readonly IChatCompletionService chatCompletionService;

    public QuickSearchProductsHandler(IProductRepo productRepo, ILogger<QuickSearchProductsHandler> logger, ResiliencePipelineProvider<string> pipelineProvider, Kernel kernel, IChatCompletionService chatCompletionService)
    {
        this.productRepo = productRepo;
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
                   PromptExecutionSettings settings = new()
                   {
                       FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                   };
                   chatMessages.AddSystemMessage("You are a product search assistant. Use the search-product function to find products based on the user's query.");
                   chatMessages.AddUserMessage($"Search for products: {command.SearchMessage}");
                   chatMessages.AddDeveloperMessage($"Return only list in JSON format with type List<{nameof(SearchProductsDto)}>");

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

    private async Task<List<SearchProductsDto>> SearchProducts(SearchProductsCriteria criteria)
    {
        var products = this.productRepo.GetProducts();

        if (!string.IsNullOrEmpty(criteria.SearchText))
        {
            products = products.Where(v => v.Name.Contains(criteria.SearchText) || v.Description.Contains(criteria.SearchText));
        }

        if (criteria.MinPrice.HasValue)
        {
            products = products.Where(v => v.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            products = products.Where(v => v.Price <= criteria.MaxPrice.Value);
        }

        if (criteria.SpecificPrices != null && criteria.SpecificPrices.Any())
        {
            products = products.Where(v => criteria.SpecificPrices.Contains(v.Price));
        }

        if(criteria.SortBy != null)
        {
            if(criteria.SortBy.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                products = criteria.SortDescending ? products.OrderByDescending(v => v.Name) : products.OrderBy(v => v.Name);
            }
            else if(criteria.SortBy.Equals("price", StringComparison.OrdinalIgnoreCase))
            {
                products = criteria.SortDescending ? products.OrderByDescending(v => v.Price) : products.OrderBy(v => v.Price);
            }
            else if(criteria.SortBy.Equals("createdat", StringComparison.OrdinalIgnoreCase))
            {
                products = criteria.SortDescending ? products.OrderByDescending(v => v.CreatedAtUtc) : products.OrderBy(v => v.CreatedAtUtc);
            }
        }
        else
        {
            products = products.OrderByDescending(v => v.CreatedAtUtc);
        }

        return await products
                        .Select(v => new SearchProductsDto
                        {
                            Price = v.Price,
                            Name = v.Name,
                            Id = v.Id
                        })
                        .Take(criteria.PageSize)
                        .ToListAsync();
    }
}