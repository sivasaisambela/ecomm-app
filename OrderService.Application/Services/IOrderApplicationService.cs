using OrderService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Services
{
    public interface IOrderApplicationService
    {
        // Places a new order
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createDto, CancellationToken cancellationToken = default);

        // Retrieves an order by its ID
        Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Retrieves all orders for a specific customer
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto request, CancellationToken cancellationToken = default);
    }
}
