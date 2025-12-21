using Microsoft.EntityFrameworkCore;
using NexOrder.ProductService.Domain.Entities;

namespace NexOrder.ProductService.Infrastructure
{
    public class ProductsContext : DbContext
    {
        public ProductsContext(DbContextOptions<ProductsContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ProductsContext).Assembly);
        }
    }
}
