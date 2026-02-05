using Arooba.Domain.Common;

namespace Arooba.Domain.Events;

/// <summary>
/// Domain event raised when a new order is created on the marketplace.
/// Handlers may trigger notifications, inventory reservation, or analytics.
/// </summary>
/// <param name="OrderId">The unique identifier of the newly created order.</param>
public sealed record OrderCreatedEvent(Guid OrderId) : IDomainEvent;
