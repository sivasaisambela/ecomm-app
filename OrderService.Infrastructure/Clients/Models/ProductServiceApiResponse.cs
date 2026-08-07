using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Clients.Models
{
    public class ProductServiceApiResponse
    {
        public bool Success { get; set; }
        public ProductDetailsResponse? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
