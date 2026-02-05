namespace Arooba.Domain.Enums;

/// <summary>
/// Defines the delivery frequency for a customer's recurring subscription.
/// </summary>
public enum SubscriptionFrequency
{
    /// <summary>Delivery every week.</summary>
    Weekly,

    /// <summary>Delivery every two weeks.</summary>
    Biweekly,

    /// <summary>Delivery once per month.</summary>
    Monthly
}
