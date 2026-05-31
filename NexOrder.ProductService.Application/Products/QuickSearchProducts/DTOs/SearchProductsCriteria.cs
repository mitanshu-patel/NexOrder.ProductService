public class SearchProductsCriteria
{
    public string? SearchText { get; set; }
    public string? Category { get; set; }
    
    // The key to your price problem:
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<decimal>? SpecificPrices { get; set; }

    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;
}