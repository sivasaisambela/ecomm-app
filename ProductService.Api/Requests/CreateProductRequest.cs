namespace ProductService.Api.Requests
{
    /// <summary>
    /// Request model for creating a new product
    /// 
    /// This is what the client sends to the API.
    /// Separate from DTO to allow different validation rules
    /// and to decouple API contract from business logic.
    /// </summary>
    public class CreateProductRequest
    {
        /// <summary>
        /// Product name (required)
        /// Example: "Dell XPS 13 Laptop"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description (optional)
        /// Example: "High-performance ultrabook with Intel i7"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Stock Keeping Unit (required, unique)
        /// Example: "LAPTOP-DELL-XPS13-2024"
        /// Used in warehouse systems
        /// </summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Selling price (required, must be > 0)
        /// Example: 1299.99
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Cost price (required, must be >= 0)
        /// Example: 800.00
        /// Used for profit calculations
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Initial stock quantity (required, must be >= 0)
        /// Example: 50
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Minimum stock level for reordering (required, must be >= 0)
        /// Example: 10
        /// When stock falls below this, admin gets alert
        /// </summary>
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// Product category (required)
        /// Example: "Electronics", "Laptops", "Accessories"
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating a product
    /// </summary>
    public class UpdateProductRequest
    {
        /// <summary>
        /// Product ID (which product to update)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name (can be updated)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description (can be updated)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Selling price (can be updated)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Cost price (can be updated)
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Minimum stock level (can be updated)
        /// </summary>
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// Product category (can be updated)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Is product active (can be updated)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Row version for optimistic locking
        /// Prevents concurrent update conflicts
        /// </summary>
        public byte[]? RowVersion { get; set; }
    }

    /// <summary>
    /// Request model for adding stock
    /// </summary>
    public class AddStockRequest
    {
        /// <summary>
        /// Quantity to add
        /// </summary>
        public int Quantity { get; set; }
    }
}
