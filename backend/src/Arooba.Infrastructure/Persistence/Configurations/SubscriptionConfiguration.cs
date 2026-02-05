using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Subscription"/> entity.
/// Maps to the "Subscriptions" table with FK to Customer. ItemsJson stored as nvarchar(max).
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.Frequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.ItemsJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Ignore(s => s.DomainEvents);
    }
}
