using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="LedgerEntry"/> entity.
/// Maps to the "LedgerEntries" table with FK to Order, indexes on VendorId.
/// </summary>
public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ParentVendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.TransactionId)
            .HasMaxLength(100);

        builder.Property(e => e.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.BalanceStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.VendorAmount).HasPrecision(18, 2);
        builder.Property(e => e.CommissionAmount).HasPrecision(18, 2);
        builder.Property(e => e.VatAmount).HasPrecision(18, 2);
        builder.Property(e => e.BalanceAfter).HasPrecision(18, 2);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.BalanceStatus);

        // Ignore computed/alias properties
        builder.Ignore(e => e.ParentVendorId);
        builder.Ignore(e => e.DomainEvents);
    }
}
