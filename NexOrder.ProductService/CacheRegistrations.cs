using Microsoft.Extensions.DependencyInjection;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Infrastructure.Services;

namespace NexOrder.ProductService
{
    public static class CacheRegistrations
    {
        public static void AddRedisCache(this IServiceCollection services, string? configuration, string? instanceName)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                // Read Redis settings from local settings (or environment) loaded above
                options.Configuration = configuration;
                options.InstanceName = instanceName;
            });
            services.AddScoped<ICacheService, CacheService>();
        }
    }
}
