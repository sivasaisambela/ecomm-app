using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Core.Entities;

namespace ProductService.Domain.Entities
{
    /// <summary>
    /// Product domain entity
    /// 
    /// Represents a product in the e-commerce system.
    /// Contains business logic for stock management.
    /// 
    /// Key Responsibilities:
    /// - Maintain product information
    /// - Manage stock reservations
    /// - Enforce business rules
    /// - Track audit information
    /// </summary>
    public class Product : BaseEntity
    {
        // ============================================
        // PRODUCT INFORMATION
        // ============================================

        /// <summary>
        /// Product name (e.g., "Dell XPS 13 Laptop")
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Product description
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Product SKU (Stock Keeping Unit)
        /// Unique identifier used in warehouse systems
        /// Example: "LAPTOP-DELL-XPS13-2024"
        /// Immutable once set
        /// </summary>
        public string Sku { get; private set; } = string.Empty;

        // ============================================
        // PRICING
        // ============================================

        /// <summary>
        /// Selling price (what customer pays)
        /// </summary>
        public decimal Price { get; private set; }

        /// <summary>
        /// Cost price (what we paid for it)
        /// Used for profit calculations
        /// </summary>
        public decimal Cost { get; private set; }

        // ============================================
        // STOCK MANAGEMENT
        // ============================================

        /// <summary>
        /// Total stock quantity in warehouse
        /// </summary>
        public int StockQuantity { get; private set; }

        /// <summary>
        /// Minimum stock level for reordering
        /// When AvailableQuantity <= MinimumStockLevel, admin should reorder
        /// </summary>
        public int MinimumStockLevel { get; private set; }

        /// <summary>
        /// Reserved quantity for pending orders
        /// When order is created but not yet confirmed
        /// 
        /// Example:
        /// - StockQuantity = 100
        /// - ReservedQuantity = 5 (reserved for pending order)
        /// - AvailableQuantity = 95 (can sell to other customers)
        /// </summary>
        public int ReservedQuantity { get; private set; }

        /// <summary>
        /// Available quantity = StockQuantity - ReservedQuantity
        /// What customers can actually purchase right now
        /// </summary>
        public int AvailableQuantity => StockQuantity - ReservedQuantity;

        // ============================================
        // CATEGORIZATION & STATUS
        // ============================================

        /// <summary>
        /// Product category (e.g., "Electronics", "Laptops")
        /// </summary>
        public string Category { get; private set; } = string.Empty;

        /// <summary>
        /// Is product active (can be purchased)
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Is stock level low
        /// </summary>
        public bool IsStockLow => AvailableQuantity <= MinimumStockLevel;

        // ============================================
        // FACTORY METHOD (Recommended way to create)
        // ============================================

        /// <summary>
        /// Create a new product
        /// 
        /// This is a factory method that validates all inputs
        /// and creates a new Product instance.
        /// 
        /// Why factory method?
        /// - Centralizes validation logic
        /// - Ensures product is always in valid state
        /// - Makes it impossible to create invalid products
        /// </summary>
        public static Product Create(
            string name,
            string description,
            string sku,
            decimal price,
            decimal cost,
            int stockQuantity,
            int minimumStockLevel,
            string category,
            string createdBy)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("Product SKU cannot be empty.", nameof(sku));

            if (price <= 0)
                throw new ArgumentException("Product price must be greater than zero.", nameof(price));

            if (cost < 0)
                throw new ArgumentException("Product cost cannot be negative.", nameof(cost));

            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

            if (minimumStockLevel < 0)
                throw new ArgumentException("Minimum stock level cannot be negative.", nameof(minimumStockLevel));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Product category cannot be empty.", nameof(category));

            // Create product with valid state
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Sku = sku.Trim().ToUpperInvariant(),
                Price = price,
                Cost = cost,
                StockQuantity = stockQuantity,
                MinimumStockLevel = minimumStockLevel,
                ReservedQuantity = 0,
                Category = category.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        // ============================================
        // BUSINESS OPERATIONS
        // ============================================

