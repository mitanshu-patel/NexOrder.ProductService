using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexOrder.ProductService.Application;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Registrations;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Infrastructure;
using NexOrder.ProductService.Infrastructure.Repos;
using NexOrder.ProductService.Infrastructure.Services;

var builder = FunctionsApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables().Build();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Services.RegisterHandlers();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddSingleton<IMessageDeliveryService, MessageDeliveryService>();

builder.Services.AddDbContext<ProductsContext>(
    v => v.UseSqlServer(configuration.GetConnectionString("SystemDbConnectionString"),
    b => b.MigrationsAssembly("NexOrder.ProductService.Infrastructure")));
builder.Services.AddScoped<IProductRepo, ProductRepo>();

builder.Build().Run();
