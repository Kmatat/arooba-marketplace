using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a customer review/rating for a product on the Arooba Marketplace.
/// Tracks verified purchases, helpfulness votes, and admin moderation.
/// </summary>
public class CustomerReview : AuditableEntity
{
    /// <summary>The customer who wrote the review.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The order associated with this review.</summary>
    public Guid OrderId { get; set; }

    /// <summary>The product being reviewed.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Star rating from 1 to 5.</summary>
    public int Rating { get; set; }

    /// <summary>Optional review text content.</summary>
    public string? ReviewText { get; set; }

    /// <summary>Whether the review is from a verified purchase.</summary>
    public bool IsVerifiedPurchase { get; set; } = true;

    /// <summary>Number of users who found this review helpful.</summary>
    public int HelpfulCount { get; set; }

    /// <summary>Moderation status of the review.</summary>
    public ReviewStatus Status { get; set; } = ReviewStatus.Published;

    /// <summary>Optional admin reply to the review.</summary>
    public string? AdminReply { get; set; }

    /// <summary>Admin who replied (if any).</summary>
    public string? AdminReplyBy { get; set; }

    /// <summary>When the admin reply was posted.</summary>
    public DateTime? AdminReplyAt { get; set; }

    // Navigation properties
    /// <summary>Navigation property to the customer.</summary>
    public Customer? Customer { get; set; }

    /// <summary>Navigation property to the order.</summary>
    public Order? Order { get; set; }

    /// <summary>Navigation property to the product.</summary>
    public Product? Product { get; set; }
}
