using Microsoft.Extensions.Configuration;
using Azure.Messaging.ServiceBus;
using NexOrder.ProductService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NexOrder.ProductService.Infrastructure.Services
{
    public class MessageDeliveryService : IMessageDeliveryService
    {
        public async Task PublishMessageAsync<T>(T requestBody, string topicName)
        {
            var options = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
            };

#if DEBUG
            options.WebProxy = new WebProxy(Environment.GetEnvironmentVariable("WebProxy"), true);
#endif
            var settingsSection = Environment.GetEnvironmentVariable("ConnectionStrings:ServiceBusConnectionString");
            var client = new ServiceBusClient(settingsSection, options);
            var sender = client.CreateSender(topicName);
            var customBody = new
            {
                FullName = requestBody.GetType().FullName,
                Data = requestBody,
            };
            var body = JsonSerializer.Serialize(customBody);
            var message = new ServiceBusMessage(body);
            await sender.SendMessageAsync(message);
        }
    }
}
