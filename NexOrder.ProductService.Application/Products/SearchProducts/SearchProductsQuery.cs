using Newtonsoft.Json;

namespace NexOrder.ProductService.Application.Products.SearchProducts
{
    public record SearchProductsQuery(int PageNumber, int PageSize, string? SearchText = null)
    {
        [JsonIgnore]
        public int PageIndex { get => this.PageNumber == 0 ? this.PageNumber : this.PageNumber - 1; }
    }
}
