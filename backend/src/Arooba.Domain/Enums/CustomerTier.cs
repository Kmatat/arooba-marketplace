namespace Arooba.Domain.Enums;

/// <summary>
/// Customer loyalty tier levels based on accumulated loyalty points.
/// </summary>
public enum CustomerTier
{
    /// <summary>Entry level tier (0-4,999 lifetime points).</summary>
    Bronze,

    /// <summary>Silver tier (5,000-9,999 lifetime points).</summary>
    Silver,

    /// <summary>Gold tier (10,000-19,999 lifetime points).</summary>
    Gold,

    /// <summary>Top tier for VIP customers (20,000+ lifetime points).</summary>
    Platinum
}
