using NexOrder.ProductService.Application.Products.Common.DTOs;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public record SearchProductsResult(List<SearchProductsDto> Products, int TotalRecords);
}
