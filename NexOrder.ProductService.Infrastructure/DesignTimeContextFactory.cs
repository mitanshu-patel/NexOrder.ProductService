using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NexOrder.ProductService.Infrastructure.Helpers;
using System.IO;

namespace NexOrder.ProductService.Infrastructure
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ProductsContext>
    {
        public ProductsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProductsContext>();
            var connectionString = ConnectionStringsHelper.GetDbConnectionString();

            // Explicitly set the migrations assembly
            optionsBuilder.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly("NexOrder.ProductService.Infrastructure")
            );

            return new ProductsContext(optionsBuilder.Options);
        }
    }
}
