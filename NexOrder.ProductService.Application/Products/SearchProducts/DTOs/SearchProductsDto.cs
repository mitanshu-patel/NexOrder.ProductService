using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.SearchProducts.DTOs
{
    public record SearchProductsDto
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public decimal Price { get; init; }
    }
}
