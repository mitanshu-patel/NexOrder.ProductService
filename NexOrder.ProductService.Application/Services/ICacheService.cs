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
        public Task<T?> GetValueAsync<T>(string cacheKey);

        public T? GetValue<T>(string cacheKey);

        public void SetValue<T>(string cacheKey, T cacheValue, DistributedCacheEntryOptions? options = null);

        public Task SetValueAsync<T>(string cacheKey, T cacheValue, DistributedCacheEntryOptions? options = null);

        public Task RefreshCacheAsync(string cacheKey);
    }
}
