using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Products.AddProduct
{
    public record AddProductCommand(string Name, string Description, decimal Price);
}
