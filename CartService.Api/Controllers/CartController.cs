using CartService.Api.Dtos.Requests;
using CartService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CartService.Api.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetCustomerId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException("Customer ID not found in token.");
        }

        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();
            var cart = await _cartService.GetOrCreateCartAsync(customerId, cancellationToken);
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();
            var cart = await _cartService.AddItemAsync(
                customerId,
                request.ProductId,
                request.ProductName,
                request.UnitPrice,
                request.Quantity,
                cancellationToken);

            return Ok(cart);
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateItemQuantity(Guid productId, [FromBody] UpdateCartItemQuantityRequest request, CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();
            var cart = await _cartService.UpdateItemQuantityAsync(customerId, productId, request.Quantity, cancellationToken);
            return Ok(cart);
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();
            var cart = await _cartService.RemoveItemAsync(customerId, productId, cancellationToken);
            return Ok(cart);
        }

        [HttpDelete]
        public async Task<IActionResult> Clear(CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();
            await _cartService.ClearCartAsync(customerId, cancellationToken);
            return NoContent();
        }
    }
}
