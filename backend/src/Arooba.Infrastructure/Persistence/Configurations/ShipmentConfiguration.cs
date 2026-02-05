using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Shipment"/> entity.
/// Maps to the "Shipments" table with FK to Order.
/// </summary>
public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(s => s.CourierProvider)
            .HasMaxLength(100);

        builder.Property(s => s.DeliveryFee)
            .HasPrecision(18, 2);

        builder.Property(s => s.CodAmountDue)
            .HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Ignore(s => s.DomainEvents);
    }
}
