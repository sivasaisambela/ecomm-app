using Microsoft.Extensions.Logging;
using OrderProcessor.Function.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessor.Function.Services
{
    public class OrderEventProcessor : IOrderEventProcessor
    {
        private readonly ILogger<OrderEventProcessor> _logger;

        public OrderEventProcessor(ILogger<OrderEventProcessor> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing order {OrderId} for customer {CustomerId}",
                orderEvent.OrderId,
                orderEvent.CustomerId);

            return Task.CompletedTask;
        }
    }
}
