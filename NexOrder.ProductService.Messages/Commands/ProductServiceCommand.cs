using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Messages.Commands
{
    public static class ProductServiceCommand
    {
        public static string QueueName => "productservicecommands";
    }
}
