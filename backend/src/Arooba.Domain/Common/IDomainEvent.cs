using MediatR;

namespace Arooba.Domain.Common;

/// <summary>
/// Marker interface for domain events raised by entities.
/// Domain events are dispatched via MediatR after changes are persisted.
/// </summary>
public interface IDomainEvent : INotification;
