using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // 1. Primary Key Configuration
            builder.HasKey(o => o.Id);

            // 2. Property Configurations
            builder.Property(o => o.CustomerId)
                .IsRequired()
                .HasMaxLength(100); // Standard length for customer/user IDs

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .HasConversion<int>() // Stores the enum value as a standard integer in the database
                .IsRequired();

            // 3. One-to-Many Relationship Configuration (Order -> OrderItems)
            builder.HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey("OrderId") // Shadow foreign key inside OrderItem table
                .OnDelete(DeleteBehavior.Cascade); // Automatically deletes child items if the parent order is deleted
        }
    }
}