        /// <summary>
        /// Reserve stock for a new order
        /// 
        /// Business Rules:
        /// - Quantity must be positive
        /// - Must have enough available stock
        /// - Updates reserved quantity
        /// 
        /// Called when: Order is created (before payment)
        /// 
        /// Example:
        /// Product has 100 stock, customer orders 5
        /// After ReserveStock(5):
        ///   - StockQuantity = 100 (unchanged)
        ///   - ReservedQuantity = 5
        ///   - AvailableQuantity = 95
        /// </summary>
        public void ReserveStock(int quantity)
        {
            // Rule 1: Validate quantity
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            // Rule 2: Check availability
            if (AvailableQuantity < quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock. Available: {AvailableQuantity}, Requested: {quantity}");

            // Rule 3: Update state
            ReservedQuantity += quantity;
        }

        /// <summary>
        /// Release reserved stock
        /// 
        /// Business Rules:
        /// - Quantity must be positive
        /// - Cannot release more than reserved
        /// 
        /// Called when: Order is cancelled
        /// 
        /// Example:
        /// If ReservedQuantity = 5 and we cancel order:
        /// After ReleaseReservedStock(5):
        ///   - ReservedQuantity = 0
        ///   - AvailableQuantity increases by 5
        /// </summary>
        public void ReleaseReservedStock(int quantity)
        {
            // Rule 1: Validate quantity
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            // Rule 2: Check reserved amount
            if (ReservedQuantity < quantity)
                throw new InvalidOperationException(
                    $"Cannot release more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

            // Rule 3: Update state
            ReservedQuantity -= quantity;
        }

        /// <summary>
        /// Confirm reserved stock (convert to actual sale)
        /// 
        /// Business Rules:
        /// - Quantity must be positive
        /// - Must have enough reserved stock
        /// - Decreases both stock and reserved quantities
        /// 
        /// Called when: Order payment is confirmed
        /// 
        /// Example:
        /// Before: StockQuantity = 100, ReservedQuantity = 5
        /// After ConfirmReservedStock(5):
        ///   - StockQuantity = 95 (actual sale)
        ///   - ReservedQuantity = 0 (no longer reserved)
        /// </summary>
        public void ConfirmReservedStock(int quantity)
        {
            // Rule 1: Validate quantity
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            // Rule 2: Check reserved amount
            if (ReservedQuantity < quantity)
                throw new InvalidOperationException(
                    $"Insufficient reserved stock. Reserved: {ReservedQuantity}, Requested: {quantity}");

            // Rule 3: Update state (actual sale)
            StockQuantity -= quantity;
            ReservedQuantity -= quantity;
        }

        /// <summary>
        /// Add stock (e.g., when new inventory arrives)
        /// </summary>
        public void AddStock(int quantity, string addedBy)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            StockQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = addedBy;
        }

        /// <summary>
        /// Update product information
        /// </summary>
        public void Update(
            string name,
            string description,
            decimal price,
            decimal cost,
            int minimumStockLevel,
            string category,
            bool isActive,
            string updatedBy)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.", nameof(name));

            if (price <= 0)
                throw new ArgumentException("Product price must be greater than zero.", nameof(price));

            if (cost < 0)
                throw new ArgumentException("Product cost cannot be negative.", nameof(cost));

            // Update
            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            Price = price;
            Cost = cost;
            MinimumStockLevel = minimumStockLevel;
            Category = category.Trim();
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Deactivate product
        /// </summary>
        public void Deactivate(string deactivatedBy)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
        }

        /// <summary>
        /// Activate product
        /// </summary>
        public void Activate(string activatedBy)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
        }

        /// <summary>
        /// Soft delete product
        /// </summary>
        public void Delete(string deletedBy)
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deletedBy;
        }

        // ============================================
        // QUERY METHODS (No state changes)
        // ============================================

        /// <summary>
        /// Check if product can be ordered
        /// </summary>
        public bool CanBeOrdered() => IsActive && !IsDeleted && AvailableQuantity > 0;

        /// <summary>
        /// Get profit margin percentage
        /// Formula: (Price - Cost) / Price * 100
        /// </summary>
        public decimal GetProfitMarginPercentage()
        {
            if (Price == 0)
                return 0;

            return ((Price - Cost) / Price) * 100;
        }

        /// <summary>
        /// Get profit per unit
        /// </summary>
        public decimal GetProfitPerUnit() => Price - Cost;
    }
}
