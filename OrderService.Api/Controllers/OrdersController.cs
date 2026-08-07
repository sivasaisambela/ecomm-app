using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using Shared.Core.Responses;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderApplicationService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderApplicationService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Place a new order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto createDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HTTP POST: api/v1/orders - Creating order for customer: {CustomerId}", createDto.CustomerId);

            var order = await _orderService.CreateOrderAsync(createDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                new ApiResponse<OrderDto>(true, order, "Order created successfully.")
            );
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HTTP GET: api/v1/orders/{id} - Fetching order", id);

            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            return Ok(new ApiResponse<OrderDto>(true, order));
        }

        /// <summary>
        /// Get all orders for a specific customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCustomerId(string customerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HTTP GET: api/v1/orders/customer/{customerId} - Fetching orders", customerId);

            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId, cancellationToken);
            return Ok(new ApiResponse<IEnumerable<OrderDto>>(true, orders));
        }
    }

    // A standard helper wrapper for unified JSON responses across our microservices
  
}
