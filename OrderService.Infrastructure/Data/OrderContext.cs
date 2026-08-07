using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Data
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order Entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CustomerId).IsRequired();
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                // Configure the relationship: An Order has many OrderItems
                entity.HasMany(o => o.OrderItems)
                      .WithOne()
                      .HasForeignKey("OrderId") // Shadow foreign key
                      .OnDelete(DeleteBehavior.Cascade); // If an order is deleted, delete its items too
            });

            // Configure OrderItem Entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id); // Corrected to oi.Id!
                entity.Property(oi => oi.ProductName).IsRequired();
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            });
        }
    }
}
