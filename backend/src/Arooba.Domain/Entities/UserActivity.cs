using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Tracks individual user actions on the platform for analytics,
/// conversion funnel analysis, and product performance reporting.
/// </summary>
public class UserActivity : BaseEntity
{
    /// <summary>The user who performed the action.</summary>
    public Guid UserId { get; set; }

    /// <summary>Navigation property to the user.</summary>
    public User? User { get; set; }

    /// <summary>Type of action performed.</summary>
    public UserActivityAction Action { get; set; }

    /// <summary>The product involved (if applicable).</summary>
    public Guid? ProductId { get; set; }

    /// <summary>Navigation property to the product.</summary>
    public Product? Product { get; set; }

    /// <summary>The category involved (if applicable).</summary>
    public string? CategoryId { get; set; }

    /// <summary>The order involved (if applicable).</summary>
    public Guid? OrderId { get; set; }

    /// <summary>Search query text (for ProductSearched actions).</summary>
    public string? SearchQuery { get; set; }

    /// <summary>Additional metadata as JSON (e.g., filter values, quantities).</summary>
    public string? Metadata { get; set; }

    /// <summary>Session identifier for grouping actions within a visit.</summary>
    public string? SessionId { get; set; }

    /// <summary>IP address of the user at time of action.</summary>
    public string? IpAddress { get; set; }

    /// <summary>User agent string.</summary>
    public string? UserAgent { get; set; }

    /// <summary>The page URL or screen the action occurred on.</summary>
    public string? PageUrl { get; set; }

    /// <summary>Referrer URL (where the user came from).</summary>
    public string? ReferrerUrl { get; set; }

    /// <summary>Device type: mobile, desktop, tablet.</summary>
    public string? DeviceType { get; set; }

    /// <summary>Cart value at the time of action (for funnel analysis).</summary>
    public decimal? CartValue { get; set; }

    /// <summary>Number of items in cart at time of action.</summary>
    public int? CartItemCount { get; set; }
}
