using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ParentVendor"/> entity.
/// Maps to the "ParentVendors" table with FK to Users.
/// Financial fields use decimal(18,2) precision. Indexed on Status.
/// </summary>
public class ParentVendorConfiguration : IEntityTypeConfiguration<ParentVendor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ParentVendor> builder)
    {
        builder.ToTable("ParentVendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.BusinessName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.BusinessNameEn)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.VendorType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(v => v.Status);

        builder.Property(v => v.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(v => v.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.GovernorateId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.TaxRegistrationNumber)
            .HasMaxLength(100);

        builder.Property(v => v.CommissionRate)
            .HasPrecision(5, 4);

        builder.Property(v => v.BankName)
            .HasMaxLength(200);

        builder.Property(v => v.BankAccountNumber)
            .HasMaxLength(100);

        builder.Property(v => v.SubVendorUpliftType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(v => v.SubVendorUpliftValue)
            .HasPrecision(5, 4);

        builder.Property(v => v.CreatedBy)
            .HasMaxLength(100);

        builder.Property(v => v.LastModifiedBy)
            .HasMaxLength(100);

        builder.Ignore(v => v.DomainEvents);
    }
}
