using OrderService.Application.DTOs;

namespace OrderService.Application.Services
{
    public interface IOrderEventPublisher
    {
        Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default);
    }
}