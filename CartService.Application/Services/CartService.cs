using CartService.Domain.Entities;
using CartService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CartService.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<Cart> GetOrCreateCartAsync(string customerId, CancellationToken cancellationToken = default)
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);

            if (cart is not null)
                return cart;

            cart = new Cart(Guid.NewGuid(), customerId);
            await _cartRepository.AddAsync(cart, cancellationToken);

            return cart;
        }

        public async Task<Cart> AddItemAsync(string customerId, Guid productId, string productName, decimal unitPrice, int quantity, CancellationToken cancellationToken = default)
        {
            var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
            cart.AddItem(new CartItem(Guid.NewGuid(), productId, productName, unitPrice, quantity));

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            _logger.LogInformation("Added product {ProductId} to cart for customer {CustomerId}", productId, customerId);

            return cart;
        }

        public async Task<Cart> UpdateItemQuantityAsync(string customerId, Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
            cart.UpdateItemQuantity(productId, quantity);

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            return cart;
        }

        public async Task<Cart> RemoveItemAsync(string customerId, Guid productId, CancellationToken cancellationToken = default)
        {
            var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
            cart.RemoveItem(productId);

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            return cart;
        }

        public async Task ClearCartAsync(string customerId, CancellationToken cancellationToken = default)
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            if (cart is null)
                return;

            cart.Clear();
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }
    }
}
