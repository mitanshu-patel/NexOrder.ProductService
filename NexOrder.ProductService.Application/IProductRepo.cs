using NexOrder.ProductService.Domain.Entities;

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
