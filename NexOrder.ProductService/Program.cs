using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexOrder.Framework.Core;
using NexOrder.Framework.Core.Common;
using NexOrder.ProductService;
using NexOrder.ProductService.Application;
using NexOrder.ProductService.Infrastructure;
using NexOrder.ProductService.Infrastructure.Helpers;
using NexOrder.ProductService.Infrastructure.Repos;
using NexOrder.ProductService.Messages.Commands;
using NexOrder.ProductService.Shared.Common;
using Polly;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = FunctionsApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
var environment = configuration.GetValue<string>("ENVIRONMENT");
var isDevelopment = !string.IsNullOrEmpty(environment) && environment.Equals(
            "DEVELOPMENT",
            System.StringComparison.InvariantCultureIgnoreCase);

builder.ConfigureFunctionsWebApplication();

var appInsightsConnection = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

builder.Services.AddNexOrderCustomLogging(isDevelopment, "NexOrder.ProductService", appInsightsConnection);
builder.Services.AddMessageDeliveryService(options =>
{
    options.ServiceBusConnectionString = configuration["ServiceBusConnectionString"] 
        ?? configuration.GetConnectionString("ServiceBusConnectionString") 
        ?? string.Empty;
#if DEBUG
    options.WebProxyAddress = Environment.GetEnvironmentVariable("WebProxy") ?? string.Empty;
#endif
});


builder.Services.AddOpenAIService(options =>
{
    options.ApiKey = configuration["OpenAIAPIKey"] ?? string.Empty;
    options.DeploymentName = configuration["OpenAIDeployment"] ?? string.Empty;
    options.Url = configuration["OpenAIEndpoint"] ?? string.Empty;
    options.Model = configuration["OpenAIModel"] ?? string.Empty;
});

builder.Services.AddResiliencePipeline(ProductServiceConstants.OpenAIResiliencePipeline, (pipelineBuilder, context) =>
{
    pipelineBuilder.AddRateLimiter(new FixedWindowRateLimiter(
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = Convert.ToInt16(configuration["RateLimitOptions_Permit"] ?? "1"),
                                    Window = TimeSpan.FromMinutes(Convert.ToInt16(configuration["RateLimitOptions_WindowInMinutes"] ?? "1")),
                                    QueueLimit = Convert.ToInt16(configuration["RateLimitOptions_Queue"] ?? "0"),
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                                }))
    .AddRetry(new Polly.Retry.RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        Delay = TimeSpan.FromSeconds(2),
        UseJitter = true,
        MaxRetryAttempts = 3,
    });
});

builder.Services.RegisterHandlers(Assembly.Load("NexOrder.ProductService.Application"));
var connectionString = ConnectionStringsHelper.GetDbConnectionString();
builder.Services.AddDbContext<ProductsContext>(
    v => v.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("NexOrder.ProductService.Infrastructure")));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.AddRedisCache(
    configuration["RedisCacheOptions_Configuration"],
    configuration["RedisCacheOptions_InstanceName"]);
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ProductsContext>("ProductsDb")
    .AddRedis(configuration["RedisCacheOptions_Configuration"], name: "RedisCache")
    .AddAzureServiceBusQueue(configuration.GetConnectionString("ServiceBusConnectionString"), ProductServiceCommand.QueueName, name: "ProductServiceQueue");
var app = builder.Build();
if (builder.Configuration.GetValue<bool>("RunMigration"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    db.Database.Migrate();
    //return; // Exit after migration
}

app.Run();
