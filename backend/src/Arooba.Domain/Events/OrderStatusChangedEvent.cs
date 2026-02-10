using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Events;

/// <summary>
/// Domain event raised when an order's status transitions from one state to another.
/// Handlers may trigger shipment creation, payment processing, or customer notifications.
/// </summary>
/// <param name="OrderId">The unique identifier of the order.</param>
/// <param name="OldStatus">The previous order status.</param>
/// <param name="NewStatus">The new order status.</param>
public sealed record OrderStatusChangedEvent(
    int OrderId,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : IDomainEvent;
