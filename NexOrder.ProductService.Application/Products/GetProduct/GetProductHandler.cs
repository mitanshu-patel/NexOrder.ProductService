using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexOrder.ProductService.Application.Common;
using NexOrder.ProductService.Application.Products.GetProduct.DTOs;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.GetProduct
{
    public class GetProductHandler : RequestHandlerBase<GetProductQuery, CustomResponse<GetProductResult>>
    {
        private readonly IProductRepo productRepo;
        private readonly ILogger<GetProductHandler> logger;

        public GetProductHandler(IProductRepo productRepo, ILogger<GetProductHandler> logger)
        {
            this.productRepo = productRepo;
            this.logger = logger;
        }

        protected async override Task<CustomResponse<GetProductResult>> ExecuteCommandAsync(GetProductQuery command)
        {
            try
            {
                this.logger.LogInformation("GetProductHandler: ExecuteCommandAsync execution started");
                var productDetail = await this.productRepo.GetProducts()
                                .Where(v => v.Id == command.Id)
                                .Select(v => new ProductDetailDto(v.Id, v.Name, v.Description, v.Price))
                                .FirstOrDefaultAsync();

                if (productDetail == null)
                {
                    this.logger.LogError("Product details for Id:{productId} not found", command.Id);
                    return CustomHttpResult.NotFound<GetProductResult>("Product not found");
                }

                this.logger.LogInformation("GetProductHandler: ExecuteCommandAsync execution completed and fetched details");

                return CustomHttpResult.Ok(new GetProductResult(productDetail));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetProductHandler: Error occurred while getting product with Id {ProductId} with message:{message}", command.Id, ex.Message);
                throw;
            }
        }

        protected override CustomResponse<GetProductResult> GetValidationFailedResult(ValidationResult validationResult)
        {
            return validationResult.GetValidationResult<GetProductResult>();
        }

        protected override IValidator<GetProductQuery> GetValidator()
        {
            var validator = new InlineValidator<GetProductQuery>();
            validator.RuleFor(v=>v.Id).GreaterThan(0).WithMessage("Product Id must be greater than zero.");
            return validator;
        }
    }
}
