using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Interfaces;
using Shared.Core.Exceptions;
using FluentValidation;

namespace ProductService.Application.Services
{
    /// <summary>
    /// Application service implementation for product operations
    /// 
    /// Flow:
    /// 1. Receive DTO from controller
    /// 2. Validate inputs
    /// 3. Call repository to get entity
    /// 4. Call domain methods on entity
    /// 5. Call repository to save
    /// 6. Map entity back to DTO
    /// 7. Return DTO to controller
    /// </summary>
    public class ProductApplicationService : IProductApplicationService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductApplicationService> _logger;
        private readonly IValidator<CreateProductDto> _createValidator; // Added!
        public ProductApplicationService(
            IProductRepository repository,
            IMapper mapper,
            ILogger<ProductApplicationService> logger,
            IValidator<CreateProductDto> createValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        }

        // ============================================
        // READ OPERATIONS
        // ============================================

        public async Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            var product = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new ProductNotFoundException(id);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product by SKU: {Sku}", sku);

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException(new() { { "sku", new[] { "SKU cannot be empty" } } });

            var product = await _repository.GetBySkuAsync(sku, cancellationToken)
                ?? throw new ProductNotFoundException(sku);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all products");

            var products = await _repository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting products by category: {Category}", category);

            if (string.IsNullOrWhiteSpace(category))
                throw new ValidationException(new() { { "category", new[] { "Category cannot be empty" } } });

            var products = await _repository.GetByCategoryAsync(category, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting low stock products");

            var products = await _repository.GetLowStockProductsAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids.ToList();
            _logger.LogInformation("Getting {Count} products by IDs", idList.Count);

            if (!idList.Any())
                return Enumerable.Empty<ProductDto>();

            var products = await _repository.GetByIdsAsync(idList, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        // ============================================
        // CREATE OPERATIONS
        // ============================================

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto,string userId,CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating product: {ProductName}", createDto.Name);

            // 1. Perform fluent validation
            var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                // Converts FluentValidation errors into a dictionary matching your ValidationException
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new Shared.Core.Exceptions.ValidationException(errors);
            }

            // 2. Check for duplicate SKU
            if (await _repository.SkuExistsAsync(createDto.Sku, cancellationToken))
                throw new DuplicateSkuException(createDto.Sku);

            // 3. Create product using domain constructor/factory
            var product = Product.Create(
                createDto.Name,
                createDto.Description,
                createDto.Sku,
                createDto.Price,
                createDto.Cost,
                createDto.StockQuantity,
                createDto.MinimumStockLevel,
                createDto.Category,
                userId);

            // 4. Save to repository
            await _repository.AddAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product created successfully: {ProductId}", product.Id);

            return _mapper.Map<ProductDto>(product);
        }

        // ============================================
        // UPDATE OPERATIONS
        // ============================================

        public async Task<ProductDto> UpdateProductAsync(
            UpdateProductDto updateDto,
            string userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating product: {ProductId}", updateDto.Id);

            // Validate input
            if (string.IsNullOrWhiteSpace(updateDto.Name))
                throw new ValidationException(new() { { "name", new[] { "Product name is required" } } });

            if (updateDto.Price <= 0)
                throw new ValidationException(new() { { "price", new[] { "Product price must be greater than zero" } } });

            // Get existing product
            var product = await _repository.GetByIdAsync(updateDto.Id, cancellationToken)
                ?? throw new ProductNotFoundException(updateDto.Id);

            // Check row version (optimistic locking)
            if (updateDto.RowVersion != null
                && product.RowVersion != null
                && !updateDto.RowVersion.SequenceEqual(product.RowVersion))
                throw new ConcurrencyException("Product was modified by another user. Please refresh and try again.");

            // Update using domain method
            product.Update(
                updateDto.Name,
                updateDto.Description,
                updateDto.Price,
                updateDto.Cost,
                updateDto.MinimumStockLevel,
                updateDto.Category,
                updateDto.IsActive,
                userId);

            // Save to repository
            await _repository.UpdateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product updated successfully: {ProductId}", product.Id);

            // Map and return
            return _mapper.Map<ProductDto>(product);
        }

        // ============================================
        // DELETE OPERATIONS
        // ============================================

        public async Task DeleteProductAsync(Guid id, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);

            // Get existing product
            var product = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new ProductNotFoundException(id);

            // Delete using domain method (soft delete)
            product.Delete(userId);

            // Save to repository
            await _repository.UpdateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
        }

        // ============================================
        // STOCK OPERATIONS
        // ============================================

        public async Task ReserveStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Reserving stock for product {ProductId}: {Quantity}", productId, quantity);

            // Get product
            var product = await _repository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            // Reserve stock (domain method handles validation)
            try
            {
                product.ReserveStock(quantity);
            }
            catch (InvalidOperationException ex)
            {
                throw new InsufficientStockException(product.Name, product.AvailableQuantity, quantity);
            }

            // Save
            await _repository.UpdateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock reserved successfully: {ProductId}", productId);
        }

        public async Task ReleaseReservedStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Releasing reserved stock for product {ProductId}: {Quantity}", productId, quantity);

            // Get product
            var product = await _repository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            // Release stock (domain method handles validation)
            product.ReleaseReservedStock(quantity);

            // Save
            await _repository.UpdateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reserved stock released successfully: {ProductId}", productId);
        }

        public async Task ConfirmReservedStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Confirming reserved stock for product {ProductId}: {Quantity}", productId, quantity);

            // Get product
            var product = await _repository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            // Confirm stock (domain method handles validation)
            product.ConfirmReservedStock(quantity);

            // Save
            await _repository.UpdateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reserved stock confirmed successfully: {ProductId}", productId);
        }

        public async Task AddStockAsync(
            Guid productId,
            int quantity,
            string userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding stock for product {ProductId}: {Quantity}", productId, quantity);

            // Get product
            var product = await _repository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            // Add stock (domain method handles validation)
            product.AddStock(quantity, userId);

            // Save
            await _repository.UpdateAsync(product, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock added successfully: {ProductId}", productId);
        }

        // ============================================
        // UTILITY OPERATIONS
        // ============================================

        public async Task<bool> CanBeOrderedAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking if product can be ordered: {ProductId}", productId);

            var product = await _repository.GetByIdAsync(productId, cancellationToken);
            return product?.CanBeOrdered() ?? false;
        }

        public async Task<(decimal Cost, decimal Price, decimal ProfitPerUnit, decimal ProfitMarginPercentage)>
            GetProfitInfoAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting profit info for product: {ProductId}", productId);

            var product = await _repository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            return (
                product.Cost,
                product.Price,
                product.GetProfitPerUnit(),
                product.GetProfitMarginPercentage()
            );
        }
    }
}
