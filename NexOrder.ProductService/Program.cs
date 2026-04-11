using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService;
using NexOrder.ProductService.Application;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Registrations;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Infrastructure;
using NexOrder.ProductService.Infrastructure.Helpers;
using NexOrder.ProductService.Infrastructure.Repos;
using NexOrder.ProductService.Infrastructure.Services;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

builder.Services.AddNexOrderCustomLogging(isDevelopment, appInsightsConnection);

builder.Services.RegisterHandlers();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddSingleton<IMessageDeliveryService, MessageDeliveryService>();
var connectionString = ConnectionStringsHelper.GetDbConnectionString();
builder.Services.AddDbContext<ProductsContext>(
    v => v.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("NexOrder.ProductService.Infrastructure")));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.AddRedisCache(
    configuration["RedisCacheOptions_Configuration"],
    configuration["RedisCacheOptions_InstanceName"]);
var app = builder.Build();
if (builder.Configuration.GetValue<bool>("RunMigration"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    db.Database.Migrate();
    //return; // Exit after migration
}

app.Run();
