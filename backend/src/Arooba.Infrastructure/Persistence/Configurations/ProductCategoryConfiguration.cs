using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ProductCategory"/> entity.
/// Maps to the "ProductCategories" table with string Id (e.g., "jewelry-accessories").
/// </summary>
public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasMaxLength(100)
            .ValueGeneratedNever();

        builder.Property(c => c.NameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Icon)
            .HasMaxLength(20);

        builder.Property(c => c.MinUpliftRate).HasPrecision(5, 4);
        builder.Property(c => c.MaxUpliftRate).HasPrecision(5, 4);
        builder.Property(c => c.DefaultUpliftRate).HasPrecision(5, 4);

        builder.Property(c => c.Risk)
            .IsRequired()
            .HasMaxLength(20);
    }
}
