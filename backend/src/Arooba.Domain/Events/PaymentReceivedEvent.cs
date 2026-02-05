using Arooba.Domain.Common;

namespace Arooba.Domain.Events;

/// <summary>
/// Domain event raised when a payment is successfully received for an order.
/// Handlers may trigger order confirmation, ledger entries, or vendor notifications.
/// </summary>
/// <param name="OrderId">The unique identifier of the order that was paid for.</param>
/// <param name="Amount">The payment amount received in EGP.</param>
public sealed record PaymentReceivedEvent(Guid OrderId, decimal Amount) : IDomainEvent;
