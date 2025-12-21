using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Domain.Entities;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.AddProduct
{
    public class AddProductHandler : RequestHandlerBase<AddProductCommand, CustomResponse<AddProductResult>>
    {
        private readonly IProductRepo productRepo;
        private readonly ILogger<AddProductHandler> logger;

        public AddProductHandler(IProductRepo productRepo, ILogger<AddProductHandler> logger)
        {
            this.logger = logger;
            this.productRepo = productRepo;
        }

        protected async override Task<CustomResponse<AddProductResult>> ExecuteCommandAsync(AddProductCommand command)
        {
            try
            {
                this.logger.LogInformation("AddProductHandler: ExecuteCommandAsync execution started");
                var productExists = await this.productRepo.GetProducts()
                    .AnyAsync(u => u.Name == command.Name);

                if (productExists)
                {
                    this.logger.LogError("Product with name :{name} already exists.", command.Name);
                    return CustomHttpResult.BadRequest<AddProductResult>("Product with the same name already exists.");
                }

                var product = new Product
                {
                    Name = command.Name,
                    Price = command.Price,
                    Description = command.Description,
                    CreatedAtUtc = DateTime.UtcNow,
                };

                await this.productRepo.AddProductAsync(product);

                this.logger.LogInformation("AddProductHandler: ExecuteCommandAsync execution successfully with Id:{productId}", product.Id);

                return CustomHttpResult.Ok(new AddProductResult(product.Id));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "AddProductHandler: exception occurred with message:{message}", ex.Message);
                throw;
            }
        }

        protected override CustomResponse<AddProductResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<AddProductResult>();
        }

        protected override IValidator<AddProductCommand> GetValidator()
        {
           var validator = new InlineValidator<AddProductCommand>();
           validator.RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
           validator.RuleFor(v => v.Description).NotEmpty().MaximumLength(200);
           validator.RuleFor(v=>v.Price).GreaterThan(0);
           return validator;
        }
    }
}
