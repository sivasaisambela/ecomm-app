using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    /// <summary>
    /// Application service for product operations
    /// 
    /// Responsibilities:
    /// - Orchestrate business logic
    /// - Validate inputs
    /// - Call repositories
    /// - Handle transactions
    /// - Map between DTOs and entities
    /// 
    /// Why separate from controller?
    /// - Reusable from multiple sources (API, Azure Function, etc.)
    /// - Easier to test
    /// - Business logic in one place
    /// - Controllers stay thin
    /// </summary>
    public interface IProductApplicationService
    {
        // ============================================
        // READ OPERATIONS
        // ============================================

        /// <summary>
        /// Get product by ID
        /// 
        /// Throws: ProductNotFoundException if not found
        /// </summary>
        Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product by SKU
        /// 
        /// Throws: ProductNotFoundException if not found
        /// </summary>
        Task<ProductDto> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all active products
        /// 
        /// Returns: List of all non-deleted, active products
        /// </summary>
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by category
        /// 
        /// Returns: All active products in specified category
        /// Throws: ValidationException if category is empty
        /// </summary>
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products with low stock
        /// 
        /// Returns: Products where AvailableQuantity <= MinimumStockLevel
        /// Used by: Admin dashboard for reordering alerts
        /// </summary>
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get multiple products by IDs
        /// 
        /// Why?
        /// - When order contains multiple products
        /// - More efficient than multiple GetByIdAsync calls
        /// - Single database round trip
        /// </summary>
        Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default);

        // ============================================
        // CREATE OPERATIONS
        // ============================================

        /// <summary>
        /// Create new product
        /// 
        /// Business Rules:
        /// - SKU must be unique
        /// - Price must be > 0
        /// - Stock must be >= 0
        /// 
        /// Throws:
        /// - DuplicateSkuException if SKU already exists
        /// - ValidationException if validation fails
        /// 
        /// Returns: Created product with generated ID
        /// </summary>
        Task<ProductDto> CreateProductAsync(
            CreateProductDto createDto,
            string userId,
            CancellationToken cancellationToken = default);

        // ============================================
        // UPDATE OPERATIONS
        // ============================================

        /// <summary>
        /// Update product information
        /// 
        /// Immutable fields:
        /// - SKU (cannot change)
        /// - ID (obviously)
        /// - CreatedAt, CreatedBy
        /// 
        /// Mutable fields:
        /// - Name, Description, Price, Cost
        /// - MinimumStockLevel, Category, IsActive
        /// 
        /// Throws:
        /// - ProductNotFoundException if not found
        /// - ConcurrencyException if RowVersion mismatch
        /// </summary>
        Task<ProductDto> UpdateProductAsync(
            UpdateProductDto updateDto,
            string userId,
            CancellationToken cancellationToken = default);

        // ============================================
        // DELETE OPERATIONS
        // ============================================

        /// <summary>
        /// Delete product (soft delete)
        /// 
        /// Why soft delete?
        /// - Preserves audit trail
        /// - Can recover if needed
        /// - Maintains referential integrity
        /// - Historical data stays intact
        /// 
        /// Throws: ProductNotFoundException if not found
        /// </summary>
        Task DeleteProductAsync(Guid id, string userId, CancellationToken cancellationToken = default);

        // ============================================
        // STOCK OPERATIONS
        // ============================================

        /// <summary>
        /// Reserve stock for pending order
        /// 
        /// Business Rules:
        /// - Quantity must be positive
        /// - Must have available stock
        /// 
        /// State Change:
        /// - ReservedQuantity += quantity
        /// - AvailableQuantity -= quantity
        /// 
        /// Called when: Order created (before payment)
        /// 
        /// Throws:
        /// - ProductNotFoundException if not found
        /// - InsufficientStockException if not enough stock
        /// </summary>
        Task ReserveStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Release reserved stock
        /// 
        /// Called when: Order cancelled
        /// 
        /// State Change:
        /// - ReservedQuantity -= quantity
        /// - AvailableQuantity += quantity
        /// </summary>
        Task ReleaseReservedStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirm reserved stock (convert to actual sale)
        /// 
        /// Called when: Order payment confirmed
        /// 
        /// State Change:
        /// - StockQuantity -= quantity
        /// - ReservedQuantity -= quantity
        /// - AvailableQuantity unchanged (already was reserved)
        /// </summary>
        Task ConfirmReservedStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Add stock (e.g., new inventory arrives)
        /// </summary>
        Task AddStockAsync(
            Guid productId,
            int quantity,
            string userId,
            CancellationToken cancellationToken = default);

        // ============================================
        // UTILITY OPERATIONS
        // ============================================

        /// <summary>
        /// Check if product can be ordered
        /// 
        /// Returns true if:
        /// - Product exists
        /// - Product is active
        /// - Product is not deleted
        /// - Product has available stock
        /// </summary>
        Task<bool> CanBeOrderedAsync(Guid productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product profit information
        /// 
        /// Returns:
        /// - Cost price
        /// - Selling price
        /// - Profit per unit
        /// - Profit margin percentage
        /// 
        /// Used by: Admin for analytics
        /// </summary>
        Task<(decimal Cost, decimal Price, decimal ProfitPerUnit, decimal ProfitMarginPercentage)>
            GetProfitInfoAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
