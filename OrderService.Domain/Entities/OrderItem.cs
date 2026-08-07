using OrderService.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        private OrderItem() { } // EF Core materialization constructor

        public OrderItem(Guid id, Guid productId, string productName, decimal unitPrice, int quantity)
        {
            if (productId == Guid.Empty)
                throw new OrderDomainException("Product ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(productName))
                throw new OrderDomainException("Product name cannot be empty.");

            if (unitPrice < 0)
                throw new OrderDomainException("Unit price cannot be negative.");

            if (quantity <= 0)
                throw new OrderDomainException("Quantity must be greater than zero.");

            Id = id;
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        // Business behaviors
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
