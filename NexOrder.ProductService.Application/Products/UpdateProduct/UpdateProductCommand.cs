using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.UpdateProduct
{
    public record UpdateProductCommand(int ProductId, ProductCriteria Criteria);
}
