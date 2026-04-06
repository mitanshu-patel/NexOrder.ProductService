using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexOrder.ProductService;
using NexOrder.ProductService.Application;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Registrations;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Infrastructure;
using NexOrder.ProductService.Infrastructure.Helpers;
using NexOrder.ProductService.Infrastructure.Repos;
using NexOrder.ProductService.Infrastructure.Services;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = FunctionsApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Services.RegisterHandlers();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddSingleton<IMessageDeliveryService, MessageDeliveryService>();
var connectionString = ConnectionStringsHelper.GetDbConnectionString();
builder.Services.AddDbContext<ProductsContext>(
    v => v.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("NexOrder.ProductService.Infrastructure")));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddRedisCache(
    configuration["RedisCacheOptions_Configuration"],
    configuration["RedisCacheOptions_InstanceName"]);
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(options => {
        options.Duration = TimeSpan.FromMinutes(5);
        // This is your Stampede Protection!
        options.LockTimeout = TimeSpan.FromSeconds(10); 
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDistributedCache(builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>());
var app = builder.Build();
if (builder.Configuration.GetValue<bool>("RunMigration"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    db.Database.Migrate();
    //return; // Exit after migration
}

app.Run();
