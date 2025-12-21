using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Products.AddProduct;
using NexOrder.ProductService.Application.Products.DeleteProduct;
using NexOrder.ProductService.Application.Products.GetProduct;
using NexOrder.ProductService.Application.Products.SearchProducts;
using NexOrder.ProductService.Application.Products.UpdateProduct;
using NexOrder.ProductService.Shared.Common;
using System.Net;

namespace NexOrder.ProductService;

public class ProductFunctions
{
    private readonly ILogger<ProductFunctions> _logger;
    private readonly IMediator mediator;

    public ProductFunctions(ILogger<ProductFunctions> logger, IMediator mediator)
    {
        _logger = logger;
        this.mediator = mediator;
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