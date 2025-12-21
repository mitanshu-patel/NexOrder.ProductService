using NexOrder.ProductService.Application.Products.GetProduct.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.GetProduct
{
    public record GetProductResult(ProductDetailDto ProductDetail);
}
