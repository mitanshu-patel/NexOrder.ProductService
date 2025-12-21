using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NexOrder.ProductService.Infrastructure
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ProductsContext>
    {
        public ProductsContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ProductsContext>();
            var connectionString = configuration.GetConnectionString("SystemDbConnectionString");

            // Explicitly set the migrations assembly
            optionsBuilder.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly("NexOrder.ProductService.Infrastructure")
            );

            return new ProductsContext(optionsBuilder.Options);
        }
    }
}
