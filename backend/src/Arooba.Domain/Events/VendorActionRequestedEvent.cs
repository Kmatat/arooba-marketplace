using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Events;

/// <summary>
/// Raised when a vendor submits an action request that requires admin approval.
/// </summary>
public record VendorActionRequestedEvent(
    int RequestId,
    int VendorId,
    VendorActionType ActionType
) : IDomainEvent;
