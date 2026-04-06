using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Products.SearchProducts.DTOs;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Domain.Entities;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public class SearchProductsHandler : RequestHandlerBase<SearchProductsQuery, CustomResponse<SearchProductsResult>>
    {
        private readonly ILogger<SearchProductsHandler> logger;
        private readonly IProductRepo productRepo;
        private readonly ICacheService cacheService;
        //private readonly SemaphoreSlim cacheLock;

        public SearchProductsHandler(ILogger<SearchProductsHandler> logger, IProductRepo productRepo, ICacheService cacheService)
        {
            this.logger = logger;
            this.productRepo = productRepo;
            this.cacheService = cacheService;
            //this.cacheLock = new SemaphoreSlim(1, 1);
        }

        protected async override Task<CustomResponse<SearchProductsResult>> ExecuteCommandAsync(SearchProductsQuery command)
        {
            try
            {
                this.logger.LogInformation("SearchProductsHandler: ExecuteCommandAsync execution started");
                var products = this.productRepo.GetProducts();

                // We only cache the first page of products without any search filter as that is the most common scenario and it will help to reduce the latency for majority of users.
                // For other scenarios we directly fetch from database without caching.
                var isCacheable = command.PageNumber <= 1 && string.IsNullOrEmpty(command.SearchText);
                var cacheVersion = string.Empty;
                var cacheKey = string.Empty;
                var cachedListValue = string.Empty; 
                var totalRecords = 0;
                List<SearchProductsDto> productsList = [];
                var cacheResponse = await this.GetCachedResponse(command, isCacheable, products);
                if (cacheResponse != null)
                {
                    this.logger.LogInformation("SearchProductsHandler: ExecuteCommandAsync execution completed and found {count} products from CACHE", totalRecords);
                    return cacheResponse;
                }

                cacheKey = CacheKey.ProductListCache(cacheVersion, command.PageIndex, command.PageSize, command.SearchText);

                if (!string.IsNullOrEmpty(command.SearchText))
                {
                    products = products.Where(v => v.Name.Contains(command.SearchText) || v.Description.Contains(command.SearchText));
                }

                totalRecords = await products.CountAsync();
                productsList = await products
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

                if (isCacheable)
                {
                    await this.cacheService.SetValueAsync(cacheKey, productsList);
                }

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

        private async Task<CustomResponse<SearchProductsResult>?> GetCachedResponse(SearchProductsQuery command, bool isCacheable, IQueryable<Product> products)
        {
            if (isCacheable)
            {
                var totalRecords = await products.CountAsync();

                // We use a cache version to invalidate all the cached data when there is a change in products data. Whenever there is a change in products data, we will update the cache version which will make all the existing cache data stale and it will be removed from cache when it expires after sliding expiration time.
                // This way we don't need to remove each cache entry individually when there is a change in products data.
                var cacheVersion = await this.cacheService.GetValueAsync<string>(CacheKey.ProductListCacheVersion);
                if (string.IsNullOrEmpty(cacheVersion))
                {
                    cacheVersion = "1";
                    this.cacheService.SetValue(CacheKey.ProductListCacheVersion, cacheVersion);
                }

                var cacheKey = CacheKey.ProductListCache(cacheVersion, command.PageIndex, command.PageSize);
                var cachedListValue = await this.cacheService.GetValueAsync<List<SearchProductsDto>>(cacheKey);
                if (cachedListValue != null)
                {
                    var productsList = cachedListValue;
                    this.logger.LogInformation("SearchProductsHandler: ExecuteCommandAsync execution completed and found {count} products from cache", totalRecords);
                    return CustomHttpResult.Ok(new SearchProductsResult(productsList, totalRecords));
                }
            }

            return null;
        }
    }
}
