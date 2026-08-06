namespace ProductService.Api.Responses
{
    /// <summary>
    /// Response model for product data
    /// 
    /// This is what the API returns to the client.
    /// Contains all product information in a clean format.
    /// </summary>
    public class ProductResponse
    {
        /// <summary>
        /// Unique product identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Stock Keeping Unit
        /// </summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Selling price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Cost price (only for admin)
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Total stock in warehouse
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Available quantity (can be purchased)
        /// = StockQuantity - ReservedQuantity
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Quantity reserved for pending orders
        /// </summary>
        public int ReservedQuantity { get; set; }

        /// <summary>
        /// Minimum stock level for reordering
        /// </summary>
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Is product active (can be purchased)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is stock level low
        /// </summary>
        public bool IsStockLow { get; set; }

        /// <summary>
        /// Profit per unit
        /// = Price - Cost
        /// </summary>
        public decimal ProfitPerUnit { get; set; }

        /// <summary>
        /// Profit margin percentage
        /// = (Price - Cost) / Price * 100
        /// </summary>
        public decimal ProfitMarginPercentage { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last modification timestamp
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response for product list
    /// </summary>
    public class ProductListResponse
    {
        /// <summary>
        /// Total count of products
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// List of products
        /// </summary>
        public List<ProductResponse> Products { get; set; } = new();
    }

    /// <summary>
    /// Response for low stock products
    /// </summary>
    public class LowStockProductResponse
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Current available quantity
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Minimum stock level
        /// </summary>
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// How many units short
        /// </summary>
        public int ShortageQuantity => MinimumStockLevel - AvailableQuantity;
    }

    /// <summary>
    /// Response for profit information
    /// </summary>
    public class ProfitInfoResponse
    {
        /// <summary>
        /// Cost price
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Selling price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Profit per unit
        /// </summary>
        public decimal ProfitPerUnit { get; set; }

        /// <summary>
        /// Profit margin percentage
        /// </summary>
        public decimal ProfitMarginPercentage { get; set; }
    }
}
