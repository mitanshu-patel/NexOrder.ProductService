using Microsoft.Extensions.DependencyInjection;
using NexOrder.ProductService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Registrations
{
    public static class HandlerRegistration
    {
        public static void RegisterHandlers(this IServiceCollection services)
        {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Any(IsHandlerInterface))
            .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(IsHandlerInterface);

                foreach (var @interface in interfaces)
                {
                    services.AddTransient(@interface, handlerType);
                }
            }
        }

        private static bool IsHandlerInterface(Type interfaceType)
        {
            var isCommandHandler = interfaceType.IsGenericType &&
                                    interfaceType.GetGenericTypeDefinition() == typeof(IHandler<,>);

            return isCommandHandler;
        }
    }
}
