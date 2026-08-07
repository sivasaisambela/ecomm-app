using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,      // Order placed, waiting for stock reservation/payment
        Paid = 2,         // Payment processed successfully
        Shipped = 3,      // Dispatched from warehouse
        Completed = 4,    // Delivered to customer
        Cancelled = 5,    // Cancelled (stock released if previously reserved)
        Refunded = 6      // Returned and money refunded
    }
}
