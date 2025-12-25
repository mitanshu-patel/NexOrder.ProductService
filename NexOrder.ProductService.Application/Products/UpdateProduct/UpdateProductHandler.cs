using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Services;
using NexOrder.ProductService.Domain.Entities;
using NexOrder.ProductService.Messages.Events;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.UpdateProduct
{
    public class UpdateProductHandler : RequestHandlerBase<UpdateProductCommand, CustomResponse<UpdateProductResult>>
    {
        private readonly IProductRepo productRepo;
        private readonly ILogger<UpdateProductHandler> logger;
        private readonly IMessageDeliveryService messageDeliveryService;

        public UpdateProductHandler(IProductRepo productRepo, ILogger<UpdateProductHandler> logger, IMessageDeliveryService messageDeliveryService)
        {
            this.productRepo = productRepo;
            this.logger = logger;
            this.messageDeliveryService = messageDeliveryService;
        }
        protected async override Task<CustomResponse<UpdateProductResult>> ExecuteCommandAsync(UpdateProductCommand command)
        {
            try
            {
                this.logger.LogInformation("UpdateProductHandler: ExecuteCommandAsync execution started");
                var productDetail = await productRepo.GetProducts().Where(v => v.Id == command.ProductId).FirstOrDefaultAsync();

                if (productDetail == null)
                {
                    this.logger.LogError("Product details for Id:{productId} not found", command.ProductId);
                    return CustomHttpResult.NotFound<UpdateProductResult>("Product not found");
                }

                productDetail.Name = command.Criteria.Name;
                productDetail.Price = command.Criteria.Price;
                productDetail.Description = command.Criteria.Description;

                await this.productRepo.UpdateProductAsync(productDetail);
                await this.messageDeliveryService.PublishMessageAsync(new ProductUpdated(productDetail.Id, productDetail.Name, productDetail.Description, productDetail.Price), ProductsTopic.TopicName);

                this.logger.LogInformation("UpdateProductHandler: ExecuteCommandAsync execution completed and saved details");

                return CustomHttpResult.Ok(new UpdateProductResult());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UpdateProductHandler: exception occurred with message:{message}", ex.Message);
                throw;
            }
        }

        protected override CustomResponse<UpdateProductResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<UpdateProductResult>();
        }

        protected override IValidator<UpdateProductCommand> GetValidator()
        {
            var validator = new InlineValidator<UpdateProductCommand>();
            validator.RuleFor(v => v.Criteria).NotNull();
            validator.RuleFor(v=>v.ProductId).GreaterThan(0).WithMessage("Product Id must be greater than zero");
            validator.RuleFor(v=>v.Criteria.Name).NotEmpty().WithMessage("Product name must not be empty").MaximumLength(100);
            validator.RuleFor(v=>v.Criteria.Description).NotEmpty().WithMessage("Product description must not be empty").MaximumLength(200);
            validator.RuleFor(v=>v.Criteria.Price).GreaterThan(0).WithMessage("Product price must be greater than zero");
            return validator;
        }
    }
}
