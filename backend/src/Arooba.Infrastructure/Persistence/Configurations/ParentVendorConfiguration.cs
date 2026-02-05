using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ParentVendor"/> entity.
/// Maps to the "ParentVendors" table with FK to Users and optional FK to Cooperatives.
/// Financial fields use decimal(18,2) precision. Indexed on Status.
/// </summary>
public class ParentVendorConfiguration : IEntityTypeConfiguration<ParentVendor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ParentVendor> builder)
    {
        builder.ToTable("ParentVendors");

        builder.HasKey(v => v.Id);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Cooperative)
            .WithMany(c => c.Vendors)
            .HasForeignKey(v => v.CooperativeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(v => v.BusinessName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.BusinessNameAr)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(v => v.Status);

        builder.Property(v => v.CommercialRegNumber)
            .HasMaxLength(100);

        builder.Property(v => v.TaxId)
            .HasMaxLength(100);

        builder.Property(v => v.DefaultCommissionRate)
            .HasPrecision(18, 2);

        builder.Property(v => v.BankName)
            .HasMaxLength(200);

        builder.Property(v => v.BankAccountNumber)
            .HasMaxLength(100);

        builder.Property(v => v.AverageRating)
            .HasPrecision(3, 2);

        builder.Property(v => v.TotalRevenue)
            .HasPrecision(18, 2);

        builder.Property(v => v.CreatedBy)
            .HasMaxLength(100);

        builder.Property(v => v.LastModifiedBy)
            .HasMaxLength(100);

        builder.Ignore(v => v.DomainEvents);
    }
}
