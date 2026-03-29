using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Shared.Common
{
    public static class CacheKey
    {
        public static string ProductListCache(string version, int pageNo, int pageSize, string? searchText = null)
        {
            var cacheKey = new StringBuilder();
            cacheKey.Append($"productList:version:v{version}:pageno:{pageNo}:pagesize:{pageSize}");
            if (!string.IsNullOrEmpty(searchText))
            {
                cacheKey.Append($":searchText:{searchText.ToLower()}");
            }

            return cacheKey.ToString();

        }

        public static string ProductListCacheVersion => "productList:version";
    }
}
