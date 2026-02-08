namespace Arooba.Domain.Enums;

/// <summary>
/// Moderation status of a customer review.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Review is published and visible to all users.</summary>
    Published,

    /// <summary>Review is pending moderation approval.</summary>
    Pending,

    /// <summary>Review has been flagged for manual review.</summary>
    Flagged,

    /// <summary>Review has been removed by admin.</summary>
    Removed
}
