using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessor.Function.Contracts
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderCreatedEventItem> Items { get; set; } = new();
    }

    public class OrderCreatedEventItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
