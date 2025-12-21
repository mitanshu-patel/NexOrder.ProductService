using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public record SearchProductsQuery(int PageNumber, int PageSize, string? SearchText = null)
    {
        [JsonIgnore]
        public int PageIndex { get => this.PageNumber == 0 ? this.PageNumber : this.PageNumber - 1; }
    }
}
