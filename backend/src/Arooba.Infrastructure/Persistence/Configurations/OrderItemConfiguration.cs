using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="OrderItem"/> entity.
/// Maps to the "OrderItems" table with FK to Order and Product.
/// All bucket fields use decimal(18,2) precision.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.ProductTitle)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.ProductImage)
            .HasMaxLength(500);

        builder.Property(i => i.VendorName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 2);
        builder.Property(i => i.BucketA_VendorRevenue).HasPrecision(18, 2);
        builder.Property(i => i.BucketB_VendorVat).HasPrecision(18, 2);
        builder.Property(i => i.BucketC_AroobaRevenue).HasPrecision(18, 2);
        builder.Property(i => i.BucketD_AroobaVat).HasPrecision(18, 2);
        builder.Property(i => i.BucketE_LogisticsFee).HasPrecision(18, 2);

        builder.Ignore(i => i.DomainEvents);
    }
}
