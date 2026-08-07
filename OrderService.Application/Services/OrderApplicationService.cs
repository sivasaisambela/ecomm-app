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
using FluentValidation;
using ValidationException = Shared.Core.Exceptions.ValidationException;
using OrderService.Domain.Enums;

namespace OrderService.Application.Services
{
    public class OrderApplicationService : IOrderApplicationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IOrderEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOrderDto> _createOrderValidator;
        private readonly IValidator<UpdateOrderStatusDto> _updateStatusValidator; // Added!

        public OrderApplicationService(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
             IOrderEventPublisher eventPublisher,
            IMapper mapper,
            IValidator<CreateOrderDto> createOrderValidator,
            IValidator<UpdateOrderStatusDto> updateStatusValidator)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));   // ADD THIS
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _createOrderValidator = createOrderValidator ?? throw new ArgumentNullException(nameof(createOrderValidator));
            _updateStatusValidator = updateStatusValidator;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createDto, CancellationToken cancellationToken = default)
        {
            // 1. Validate input using FluentValidation
            var validationResult = await _createOrderValidator.ValidateAsync(createDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(errors);
            }

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

            // 4. Publish OrderCreated event so async processing (payment, stock confirm, email) can begin
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(i => new OrderCreatedEventItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _eventPublisher.PublishOrderCreatedAsync(orderCreatedEvent, cancellationToken);

            // 5. Return the mapped DTO
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

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto request, CancellationToken cancellationToken = default)
        {
            // 1. Validate input shape first (empty/garbage status caught here)
            var validationResult = await _updateStatusValidator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(errors);
            }

            // 2. Fetch the order
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
                ?? throw new OrderNotFoundException(orderId);

            // 3. Parse the incoming string into our OrderStatus enum (safe now — already validated above)
            Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var targetStatus);

            // 4. Delegate to the correct domain transition method based on target status.
            switch (targetStatus)
            {
                case OrderStatus.Paid:
                    order.TransitionToPaid();
                    break;

                case OrderStatus.Shipped:
                    order.TransitionToShipped();
                    break;

                case OrderStatus.Completed:
                    order.TransitionToCompleted();
                    break;

                case OrderStatus.Cancelled:
                    order.CancelOrder("Cancelled by admin.");
                    break;

                default:
                    throw new ValidationException(new Dictionary<string, string[]>
            {
                { "status", new[] { $"Status '{request.Status}' cannot be set directly via this endpoint." } }
            });
            }

            // 5. Persist changes
            _orderRepository.Update(order);

            var result = await _orderRepository.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new OrderDomainException("An error occurred while updating the order status.");
            }

            // 6. Return mapped DTO
            return _mapper.Map<OrderDto>(order);
        }
    }
}
