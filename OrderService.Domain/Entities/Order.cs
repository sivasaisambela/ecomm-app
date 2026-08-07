using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public string CustomerId { get; private set; } = string.Empty;
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }

        // Private field backing property protects our list from outer modifications (.Add() directly)
        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        private Order() { } // EF Core requirement

        public Order(Guid id, string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new OrderDomainException("Customer ID cannot be empty.");

            Id = id;
            CustomerId = customerId.Trim();
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
            TotalAmount = 0;
        }

        // Business Methods

        public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
        {
            if (Status != OrderStatus.Pending)
                throw new OrderDomainException("Cannot modify items after an order is placed and processed.");

            var existingItem = _orderItems.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                // If item already exists, we replace it with updated quantity
                _orderItems.Remove(existingItem);
                _orderItems.Add(new OrderItem(existingItem.Id, productId, productName, unitPrice, existingItem.Quantity + quantity));
            }
            else
            {
                _orderItems.Add(new OrderItem(Guid.NewGuid(), productId, productName, unitPrice, quantity));
            }

            RecalculateTotal();
        }

        public void TransitionToPaid()
        {
            if (Status != OrderStatus.Pending)
                throw new OrderDomainException($"Cannot pay for order. Current status: {Status}");

            Status = OrderStatus.Paid;
        }

        public void TransitionToShipped()
        {
            if (Status != OrderStatus.Paid)
                throw new OrderDomainException($"Cannot ship unpaid or already closed order. Current status: {Status}");

            Status = OrderStatus.Shipped;
        }

        public void TransitionToCompleted()
        {
            if (Status != OrderStatus.Shipped)
                throw new OrderDomainException($"Cannot complete order that hasn't been shipped. Current status: {Status}");

            Status = OrderStatus.Completed;
        }

        public void CancelOrder(string reason)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Shipped)
                throw new OrderDomainException("Cannot cancel orders that are already shipped or completed.");

            Status = OrderStatus.Cancelled;
            _loggerDomainNote = $"Cancelled: {reason}"; // Storing cancellation logic
        }

        private string? _loggerDomainNote;

        private void RecalculateTotal()
        {
            TotalAmount = _orderItems.Sum(item => item.TotalPrice);
        }
    }
}
