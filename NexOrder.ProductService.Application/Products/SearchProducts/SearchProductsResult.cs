using NexOrder.ProductService.Application.Products.SearchProducts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public record SearchProductsResult(List<SearchProductsDto> Products, int TotalRecords);
}
