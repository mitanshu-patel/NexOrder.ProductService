using Microsoft.Extensions.Caching.Distributed;
using NexOrder.ProductService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache distributedCache;
        public CacheService(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public string GetValue(string cacheKey)
        {
            return this.distributedCache.GetString(cacheKey) ?? string.Empty;
        }

        public async Task<string> GetValueAsync(string cacheKey)
        {
            return await this.distributedCache.GetStringAsync(cacheKey) ?? string.Empty;
        }

        public async Task RefreshCacheAsync(string cacheKey)
        {
            // Here we're updating cache version so that old version isn't used anymore.
            var cacheVersion = await this.distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cacheVersion))
            {
                int version = Convert.ToInt32(cacheVersion);
                version += 1;
                this.distributedCache.SetString(cacheKey, version.ToString());
            }
        }

        public void SetValue(string cacheKey, string cacheValue, DistributedCacheEntryOptions? cacheOptions = null)
        {
            if(cacheOptions != null)
            {
                this.distributedCache.SetString(cacheKey, cacheValue, cacheOptions);
            }
            else
            {
                this.distributedCache.SetString(cacheKey, cacheValue);
            }
        }

        public async Task SetValueAsync(string cacheKey, string cacheValue, DistributedCacheEntryOptions? cacheOptions = null)
        {
            if (cacheOptions != null)
            {
                await this.distributedCache.SetStringAsync(cacheKey, cacheValue, cacheOptions);
            }
            else
            {
                await this.distributedCache.SetStringAsync(cacheKey, cacheValue);
            }
        }
    }
}
