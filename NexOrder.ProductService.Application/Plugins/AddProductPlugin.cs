using Microsoft.SemanticKernel;
using NexOrder.Framework.Core.Common;
using NexOrder.Framework.Core.Contracts;
using NexOrder.ProductService.Application.Products.AddProduct;
using NexOrder.ProductService.Application.Products.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Plugins
{
    [Description("Adds product information to the product database.")]
    public class AddProductPlugin
    {
        private readonly IProductRepo productRepo;
        private readonly IMediator mediator;
       
        public AddProductPlugin(IProductRepo productRepo, IMediator mediator)
        {
            this.productRepo = productRepo;
            this.mediator = mediator;
        }

        [KernelFunction("add-product")]
        [Description("Add product based on a user input. " +
            "The input is a product details. Do not infer any data, stick to the details provided by user. " +
            "If any detail is missing then use appropriate default values. " +
            "For example if price is not mentioned then value should be 0, if name is not mentioned then value should be an empty string." +
            "Return the output of add operation, don't retry if it fails.")]
        [return: Description("The result of the add operation.")]
        public async Task<CustomResponse<AddProductResult>> AddNewProduct(AddProductCommand command)
        {
            var result = await this.mediator.SendAsync<AddProductCommand, CustomResponse<AddProductResult>>(command);
            return result;
        }
    }
}
