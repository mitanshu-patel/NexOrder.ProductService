using NexOrder.ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application
{
    public interface IProductRepo
    {
        public IQueryable<Product> GetProducts();

        public Task AddProductAsync(Product user);

        public Task UpdateProductAsync(Product user);

        public Task DeleteProductAsync(Product user);
    }
}
