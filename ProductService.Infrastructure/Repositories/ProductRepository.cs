using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{

    public class ProductRepository : IProductRepository
    {
        private readonly ProductContext _context;

        public ProductRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            var normalizedSku = sku.ToUpperInvariant();
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Sku == normalizedSku, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.IsActive && !p.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            var trimmedCategory = category.Trim();
            return await _context.Products
                .Where(p => p.IsActive && !p.IsDeleted && p.Category == trimmedCategory)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.IsActive && !p.IsDeleted && (p.StockQuantity - p.ReservedQuantity) <= p.MinimumStockLevel)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddRangeAsync(products, cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Entry(product).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task UpdateRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            foreach (var product in products)
            {
                _context.Entry(product).State = EntityState.Modified;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(id, cancellationToken);
            if (product != null)
            {
                // Soft delete behavior handled by domain method if preferred, 
                // or simply remove tracking here.
                _context.Products.Remove(product);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (products.Any())
            {
                _context.Products.RemoveRange(products);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
        {
            var normalizedSku = sku.ToUpperInvariant();
            return await _context.Products.AnyAsync(p => p.Sku == normalizedSku, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, Guid excludeProductId, CancellationToken cancellationToken = default)
        {
            var normalizedSku = sku.ToUpperInvariant();
            return await _context.Products.AnyAsync(p => p.Sku == normalizedSku && p.Id != excludeProductId, cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.CountAsync(cancellationToken);
        }

        public async Task<int> CountByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            var trimmedCategory = category.Trim();
            return await _context.Products.CountAsync(p => p.Category == trimmedCategory, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
