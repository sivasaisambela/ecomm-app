using OrderProcessor.Function.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessor.Function.Services
{
    public interface IOrderEventProcessor
    {
        Task ProcessAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default);
    }
}
