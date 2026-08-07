using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Services
{
    // A simple model representing the product data we need from the Product Service
    public record ProductValidationResult(string Name, decimal Price, bool IsAvailable);

    public interface IProductServiceClient
    {
        // Fetches name, price, and availability check from the Product Service
        Task<ProductValidationResult?> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default);

        // Tells the Product Service to reserve stock for a purchase
        Task<bool> ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    }
}
