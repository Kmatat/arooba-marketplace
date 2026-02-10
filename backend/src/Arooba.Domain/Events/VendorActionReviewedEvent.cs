using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Events;

/// <summary>
/// Raised when an admin reviews (approves or rejects) a vendor action request.
/// </summary>
public record VendorActionReviewedEvent(
    int RequestId,
    int VendorId,
    VendorActionType ActionType,
    ApprovalStatus NewStatus,
    string ReviewedBy
) : IDomainEvent;
