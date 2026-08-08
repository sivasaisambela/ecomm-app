using CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartService.Infrastructure.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("CartItems");
            item.WithOwner().HasForeignKey("CartId");
            item.HasKey(x => x.Id);

            item.Property(x => x.ProductId).IsRequired();
            item.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            item.Property(x => x.UnitPrice).IsRequired().HasPrecision(18, 2);
            item.Property(x => x.Quantity).IsRequired();
        });
    }
}