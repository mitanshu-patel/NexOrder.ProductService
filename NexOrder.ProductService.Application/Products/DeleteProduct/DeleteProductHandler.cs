using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Messages.Commands;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.DeleteProduct
{
    public class DeleteProductHandler : RequestHandlerBase<DeleteProductCommand, CustomResponse<DeleteProductResult>>
    {
        private readonly IProductRepo productRepo;
        private readonly ILogger<DeleteProductHandler> logger;
        private readonly IMessageDeliveryService messageDeliveryService;

        public DeleteProductHandler(IProductRepo productRepo, ILogger<DeleteProductHandler> logger, IMessageDeliveryService messageDeliveryService)
        {
            this.logger = logger;
            this.productRepo = productRepo;
            this.messageDeliveryService = messageDeliveryService;
        }

        protected async override Task<CustomResponse<DeleteProductResult>> ExecuteCommandAsync(DeleteProductCommand command)
        {
            try
            {
                this.logger.LogInformation("DeleteProductHandler: ExecuteCommandAsync execution started");
                var productDetail = await productRepo.GetProducts().Where(v => v.Id == command.Id).FirstOrDefaultAsync();

                if (productDetail == null)
                {
                    this.logger.LogError("Product details for Id:{productId} not found", command.Id);
                    return CustomHttpResult.NotFound<DeleteProductResult>("Product not found");
                }

                productDetail.IsDeleted = true;

                await this.productRepo.UpdateProductAsync(productDetail);
                await this.messageDeliveryService.PublishMessageAsync(new UpdateProductsCache(), ProductServiceCommand.QueueName);

                this.logger.LogInformation("DeleteProductHandler: ExecuteCommandAsync execution completed and deleted product");

                return CustomHttpResult.Ok(new DeleteProductResult());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DeleteProductHandler: exception occurred with message:{message}", ex.Message);
                throw;
            }
        }

        protected override CustomResponse<DeleteProductResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<DeleteProductResult>();
        }

        protected override IValidator<DeleteProductCommand> GetValidator()
        {
            var validator = new InlineValidator<DeleteProductCommand>();
            validator.RuleFor(v=>v.Id).GreaterThan(0).WithMessage("Product Id must be greater than zero");
            return validator;
        }
    }
}
