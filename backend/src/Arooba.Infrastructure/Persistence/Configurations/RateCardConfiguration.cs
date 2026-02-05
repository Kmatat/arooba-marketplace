using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RateCard"/> entity.
/// Maps to the "RateCards" table with composite index on FromZoneId + ToZoneId.
/// </summary>
public class RateCardConfiguration : IEntityTypeConfiguration<RateCard>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RateCard> builder)
    {
        builder.ToTable("RateCards");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FromZoneId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.ToZoneId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => new { r.FromZoneId, r.ToZoneId });

        builder.Property(r => r.BasePrice).HasPrecision(18, 2);
        builder.Property(r => r.PricePerKg).HasPrecision(18, 2);

        builder.Ignore(r => r.DomainEvents);
    }
}
