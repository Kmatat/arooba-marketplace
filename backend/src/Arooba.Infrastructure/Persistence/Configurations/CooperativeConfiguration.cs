using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Cooperative"/> entity.
/// Maps to the "Cooperatives" table with a unique index on TaxId.
/// </summary>
public class CooperativeConfiguration : IEntityTypeConfiguration<Cooperative>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Cooperative> builder)
    {
        builder.ToTable("Cooperatives");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.NameAr)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.TaxId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.TaxId)
            .IsUnique();

        builder.Property(c => c.FeePercentage)
            .HasPrecision(5, 4);

        builder.Ignore(c => c.DomainEvents);
    }
}
