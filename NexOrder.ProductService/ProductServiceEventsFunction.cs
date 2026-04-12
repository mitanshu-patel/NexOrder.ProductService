using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using NexOrder.ProductService.Application.ProductEvents.UpdateProductsCache;
using NexOrder.ProductService.Messages.Commands;
using System.Text.Json;

namespace NexOrder.ProductService
{
    public class ProductServiceEventsFunction
    {
        private readonly ILogger<ProductServiceEventsFunction> _logger;

        private readonly IMediator mediator;

        public ProductServiceEventsFunction(IMediator mediator, ILogger<ProductServiceEventsFunction> _logger)
        {
            this.mediator = mediator;
            this._logger = _logger;
        }

        [Function("ProductServiceEventsFunction")]
        public async Task Run([ServiceBusTrigger("productservicecommands", Connection = "ServiceBusConnectionString")] string mySbMsg)
        {
            var response = JsonSerializer.Deserialize<MessageResult>(mySbMsg);
            if (response.FullName == typeof(UpdateProductsCache).FullName)
            {
                await this.mediator.SendAsync<UpdateProductsCacheCommand, CustomResponse<UpdateProductsCacheResult>>(new UpdateProductsCacheCommand());
                this._logger.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
            }
        }
    }
}
