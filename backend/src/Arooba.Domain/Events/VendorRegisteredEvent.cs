using Arooba.Domain.Common;

namespace Arooba.Domain.Events;

/// <summary>
/// Domain event raised when a new vendor registers on the marketplace.
/// Handlers may trigger welcome communications, admin review workflows, or analytics.
/// </summary>
/// <param name="VendorId">The unique identifier of the newly registered vendor.</param>
public sealed record VendorRegisteredEvent(Guid VendorId) : IDomainEvent;
