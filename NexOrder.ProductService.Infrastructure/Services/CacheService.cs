using Microsoft.Extensions.Caching.Distributed;
using NexOrder.ProductService.Application.Services;
using ZiggyCreatures.Caching.Fusion;

namespace NexOrder.ProductService.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IFusionCache distributedCache;
        public CacheService(IFusionCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public T? GetValue<T>(string cacheKey)
        {
            return this.distributedCache.GetOrDefault<T>(cacheKey);
        }

        public async Task<T?> GetValueAsync<T>(string cacheKey)
        {
            return await this.distributedCache.GetOrDefaultAsync<T>(cacheKey);
        }

        public async Task RefreshCacheAsync(string cacheKey)
        {
            // Here we're updating cache version so that old version isn't used anymore.
            var cacheVersion = await this.distributedCache.GetOrDefaultAsync<string>(cacheKey);
            if (!string.IsNullOrEmpty(cacheVersion))
            {
                int version = Convert.ToInt32(cacheVersion);
                version += 1;
                this.distributedCache.Set(cacheKey, version.ToString());
            }
        }

        public void SetValue<T>(string cacheKey, T cacheValue, DistributedCacheEntryOptions? cacheOptions = null)
        {
            this.distributedCache.Set(cacheKey, cacheValue);
        }

        public async Task SetValueAsync<T>(string cacheKey, T cacheValue, DistributedCacheEntryOptions? cacheOptions = null)
        {
            await this.distributedCache.SetAsync(cacheKey, cacheValue);
        }
    }
}
