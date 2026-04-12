namespace NexOrder.ProductService.Application.Products.UpdateProduct
{
    public record UpdateProductCommand(int ProductId, ProductCriteria Criteria);
}
