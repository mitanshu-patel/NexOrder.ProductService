using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Products.SearchProducts.DTOs;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public class SearchProductsHandler : RequestHandlerBase<SearchProductsQuery, CustomResponse<SearchProductsResult>>
    {
        private readonly ILogger<SearchProductsHandler> logger;
        private readonly IProductRepo productRepo;

        public SearchProductsHandler(ILogger<SearchProductsHandler> logger, IProductRepo productRepo)
        {
            this.logger = logger;
            this.productRepo = productRepo;
        }

        protected async override Task<CustomResponse<SearchProductsResult>> ExecuteCommandAsync(SearchProductsQuery command)
        {
            try
            {
                this.logger.LogInformation("SearchProductsHandler: ExecuteCommandAsync execution started");
                var products = this.productRepo.GetProducts();

                if (!string.IsNullOrEmpty(command.SearchText))
                {
                    products = products.Where(v => v.Name.Contains(command.SearchText) || v.Description.Contains(command.SearchText));
                }

                var totalRecords = await products.CountAsync();

                var productsList = await products
                                .OrderByDescending(v => v.CreatedAtUtc)
                                .Select(v => new SearchProductsDto
                                {
                                    Price = v.Price,
                                    Name = v.Name,
                                    Id = v.Id
                                })
                                .Skip(command.PageIndex * command.PageSize)
                                .Take(command.PageSize)
                                .ToListAsync();

                this.logger.LogInformation("SearchProductsHandler: ExecuteCommandAsync execution completed and found {count} products", totalRecords);

                return CustomHttpResult.Ok(new SearchProductsResult(productsList, totalRecords));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SearchProductsHandler: exception occurred with message:{message}", ex.Message);
                throw;
            }
        }

        protected override CustomResponse<SearchProductsResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<SearchProductsResult>();
        }

        protected override IValidator<SearchProductsQuery> GetValidator()
        {
            var validator = new InlineValidator<SearchProductsQuery>();
            validator.RuleFor(v => v.PageIndex).GreaterThanOrEqualTo(0);
            validator.RuleFor(v => v.PageSize).GreaterThan(0);
            return validator;
        }
    }
}
