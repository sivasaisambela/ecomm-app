using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // 1. Primary Key Configuration
            builder.HasKey(p => p.Id);

            // 2. Property Configurations
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);

            // Sets the database index on Sku to be Unique (prevents duplicates at SQL level)
            builder.HasIndex(p => p.Sku)
                .IsUnique();

            builder.Property(p => p.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.StockQuantity)
                .IsRequired();

            builder.Property(p => p.ReservedQuantity)
                .IsRequired();
        }
    }
}
