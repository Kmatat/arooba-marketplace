using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="PickupLocation"/> entity.
/// Maps to the "PickupLocations" table with FK to vendor via VendorId.
/// </summary>
public class PickupLocationConfiguration : IEntityTypeConfiguration<PickupLocation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PickupLocation> builder)
    {
        builder.ToTable("PickupLocations");

        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.ParentVendor)
            .WithMany(v => v.PickupLocations)
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ZoneId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ContactName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ContactPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Ignore(p => p.DomainEvents);
    }
}
