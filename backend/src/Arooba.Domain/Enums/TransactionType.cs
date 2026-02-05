namespace Arooba.Domain.Enums;

/// <summary>
/// Classifies financial transactions flowing through the platform ledger.
/// </summary>
public enum TransactionType
{
    /// <summary>Revenue from a product sale.</summary>
    Sale,

    /// <summary>Commission charged by the marketplace.</summary>
    Commission,

    /// <summary>Value-added tax collected.</summary>
    Vat,

    /// <summary>Shipping fee charged to the customer.</summary>
    Shipping,

    /// <summary>Refund issued to the customer.</summary>
    Refund,

    /// <summary>Payout disbursed to a vendor.</summary>
    Payout
}
