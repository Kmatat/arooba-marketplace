using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arooba.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="CustomerReview"/> entity.
/// Maps to the "CustomerReviews" table with FK relationships and indexes.
/// </summary>
public class CustomerReviewConfiguration : IEntityTypeConfiguration<CustomerReview>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomerReview> builder)
    {
        builder.ToTable("CustomerReviews");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Customer)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.ReviewText)
            .HasMaxLength(2000);

        builder.Property(r => r.AdminReply)
            .HasMaxLength(1000);

        builder.Property(r => r.AdminReplyBy)
            .HasMaxLength(100);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => new { r.CustomerId, r.ProductId, r.OrderId }).IsUnique();

        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);
        builder.Ignore(r => r.DomainEvents);
    }
}
