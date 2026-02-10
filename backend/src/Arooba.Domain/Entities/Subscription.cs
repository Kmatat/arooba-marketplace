using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Subscription in the Arooba Marketplace domain.</summary>
public class Subscription : AuditableEntity
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SubscriptionFrequency Frequency { get; set; }
    public string? ItemsJson { get; set; }

    /// <summary>Navigation property to the subscribing customer.</summary>
    public Customer? Customer { get; set; }
}
