using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OrderService.Application.Services;
using OrderService.Infrastructure.Clients.Models;

namespace OrderService.Infrastructure.Clients
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProductServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ProductValidationResult?> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Call GET /api/v1/Products/{id} on the Product Service API
                var response = await _httpClient.GetAsync($"api/v1/Products/{productId}", cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                // Deserialize using our newly separated external model class
                var apiResult = await response.Content.ReadFromJsonAsync<ProductServiceApiResponse>(cancellationToken: cancellationToken);

                if (apiResult?.Data == null)
                    return null;

                // Determine availability based on whether there is unreserved stock remaining
                var isAvailable = apiResult.Data.StockQuantity > apiResult.Data.ReservedQuantity;

                return new ProductValidationResult(apiResult.Data.Name, apiResult.Data.Price, isAvailable);
            }
            catch
            {
                // Fail-safe: Return null to let the Application Service gracefully handle external service unavailability
                return null;
            }
        }

        public async Task<bool> ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                // Send the quantity as a raw integer body to: POST /api/v1/Products/{id}/reserve-stock
                var content = new StringContent(quantity.ToString(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"api/v1/Products/{productId}/reserve-stock", content, cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
