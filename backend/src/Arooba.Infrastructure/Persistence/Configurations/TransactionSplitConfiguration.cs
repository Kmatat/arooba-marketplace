using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="TransactionSplit"/> entity.
/// Maps to the "TransactionSplits" table with FK to OrderItem.
/// All bucket fields use decimal(18,2) precision.
/// </summary>
public class TransactionSplitConfiguration : IEntityTypeConfiguration<TransactionSplit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TransactionSplit> builder)
    {
        builder.ToTable("TransactionSplits");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.OrderItem)
            .WithMany()
            .HasForeignKey(t => t.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.BucketA).HasPrecision(18, 2);
        builder.Property(t => t.BucketB).HasPrecision(18, 2);
        builder.Property(t => t.BucketC).HasPrecision(18, 2);
        builder.Property(t => t.BucketD).HasPrecision(18, 2);
        builder.Property(t => t.BucketE).HasPrecision(18, 2);
        builder.Property(t => t.TotalAmount).HasPrecision(18, 2);

        builder.Ignore(t => t.DomainEvents);
    }
}
