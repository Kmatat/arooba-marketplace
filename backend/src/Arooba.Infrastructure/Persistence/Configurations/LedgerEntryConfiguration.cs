using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="LedgerEntry"/> entity.
/// Maps to the "LedgerEntries" table with index on VendorId and TransactionType stored as string.
/// </summary>
public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.BalanceAfter)
            .HasPrecision(18, 2);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.VendorId);

        builder.Ignore(e => e.DomainEvents);
    }
}
