using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Services
{
    public interface ICacheService
    {
        public Task<string> GetValueAsync(string cacheKey);

        public string GetValue(string cacheKey);

        public void SetValue(string cacheKey, string cacheValue, DistributedCacheEntryOptions? options = null);

        public Task SetValueAsync(string cacheKey, string cacheValue, DistributedCacheEntryOptions? options = null);

        public Task RefreshCacheAsync(string cacheKey);
    }
}
