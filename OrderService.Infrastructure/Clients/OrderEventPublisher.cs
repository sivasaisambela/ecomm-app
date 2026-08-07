using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using System.Text.Json;

namespace OrderService.Infrastructure.Clients
{
    public class OrderEventPublisher : IOrderEventPublisher
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<OrderEventPublisher> _logger;

        public OrderEventPublisher(string connectionString, string queueName, ILogger<OrderEventPublisher> logger)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default)
        {
            var message = JsonSerializer.Serialize(orderEvent);
            var base64Message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message));

            await _queueClient.SendMessageAsync(base64Message, cancellationToken: cancellationToken);

            _logger.LogInformation("Published OrderCreated event for OrderId: {OrderId}", orderEvent.OrderId);
        }
    }
}