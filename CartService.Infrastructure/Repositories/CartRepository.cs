using CartService.Domain.Entities;
using CartService.Domain.Interfaces;
using CartService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _dbContext;

        public CartRepository(CartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Cart?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
        }

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await _dbContext.Carts.AddAsync(cart, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            _dbContext.Carts.Update(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts.FirstOrDefaultAsync(x => x.Id == cartId, cancellationToken);
            if (cart is null)
                return;

            _dbContext.Carts.Remove(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
