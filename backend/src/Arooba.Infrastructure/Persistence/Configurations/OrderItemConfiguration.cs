using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="OrderItem"/> entity.
/// Maps to the "OrderItems" table with FK to Order and Product.
/// All financial fields use decimal(18,2) precision.
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

        builder.HasOne(i => i.Shipment)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(i => i.ProductTitle)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.ProductSku)
            .HasMaxLength(100);

        builder.Property(i => i.ProductImage)
            .HasMaxLength(500);

        builder.Property(i => i.VendorName)
            .HasMaxLength(300);

        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 2);
        builder.Property(i => i.VendorPayout).HasPrecision(18, 2);
        builder.Property(i => i.CommissionAmount).HasPrecision(18, 2);
        builder.Property(i => i.VatAmount).HasPrecision(18, 2);
        builder.Property(i => i.ParentUpliftAmount).HasPrecision(18, 2);
        builder.Property(i => i.WithholdingTaxAmount).HasPrecision(18, 2);

        builder.HasIndex(i => i.ParentVendorId);
        builder.HasIndex(i => i.PickupLocationId);

        // Ignore computed properties
        builder.Ignore(i => i.VendorNetPayout);
        builder.Ignore(i => i.DomainEvents);
    }
}
