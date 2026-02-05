namespace Arooba.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a product listing on the marketplace.
/// </summary>
public enum ProductStatus
{
    /// <summary>Product is being prepared by the vendor and is not yet submitted.</summary>
    Draft,

    /// <summary>Product has been submitted and is awaiting admin review.</summary>
    PendingReview,

    /// <summary>Product is approved and visible to customers.</summary>
    Active,

    /// <summary>Product has been temporarily paused by the vendor or admin.</summary>
    Paused,

    /// <summary>Product was rejected during the review process.</summary>
    Rejected
}
