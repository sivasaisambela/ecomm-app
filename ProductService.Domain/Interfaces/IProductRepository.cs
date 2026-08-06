using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Product persistence
    /// 
    /// This interface defines the contract for data access operations.
    /// Implementation can use SQL Server, MongoDB, In-Memory, etc.
    /// 
    /// Why interface?
    /// - Allows multiple implementations (SQL, NoSQL, In-Memory for testing)
    /// - Decouples domain from infrastructure
    /// - Makes unit testing easier (mock the repository)
    /// - Follows Dependency Inversion Principle
    /// </summary>
    public interface IProductRepository
    {
        // ============================================
        // READ OPERATIONS
        // ============================================

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Product if found, null otherwise</returns>
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get product by SKU
        /// 
        /// Why separate method?
        /// - SKU is unique identifier in warehouse systems
        /// - Common query in e-commerce
        /// - More efficient than GetAll() + filter in memory
        /// </summary>
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all active products
        /// </summary>
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by category
        /// 
        /// Why?
        /// - Common filtering in product listing
        /// - Better performance than GetAll() + filter in memory
        /// - Reduces data transfer
        /// </summary>
        Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products with low stock
        /// 
        /// Why?
        /// - Admin needs to know when to reorder
        /// - Prevents stockouts
        /// - Critical for supply chain management
        /// </summary>
        Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get products by multiple IDs
        /// 
        /// Why?
        /// - When order contains multiple products
        /// - More efficient than multiple GetByIdAsync calls
        /// - Single database round trip
        /// </summary>
        Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

        // ============================================
        // WRITE OPERATIONS
        // ============================================

        /// <summary>
        /// Add new product
        /// </summary>
        Task AddAsync(Product product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add multiple products
        /// </summary>
        Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update existing product
        /// </summary>
        Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update multiple products
        /// </summary>
        Task UpdateRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete product (soft delete)
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete multiple products (soft delete)
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

        // ============================================
        // EXISTENCE CHECKS
        // ============================================

        /// <summary>
        /// Check if product exists
        /// 
        /// Why separate method?
        /// - Faster than GetByIdAsync if you only need to check existence
        /// - Used before operations like update/delete
        /// - Returns bool instead of object
        /// </summary>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if SKU exists
        /// 
        /// Why?
        /// - Prevent duplicate SKUs
        /// - Validation before creating product
        /// - Faster than GetBySkuAsync
        /// </summary>
        Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if SKU exists (excluding specific product)
        /// 
        /// Why?
        /// - When updating product, SKU might be same
        /// - Don't want to reject update because SKU "already exists"
        /// - Allows same SKU for the same product
        /// </summary>
        Task<bool> SkuExistsAsync(string sku, Guid excludeProductId, CancellationToken cancellationToken = default);

        // ============================================
        // COUNTING
        // ============================================

        /// <summary>
        /// Count total products
        /// </summary>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Count products by category
        /// </summary>
        Task<int> CountByCategoryAsync(string category, CancellationToken cancellationToken = default);

        // ============================================
        // UNIT OF WORK
        // ============================================

        /// <summary>
        /// Save all changes to database
        /// 
        /// Why separate method?
        /// - Allows multiple operations before saving
        /// - Atomic transactions (all or nothing)
        /// - Better performance
        /// - Unit of Work pattern
        /// 
        /// Example:
        /// var product1 = await repository.GetByIdAsync(id1);
        /// var product2 = await repository.GetByIdAsync(id2);
        /// product1.ReserveStock(5);
        /// product2.ReserveStock(3);
        /// await repository.UpdateAsync(product1);
        /// await repository.UpdateAsync(product2);
        /// await repository.SaveChangesAsync();  // Both saved atomically
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
