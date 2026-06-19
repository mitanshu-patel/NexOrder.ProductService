using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using NexOrder.ProductService.Application;
using NexOrder.ProductService.Application.Products.Common.DTOs;

[Description("Queries product information from the product database.")]
public class SearchProductPlugin
{
    private readonly IProductRepo productRepo;
    public SearchProductPlugin(IProductRepo productRepo)
    {
        this.productRepo = productRepo;
    }

    [KernelFunction("search-product")]
    [Description("Search products based on a user input. The input is a search query provided by the user. The output is a list of products that match the search query.")]
    [return: Description("A list of products that match the search query.")]
    public async Task<List<SearchProductsDto>> SearchProductsAsync([Description("The search criteria for finding products.")] SearchProductsCriteria searchProductsCriteria)
    {
        return await SearchProducts(searchProductsCriteria);
    }

    private async Task<List<SearchProductsDto>> SearchProducts(SearchProductsCriteria criteria)
    {
        var products = this.productRepo.GetProducts();

        if (!string.IsNullOrEmpty(criteria.SearchText))
        {
            products = products.Where(v => v.Name.Contains(criteria.SearchText) || v.Description.Contains(criteria.SearchText));
        }

        if (criteria.MinPrice.HasValue)
        {
            products = products.Where(v => v.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            products = products.Where(v => v.Price <= criteria.MaxPrice.Value);
        }

        if (criteria.SpecificPrices != null && criteria.SpecificPrices.Any())
        {
            products = products.Where(v => criteria.SpecificPrices.Contains(v.Price));
        }

        if(criteria.SortBy != null)
        {
            if(criteria.SortBy.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                products = criteria.SortDescending ? products.OrderByDescending(v => v.Name) : products.OrderBy(v => v.Name);
            }
            else if(criteria.SortBy.Equals("price", StringComparison.OrdinalIgnoreCase))
            {
                products = criteria.SortDescending ? products.OrderByDescending(v => v.Price) : products.OrderBy(v => v.Price);
            }
            else if(criteria.SortBy.Equals("createdat", StringComparison.OrdinalIgnoreCase))
            {
                products = criteria.SortDescending ? products.OrderByDescending(v => v.CreatedAtUtc) : products.OrderBy(v => v.CreatedAtUtc);
            }
        }
        else
        {
            products = products.OrderByDescending(v => v.CreatedAtUtc);
        }

        return await products
                        .Select(v => new SearchProductsDto
                        {
                            Price = v.Price,
                            Name = v.Name,
                            Id = v.Id
                        })
                        .Take(criteria.PageSize)
                        .ToListAsync();
    }
}