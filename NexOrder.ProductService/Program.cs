using Google.Protobuf.WellKnownTypes;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
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
var app = builder.Build();
if (builder.Configuration.GetValue<bool>("RunMigration"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    db.Database.Migrate();
    //return; // Exit after migration
}

app.Run();
