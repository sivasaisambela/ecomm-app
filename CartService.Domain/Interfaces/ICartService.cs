using CartService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Interfaces
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(string customerId, CancellationToken cancellationToken = default);
        Task<Cart> AddItemAsync(string customerId, Guid productId, string productName, decimal unitPrice, int quantity, CancellationToken cancellationToken = default);
        Task<Cart> UpdateItemQuantityAsync(string customerId, Guid productId, int quantity, CancellationToken cancellationToken = default);
        Task<Cart> RemoveItemAsync(string customerId, Guid productId, CancellationToken cancellationToken = default);
        Task ClearCartAsync(string customerId, CancellationToken cancellationToken = default);
    }
}
