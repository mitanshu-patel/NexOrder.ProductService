namespace NexOrder.ProductService.Application.Products.Common.DTOs
{
    public record SearchProductsDto
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public decimal Price { get; init; }
    }
}
