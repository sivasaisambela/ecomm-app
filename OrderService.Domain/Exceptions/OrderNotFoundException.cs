using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Exceptions
{
    public class OrderNotFoundException : OrderDomainException
    {
        public OrderNotFoundException(Guid orderId) : base($"Order with ID {orderId} was not found.") { }
    }
}
