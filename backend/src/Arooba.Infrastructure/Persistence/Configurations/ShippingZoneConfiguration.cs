using System.Text.Json;
using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ShippingZone"/> entity.
/// Maps to the "ShippingZones" table with string Id (e.g., "cairo").
/// CitiesCovered stored as JSON.
/// </summary>
public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Id)
            .HasMaxLength(50)
            .ValueGeneratedNever();

        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(z => z.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(z => z.CitiesCovered)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");
    }
}
