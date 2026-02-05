using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Customer"/> entity.
/// Maps to the "Customers" table with FK to User and unique index on ReferralCode.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.WalletBalance)
            .HasPrecision(18, 2);

        builder.Property(c => c.TotalSpent)
            .HasPrecision(18, 2);

        builder.Property(c => c.ReferralCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.ReferralCode)
            .IsUnique();

        builder.Property(c => c.ReferredBy)
            .HasMaxLength(50);

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(100);

        builder.Ignore(c => c.DomainEvents);
    }
}
