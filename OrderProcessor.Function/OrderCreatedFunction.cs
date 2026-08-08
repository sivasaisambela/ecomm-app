using System;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderProcessor.Function.Contracts;
using OrderProcessor.Function.Services;

namespace OrderProcessor.Function;

public class OrderCreatedFunction
{
    private readonly ILogger<OrderCreatedFunction> _logger;
    private readonly IOrderEventProcessor _processor;

    public OrderCreatedFunction(
        ILogger<OrderCreatedFunction> logger,
        IOrderEventProcessor processor)
    {
        _logger = logger;
        _processor = processor;
    }

    [Function("OrderCreatedFunction")]
    public async Task Run(
        [QueueTrigger("order-created", Connection = "AzureWebJobsStorage")] string queueItem)
    {
        _logger.LogInformation("Queue message received: {Message}", queueItem);

        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(queueItem);

        if (orderEvent == null)
        {
            _logger.LogWarning("Could not deserialize OrderCreatedEvent.");
            return;
        }

        await _processor.ProcessAsync(orderEvent);
    }
}