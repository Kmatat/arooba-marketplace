using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="SubVendor"/> entity.
/// Maps to the "SubVendors" table with FK to ParentVendors and decimal precision for UpliftValue.
/// </summary>
public class SubVendorConfiguration : IEntityTypeConfiguration<SubVendor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SubVendor> builder)
    {
        builder.ToTable("SubVendors");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.ParentVendor)
            .WithMany(p => p.SubVendors)
            .HasForeignKey(s => s.ParentVendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.InternalName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.InternalNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.UpliftType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.UpliftValue)
            .HasPrecision(18, 4);

        builder.Ignore(s => s.DomainEvents);
    }
}
