using Arooba.Domain.Common;

namespace Arooba.Domain.Events;

/// <summary>
/// Domain event raised when a product is listed (submitted for review or activated) on the marketplace.
/// Handlers may trigger search index updates, admin review queues, or analytics.
/// </summary>
/// <param name="ProductId">The unique identifier of the listed product.</param>
public sealed record ProductListedEvent(int ProductId) : IDomainEvent;
