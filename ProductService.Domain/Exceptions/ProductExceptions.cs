using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Core.Exceptions;

namespace ProductService.Domain.Exceptions
{
    /// <summary>
    /// Thrown when a product is not found
    /// Returns HTTP 404
    /// </summary>
    public class ProductNotFoundException : ResourceNotFoundException
    {
        public ProductNotFoundException(string identifier)
            : base("Product", identifier)
        {
        }

        public ProductNotFoundException(Guid productId)
            : base("Product", productId.ToString())
        {
        }
    }

    /// <summary>
    /// Thrown when attempting to create a product with a duplicate SKU
    /// Returns HTTP 400
    /// </summary>
    public class DuplicateSkuException : BusinessRuleException
    {
        public DuplicateSkuException(string sku)
            : base(
                $"Product with SKU '{sku}' already exists.",
                "DUPLICATE_SKU"
            )
        {
        }
    }

    /// <summary>
    /// Thrown when there is insufficient stock to fulfill an operation
    /// Returns HTTP 400
    /// </summary>
    public class InsufficientStockException : BusinessRuleException
    {
        public InsufficientStockException(string productName, int available, int requested)
            : base(
                $"Insufficient stock for product '{productName}'. Available: {available}, Requested: {requested}",
                "INSUFFICIENT_STOCK"
            )
        {
        }
    }

    /// <summary>
    /// Thrown when product is in invalid state for an operation
    /// Returns HTTP 400
    /// </summary>
    public class InvalidProductStateException : BusinessRuleException
    {
        public InvalidProductStateException(string message)
            : base(message, "INVALID_PRODUCT_STATE")
        {
        }
    }
}
