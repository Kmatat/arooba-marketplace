using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Immutable audit log entry recording all significant actions in the system.
/// Provides full traceability for compliance and dispute resolution.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>The user who performed the action.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Display name of the user.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Role of the user at the time of action.</summary>
    public string UserRole { get; set; } = string.Empty;

    /// <summary>The type of action performed.</summary>
    public AuditAction Action { get; set; }

    /// <summary>The entity type affected (e.g., "Product", "Vendor", "Order").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>The ID of the entity affected.</summary>
    public string? EntityId { get; set; }

    /// <summary>Human-readable description of what happened.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Arabic description.</summary>
    public string? DescriptionAr { get; set; }

    /// <summary>JSON snapshot of values before the change.</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON snapshot of values after the change.</summary>
    public string? NewValues { get; set; }

    /// <summary>IP address of the user.</summary>
    public string? IpAddress { get; set; }

    /// <summary>User agent string.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Related vendor action request ID (if applicable).</summary>
    public Guid? VendorActionRequestId { get; set; }
}
