using NexOrder.ProductService.Application.Products.SearchProducts.DTOs;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public record SearchProductsResult(List<SearchProductsDto> Products, int TotalRecords);
}
