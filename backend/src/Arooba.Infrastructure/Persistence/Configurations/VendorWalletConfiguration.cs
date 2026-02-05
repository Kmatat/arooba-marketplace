using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="VendorWallet"/> entity.
/// Maps to the "VendorWallets" table with VendorId as PK and FK to ParentVendor.
/// All balance fields use decimal(18,2) precision.
/// </summary>
public class VendorWalletConfiguration : IEntityTypeConfiguration<VendorWallet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VendorWallet> builder)
    {
        builder.ToTable("VendorWallets");

        builder.HasKey(w => w.VendorId);

        builder.HasOne(w => w.ParentVendor)
            .WithOne()
            .HasForeignKey<VendorWallet>(w => w.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(w => w.TotalBalance).HasPrecision(18, 2);
        builder.Property(w => w.PendingBalance).HasPrecision(18, 2);
        builder.Property(w => w.AvailableBalance).HasPrecision(18, 2);
        builder.Property(w => w.LifetimeEarnings).HasPrecision(18, 2);
    }
}
