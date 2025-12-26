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
        private string _connectionString;

        private readonly IConfiguration configuration;

        public MessageDeliveryService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task PublishMessageAsync<T>(T requestBody, string topicName)
        {
            var options = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
            };

#if DEBUG
            options.WebProxy = new WebProxy(Environment.GetEnvironmentVariable("WebProxy"), true);
#endif
            this._connectionString = this.configuration.GetConnectionString("ServiceBusConnectionString") ?? string.Empty;
            var client = new ServiceBusClient(this._connectionString, options);
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
