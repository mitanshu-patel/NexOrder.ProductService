using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Common
{
    public interface IMediator
    {
        Task<TResult> SendAsync<TCommand, TResult>(TCommand command) where TCommand : class;
    }
}
