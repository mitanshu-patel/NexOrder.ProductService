using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using NexOrder.ProductService.Application.Plugins;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Infrastructure.Services;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace NexOrder.ProductService
{
    public static class RegistrationExtensions
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
            .WithDefaultEntryOptions(options =>
            {
                options.Duration = TimeSpan.FromMinutes(5);
                options.LockTimeout = TimeSpan.FromSeconds(10); // This is duration upto which lock will be held.
            })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>());
        }

        public static void AddKernelWithPlugins(
            this FunctionsApplicationBuilder builder,
            string deploymentName,
            string apiKey,
            string endPoint,
            string modelId)
        {
            builder.Services.AddScoped<SearchProductPlugin>();
            builder.Services.AddScoped<AddProductPlugin>();
            builder.Services.AddKernel().AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                apiKey: apiKey,
                endpoint: endPoint,
                modelId: modelId);

            builder.Services.AddScoped<KernelPlugin>(sp =>
            {
                var pluginInstance = sp.GetRequiredService<SearchProductPlugin>();
                return KernelPluginFactory.CreateFromObject(pluginInstance, "SearchProductPlugin");
            });

            builder.Services.AddScoped<KernelPlugin>(sp =>
            {
                var pluginInstance = sp.GetRequiredService<AddProductPlugin>();
                return KernelPluginFactory.CreateFromObject(pluginInstance, "AddProductPlugin");
            });

        }
    }
}
