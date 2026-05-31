using NexOrder.Framework.Core.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Products.Common.DTOs;
using NexOrder.ProductService.Application;
using FluentValidation.Results;
using FluentValidation;
using NexOrder.Framework.Core.Contracts;
using System.Reflection;
using System.Text;
public class QuickSearchProductsHandler : RequestHandlerBase<QuickSearchProductsQuery, CustomResponse<QuickSearchProductsResult>>
{
    private readonly IProductRepo productRepo;
    private readonly ILogger<QuickSearchProductsHandler> logger;
    private readonly IOpenAIService openAIService;

    public QuickSearchProductsHandler(IProductRepo productRepo, ILogger<QuickSearchProductsHandler> logger, IOpenAIService openAIService)
    {
        this.productRepo = productRepo;
        this.logger = logger;
        this.openAIService = openAIService;
    }

    protected async override Task<CustomResponse<QuickSearchProductsResult>> ExecuteCommandAsync(QuickSearchProductsQuery command)
    {
        try
            {
                this.logger.LogInformation("QuickSearchProductsHandler: ExecuteCommandAsync execution started");
                Type type = typeof(SearchProductsCriteria);
                PropertyInfo[] properties = type.GetProperties();
                this.openAIService.InitializeOpenAIService();
                this.openAIService.SetSystemMessage("You are a helpful assistant for quickly searching products in the system. You will receive a message describing the product search criteria, and you need to generate a JSON object with the appropriate properties and their types for searching products in our system.");
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Following is the message received for quick searching products:");
                messageBuilder.AppendLine(command.SearchMessage);
                messageBuilder.AppendLine("Generate a JSON object with the following properties and their types for searching products:");
                var products = this.productRepo.GetProducts();
                messageBuilder.AppendLine("{");
                List<SearchProductsDto> productsList = [];
                foreach (PropertyInfo property in properties)
                {
                    messageBuilder.AppendLine($"\"{property.Name}\": \"{property.PropertyType}\",");
                }

                messageBuilder.AppendLine("}");
                messageBuilder.AppendLine("In case of any missing or invalid properties, please provide default values, don't generate any details by yourself, strictly stick to what user has provided");
                messageBuilder.AppendLine("For example if search text not mentioned then keep as empty string. If price range mentioned as 'between 100 and 500' then MinPrice should be 100 and MaxPrice should be 500. If specific prices mentioned as 'specific prices 100, 200, 300' then SpecificPrices should be [100, 200, 300].");
                messageBuilder.AppendLine("For example if sort by mentioned as 'sort by price descending' then SortBy should be 'price' and SortDescending should be true. If no sorting details mentioned then keep SortBy as null which means no specific sorting and it will be sorted by created date in descending order by default.");
                messageBuilder.AppendLine("For SortBy property valid values are 'name', 'price' and 'createdat'. For example if sort by mentioned as 'sort by name ascending' then SortBy should be 'name' and SortDescending should be false.");
                messageBuilder.AppendLine("For example if page size not mentioned then keep as 10. For nullable types you can keep as null if not mentioned in the input message.");
                var result = await this.openAIService.GenerateResponseAsyc(messageBuilder.ToString());
                if (!string.IsNullOrEmpty(result))
                {
                    var deserializedResult = System.Text.Json.JsonSerializer.Deserialize<SearchProductsCriteria>(result);
                    if (deserializedResult == null)                    {
                        return CustomHttpResult.BadRequest<QuickSearchProductsResult>("Failed to search products using quick search. Please check the input message and try again.", null);
                    }
                    productsList = await this.SearchProducts(deserializedResult);
                }

                this.logger.LogInformation("QuickSearchProductsHandler: ExecuteCommandAsync execution completed and found {count} products", productsList.Count);

                return CustomHttpResult.Ok(new QuickSearchProductsResult(productsList));
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