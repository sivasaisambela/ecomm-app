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

    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            // 1. Primary Key Configuration
            builder.HasKey(oi => oi.Id);

            // 2. Property Configurations
            builder.Property(oi => oi.ProductName)
                .IsRequired()
                .HasMaxLength(250); // Restricts max length to prevent database abuse

            builder.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(oi => oi.Quantity)
                .IsRequired();
        }
    }
}
