using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Order"/> entity.
/// Maps to the "Orders" table with FK to Customer, decimal(18,2) precision for money fields,
/// and indexes on Status and CustomerId.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.TotalDeliveryFee).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        builder.Property(o => o.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.DeliveryCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.DeliveryZoneId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.CreatedBy)
            .HasMaxLength(100);

        builder.Property(o => o.LastModifiedBy)
            .HasMaxLength(100);

        builder.Ignore(o => o.DomainEvents);
    }
}
