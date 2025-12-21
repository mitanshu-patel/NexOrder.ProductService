using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Products.AddProduct;
using NexOrder.ProductService.Application.Products.GetProduct;
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
    public async Task<IActionResult> AddUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/products")] HttpRequest req)
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
}