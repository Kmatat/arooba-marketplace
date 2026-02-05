using System.Text.Json;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Product"/> entity.
/// Maps to the "Products" table with unique index on Sku, FK relationships to ParentVendor,
/// SubVendor, PickupLocation, and ProductCategory. Images and AllowedZoneIds stored as JSON.
/// All monetary fields use decimal(18,2) precision.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.HasOne(p => p.ParentVendor)
            .WithMany(v => v.Products)
            .HasForeignKey(p => p.ParentVendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SubVendor)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SubVendorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PickupLocation)
            .WithMany()
            .HasForeignKey(p => p.PickupLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CategoryId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.CategoryId);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.TitleAr)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.DescriptionAr)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        // Images stored as JSON
        builder.Property(p => p.Images)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Money fields with decimal(18,2) precision
        builder.Property(p => p.CostPrice).HasPrecision(18, 2);
        builder.Property(p => p.SellingPrice).HasPrecision(18, 2);
        builder.Property(p => p.CooperativeFee).HasPrecision(18, 2);
        builder.Property(p => p.MarketplaceUplift).HasPrecision(18, 2);
        builder.Property(p => p.FinalPrice).HasPrecision(18, 2);
        builder.Property(p => p.WeightKg).HasPrecision(10, 3);
        builder.Property(p => p.DimensionL).HasPrecision(10, 2);
        builder.Property(p => p.DimensionW).HasPrecision(10, 2);
        builder.Property(p => p.DimensionH).HasPrecision(10, 2);
        builder.Property(p => p.VolumetricWeight).HasPrecision(10, 3);

        builder.Property(p => p.StockMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(p => p.Status);

        // AllowedZoneIds stored as JSON
        builder.Property(p => p.AllowedZoneIds)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, JsonOptions))
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.LastModifiedBy)
            .HasMaxLength(100);

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.LengthCm);
        builder.Ignore(p => p.WidthCm);
        builder.Ignore(p => p.HeightCm);
    }
}
