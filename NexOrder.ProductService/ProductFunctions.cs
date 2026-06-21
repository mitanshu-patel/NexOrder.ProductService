using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using NexOrder.ProductService.Application.Products.AddProduct;
using NexOrder.ProductService.Application.Products.Common;
using NexOrder.ProductService.Application.Products.Common.DTOs;
using NexOrder.ProductService.Application.Products.DeleteProduct;
using NexOrder.ProductService.Application.Products.GetProduct;
using NexOrder.ProductService.Application.Products.QuickAddProduct;
using NexOrder.ProductService.Application.Products.ResyncProducts;
using NexOrder.ProductService.Application.Products.SearchProducts;
using NexOrder.ProductService.Application.Products.UpdateProduct;
using System.Net;
using System.Text.Json;

namespace NexOrder.ProductService;

public class ProductFunctions
{
    private readonly ILogger<ProductFunctions> _logger;
    private readonly IMediator mediator;

    private readonly Kernel kernel;

    private readonly IChatCompletionService chatCompletionService;

    public ProductFunctions(ILogger<ProductFunctions> logger, IMediator mediator, Kernel kernel, IChatCompletionService chatCompletionService)
    {
        _logger = logger;
        this.mediator = mediator;
        this.kernel = kernel;
        this.chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    [Function("AddProduct")]
    [OpenApiOperation(operationId: "AddProduct", tags: new[] { "AddProduct" }, Description = "Add new product.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AddProductCommand))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddProductResult))]
    public async Task<IActionResult> AddProduct([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/products")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<AddProductCommand>(requestBody);
        var result = await this.mediator.SendAsync<AddProductCommand, CustomResponse<AddProductResult>>(data);
        return result.GetResponse();
    }

    [Function("QuickAddProduct")]
    [OpenApiOperation(operationId: "QuickAddProduct", tags: new[] { "QuickAddProduct" }, Description = "Add new product quickly.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(QuickAddProductCommand))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddProductResult))]
    public async Task<IActionResult> QuickAddProduct([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/products/quick-add")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<QuickAddProductCommand>(requestBody);
        var result = await this.mediator.SendAsync<QuickAddProductCommand, CustomResponse<AddProductResult>>(data);
        return result.GetResponse();
    }

    [Function("GetProduct")]
    [OpenApiOperation(operationId: "GetProduct", tags: new[] { "GetProduct" }, Description = "Get product details for given product id.")]
    [OpenApiParameter(name: "productId", Type = typeof(int), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetProductResult))]
    public async Task<IActionResult> GetProduct([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/products/{productId:int}")] HttpRequest req, int productId)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = new GetProductQuery(productId);
        var result = await this.mediator.SendAsync<GetProductQuery, CustomResponse<GetProductResult>>(command);
        return result.GetResponse();
    }

    [Function("ResyncProducts")]
    [OpenApiOperation(operationId: "ResyncProducts", tags: new[] { "ResyncProducts" }, Description = "Resync product details to other subscribing services")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ResyncProductsResult))]
    public async Task<IActionResult> ResyncProducts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/products/resync")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = new ResyncProductsCommand();
        var result = await this.mediator.SendAsync<ResyncProductsCommand, CustomResponse<ResyncProductsResult>>(command);
        return result.GetResponse();
    }

    [Function("DeleteProduct")]
    [OpenApiOperation(operationId: "DeleteProduct", tags: new[] { "DeleteProduct" }, Description = "Delete product details of given product id.")]
    [OpenApiParameter(name: "productId", Type = typeof(int), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DeleteProductResult))]
    public async Task<IActionResult> DeleteProduct([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/products/{productId:int}")] HttpRequest req, int productId)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = new DeleteProductCommand(productId);
        var result = await this.mediator.SendAsync<DeleteProductCommand, CustomResponse<DeleteProductResult>>(command);
        return result.GetResponse();
    }

    [Function("SearchProducts")]
    [OpenApiOperation(operationId: "SearchProducts", tags: new[] { "SearchProducts" }, Description = "Search products for given criteria with pagination.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SearchProductsQuery))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SearchProductsResult))]
    public async Task<IActionResult> SearchProducts([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/products/search")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<SearchProductsQuery>(requestBody);
        var result = await this.mediator.SendAsync<SearchProductsQuery, CustomResponse<SearchProductsResult>>(data);
        return result.GetResponse();
    }

    [Function("QuickSearchProducts")]
    [OpenApiOperation(operationId: "QuickSearchProducts", tags: new[] { "QuickSearchProducts" }, Description = "Quick search products for given criteria.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(QuickSearchProductsQuery))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(QuickSearchProductsResult))]
    public async Task<IActionResult> QuickSearchProducts([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/products/quick-search")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<QuickSearchProductsQuery>(requestBody);
        var result = await this.mediator.SendAsync<QuickSearchProductsQuery, CustomResponse<QuickSearchProductsResult>>(data);
        return result.GetResponse();
    }

    [Function("UpdateProduct")]
    [OpenApiOperation(operationId: "UpdateProduct", tags: new[] { "UpdateProduct" }, Description = "Update product details for given product id.")]
    [OpenApiParameter(name: "productId", Type = typeof(int), Required = true)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ProductCriteria))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateProductResult))]
    public async Task<IActionResult> UpdateProduct([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/products/{productId:int}")] HttpRequest req, int productId)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<ProductCriteria>(requestBody);
        var command = new UpdateProductCommand(productId, data);
        var result = await this.mediator.SendAsync<UpdateProductCommand, CustomResponse<UpdateProductResult>>(command);
        return result.GetResponse();
    }
}