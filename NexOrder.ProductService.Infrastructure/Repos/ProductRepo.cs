using NexOrder.ProductService.Application;
using NexOrder.ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Infrastructure.Repos
{
    public class ProductRepo : IProductRepo
    {
        private readonly ProductsContext productsContext;
        public ProductRepo(ProductsContext productsContext)
        {
           this.productsContext = productsContext;
        }

        public async Task AddProductAsync(Product product)
        {
            this.productsContext.Products.Add(product);
            await this.productsContext.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Product product)
        {
            this.productsContext.Products.Remove(product);
            await this.productsContext.SaveChangesAsync();
        }

        public IQueryable<Product> GetProducts()
        {
            return this.productsContext.Products.AsQueryable();
        }

        public async Task UpdateProductAsync(Product product)
        {
            this.productsContext.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await this.productsContext.SaveChangesAsync();
        }
    }
}
