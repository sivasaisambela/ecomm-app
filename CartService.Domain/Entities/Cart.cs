using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; private set; }
        public string CustomerId { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        private Cart() { }

        public Cart(Guid id, string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

            Id = id;
            CustomerId = customerId.Trim();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddItem(CartItem item)
        {
            var existingItem = _items.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existingItem is not null)
            {
                _items.Remove(existingItem);
                _items.Add(new CartItem(Guid.NewGuid(), item.ProductId, item.ProductName, item.UnitPrice, existingItem.Quantity + item.Quantity));
            }
            else
            {
                _items.Add(item);
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItemQuantity(Guid productId, int quantity)
        {
            var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem is null)
                return;

            _items.Remove(existingItem);

            if (quantity > 0)
            {
                _items.Add(new CartItem(Guid.NewGuid(), existingItem.ProductId, existingItem.ProductName, existingItem.UnitPrice, quantity));
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(Guid productId)
        {
            var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem is null)
                return;

            _items.Remove(existingItem);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Clear()
        {
            _items.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
