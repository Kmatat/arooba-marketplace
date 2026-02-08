using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="TransactionSplit"/> entity.
/// Maps to the "TransactionSplits" table with FK to OrderItem and Order.
/// All bucket fields use decimal(18,2) precision.
/// </summary>
public class TransactionSplitConfiguration : IEntityTypeConfiguration<TransactionSplit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TransactionSplit> builder)
    {
        builder.ToTable("TransactionSplits");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.Order)
            .WithMany()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.OrderItem)
            .WithMany()
            .HasForeignKey(t => t.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Financial tracking
        builder.Property(t => t.GrossAmount).HasPrecision(18, 2);
        builder.Property(t => t.VendorPayoutBucket).HasPrecision(18, 2);
        builder.Property(t => t.AroobaBucket).HasPrecision(18, 2);
        builder.Property(t => t.VatBucket).HasPrecision(18, 2);
        builder.Property(t => t.ParentUpliftBucket).HasPrecision(18, 2);
        builder.Property(t => t.WithholdingTaxBucket).HasPrecision(18, 2);
        builder.Property(t => t.TotalAmount).HasPrecision(18, 2);

        builder.HasIndex(t => t.OrderId);
        builder.HasIndex(t => t.ParentVendorId);

        // Ignore computed/alias properties
        builder.Ignore(t => t.BucketA);
        builder.Ignore(t => t.BucketB);
        builder.Ignore(t => t.BucketC);
        builder.Ignore(t => t.BucketD);
        builder.Ignore(t => t.BucketE);
        builder.Ignore(t => t.DomainEvents);
    }
}
