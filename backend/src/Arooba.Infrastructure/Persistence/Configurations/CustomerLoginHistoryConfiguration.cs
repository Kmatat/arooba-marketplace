using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="CustomerLoginHistory"/> entity.
/// Maps to the "CustomerLoginHistory" table with FK and indexes for security queries.
/// </summary>
public class CustomerLoginHistoryConfiguration : IEntityTypeConfiguration<CustomerLoginHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomerLoginHistory> builder)
    {
        builder.ToTable("CustomerLoginHistory");

        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.Customer)
            .WithMany(c => c.LoginHistory)
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(l => l.DeviceType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.DeviceInfo)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Location)
            .HasMaxLength(200);

        builder.Property(l => l.SessionId)
            .HasMaxLength(100);

        builder.HasIndex(l => l.CustomerId);
        builder.HasIndex(l => l.CreatedAt);
        builder.HasIndex(l => new { l.CustomerId, l.Status });

        builder.Ignore(l => l.DomainEvents);
    }
}
