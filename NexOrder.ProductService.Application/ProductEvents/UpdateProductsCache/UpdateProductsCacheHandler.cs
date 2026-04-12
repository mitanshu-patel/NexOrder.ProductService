using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NexOrder.Framework.Core.Common;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Shared.Common;

namespace NexOrder.ProductService.Application.ProductEvents.UpdateProductsCache
{
    public class UpdateProductsCacheHandler : RequestHandlerBase<UpdateProductsCacheCommand, CustomResponse<UpdateProductsCacheResult>>
    {
        private readonly ICacheService cacheService;
        private readonly ILogger<UpdateProductsCacheHandler> logger;

        public UpdateProductsCacheHandler(ICacheService cacheService, ILogger<UpdateProductsCacheHandler> logger)
        {
            this.cacheService = cacheService;
            this.logger = logger;
        }

        protected override async Task<CustomResponse<UpdateProductsCacheResult>> ExecuteCommandAsync(UpdateProductsCacheCommand command)
        {
            try
            {
                this.logger.LogDebug("Refreshing product list cache version.");
                await this.cacheService.RefreshCacheAsync(CacheKey.ProductListCacheVersion);
                this.logger.LogDebug("Product list cache version refreshed successfully.");
                return CustomHttpResult.Ok(new UpdateProductsCacheResult());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while updating products cache.");
                throw;
            }
        }

        protected override CustomResponse<UpdateProductsCacheResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<UpdateProductsCacheResult>();
        }

        protected override IValidator<UpdateProductsCacheCommand> GetValidator()
        {
            return new InlineValidator<UpdateProductsCacheCommand>();
        }
    }
}
