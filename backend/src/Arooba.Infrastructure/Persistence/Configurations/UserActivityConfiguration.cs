using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="UserActivity"/> entity.
/// Optimized for high-volume write and time-range query patterns.
/// </summary>
public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        builder.ToTable("UserActivities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.SessionId)
            .HasMaxLength(100);

        builder.Property(a => a.SearchQuery)
            .HasMaxLength(500);

        builder.Property(a => a.CategoryId)
            .HasMaxLength(100);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.PageUrl)
            .HasMaxLength(2000);

        builder.Property(a => a.ReferrerUrl)
            .HasMaxLength(2000);

        builder.Property(a => a.DeviceType)
            .HasMaxLength(20);

        builder.Property(a => a.CartValue)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for analytics queries
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.ProductId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.SessionId);
        builder.HasIndex(a => new { a.Action, a.CreatedAt });
        builder.HasIndex(a => new { a.ProductId, a.Action });

        builder.Ignore(a => a.DomainEvents);
    }
}
