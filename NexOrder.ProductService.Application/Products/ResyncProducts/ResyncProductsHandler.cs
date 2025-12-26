using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Messages.Events;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.ResyncProducts
{
    public class ResyncProductsHandler : RequestHandlerBase<ResyncProductsCommand, CustomResponse<ResyncProductsResult>>
    {
        private readonly IProductRepo productRepo;

        private readonly ILogger<ResyncProductsHandler> logger;

        private readonly IMessageDeliveryService messageDeliveryService;

        public ResyncProductsHandler(IProductRepo productRepo, IMessageDeliveryService messageDeliveryService, ILogger<ResyncProductsHandler> logger)
        {
            this.logger = logger;
            this.productRepo = productRepo;
            this.messageDeliveryService = messageDeliveryService;
        }
        protected override async Task<CustomResponse<ResyncProductsResult>> ExecuteCommandAsync(ResyncProductsCommand command)
        {
            try
            {
                this.logger.LogDebug("Starting resync of products.");
                var products = await this.productRepo.GetProducts().Select(v => new ProductUpdated(v.Id, v.Name, v.Description, v.Price)).ToListAsync();

                foreach(var product in products)
                {
                    await this.messageDeliveryService.PublishMessageAsync(product, ProductsTopic.TopicName);
                }

                this.logger.LogDebug("Resync of products completed. Total products resynced: {SyncCount}", products.Count);
                return CustomHttpResult.Ok(new ResyncProductsResult(products.Count));
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while resyncing products.");
                throw;
            }
        }

        protected override CustomResponse<ResyncProductsResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<ResyncProductsResult>();
        }

        protected override IValidator<ResyncProductsCommand> GetValidator()
        {
            return new InlineValidator<ResyncProductsCommand>();
        }
    }
}
