using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Services
{
    public class OrderApplicationService : IOrderApplicationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IMapper _mapper;

        public OrderApplicationService(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createDto, CancellationToken cancellationToken = default)
        {
            // 1. Create a new Order entity
            var order = new Order(Guid.NewGuid(), createDto.CustomerId);

            // 2. Process each item requested
            foreach (var item in createDto.Items)
            {
                // Fetch product details from the Product Service
                var productDetails = await _productServiceClient.GetProductDetailsAsync(item.ProductId, cancellationToken);

                if (productDetails == null || !productDetails.IsAvailable)
                {
                    throw new OrderDomainException($"Product {item.ProductId} is not available for purchase.");
                }

                // Attempt to reserve stock in the Product Service
                var stockReserved = await _productServiceClient.ReserveStockAsync(item.ProductId, item.Quantity, cancellationToken);
                if (!stockReserved)
                {
                    throw new OrderDomainException($"Failed to reserve stock for product {productDetails.Name}.");
                }

                // Add the item to our Domain Order (which recalculates the order total)
                order.AddItem(item.ProductId, productDetails.Name, productDetails.Price, item.Quantity);
            }

            // 3. Save the order to our database
            await _orderRepository.AddAsync(order, cancellationToken);

            // Match the Task<bool> signature from IOrderRepository
            var result = await _orderRepository.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new OrderDomainException("An error occurred while saving the order to the database.");
            }

            // 4. Return the mapped DTO
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new OrderNotFoundException(id);

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
    }
}
