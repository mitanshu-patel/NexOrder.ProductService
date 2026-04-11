using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Infrastructure.Services;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace NexOrder.ProductService
{
    public static class CacheRegistrations
    {
        public static void AddRedisCache(this FunctionsApplicationBuilder builder, string? configuration, string? instanceName)
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                // Read Redis settings from local settings (or environment) loaded above
                options.Configuration = configuration;
                options.InstanceName = instanceName;
            });
            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.AddFusionCache()
            .WithDefaultEntryOptions(options => {
                options.Duration = TimeSpan.FromMinutes(5);
                options.LockTimeout = TimeSpan.FromSeconds(10); // This is duration upto which lock will be held.
            })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>());
        }
    }
}
