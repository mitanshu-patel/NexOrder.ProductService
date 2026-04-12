namespace NexOrder.ProductService.Application.Products.AddProduct
{
    public record AddProductCommand(string Name, string Description, decimal Price);
}
