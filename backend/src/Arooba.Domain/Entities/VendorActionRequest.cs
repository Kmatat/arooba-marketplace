using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Tracks vendor actions that require admin approval before taking effect.
/// Implements the approval cycle workflow: Pending → Approved/Rejected.
/// Stores both the current and proposed values for audit purposes.
/// </summary>
public class VendorActionRequest : AuditableEntity
{
    /// <summary>The vendor who initiated the action.</summary>
    public int VendorId { get; set; }
    public ParentVendor? Vendor { get; set; }

    /// <summary>The type of action being requested.</summary>
    public VendorActionType ActionType { get; set; }

    /// <summary>Current approval status.</summary>
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    /// <summary>The entity type this action affects (e.g., "Product", "SubVendor").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>The ID of the entity being modified (if applicable).</summary>
    public int? EntityId { get; set; }

    /// <summary>JSON snapshot of current values before the change.</summary>
    public string? CurrentValues { get; set; }

    /// <summary>JSON snapshot of proposed new values.</summary>
    public string? ProposedValues { get; set; }

    /// <summary>Vendor-provided justification for the change.</summary>
    public string? Justification { get; set; }

    /// <summary>Admin who reviewed the request.</summary>
    public string? ReviewedBy { get; set; }

    /// <summary>When the request was reviewed.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Admin's reason for approval or rejection.</summary>
    public string? ReviewNotes { get; set; }

    /// <summary>Priority level (1=low, 2=medium, 3=high, 4=urgent).</summary>
    public int Priority { get; set; } = 2;

    /// <summary>When the request expires if not reviewed.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Approves the action request.</summary>
    public Result Approve(string reviewedBy, string? notes = null)
    {
        if (Status != ApprovalStatus.Pending)
            return Result.Failure("Only pending requests can be approved.");
        Status = ApprovalStatus.Approved;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = notes;
        return Result.Success();
    }

    /// <summary>Rejects the action request.</summary>
    public Result Reject(string reviewedBy, string notes)
    {
        if (Status != ApprovalStatus.Pending)
            return Result.Failure("Only pending requests can be rejected.");
        Status = ApprovalStatus.Rejected;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = notes;
        return Result.Success();
    }

    /// <summary>Cancels the action request (by the vendor).</summary>
    public Result Cancel()
    {
        if (Status != ApprovalStatus.Pending)
            return Result.Failure("Only pending requests can be cancelled.");
        Status = ApprovalStatus.Cancelled;
        return Result.Success();
    }
}
