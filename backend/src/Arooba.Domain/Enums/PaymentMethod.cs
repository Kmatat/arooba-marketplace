namespace Arooba.Domain.Enums;

/// <summary>
/// Represents the available payment methods on the Arooba Marketplace.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Cash on delivery.</summary>
    Cod,

    /// <summary>Payment via the Fawry electronic payment network.</summary>
    Fawry,

    /// <summary>Credit or debit card payment.</summary>
    Card,

    /// <summary>Payment from the customer's Arooba wallet balance.</summary>
    Wallet
}
