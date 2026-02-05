namespace Arooba.Domain.Enums;

/// <summary>
/// Represents the status of funds within a vendor's wallet.
/// </summary>
public enum BalanceStatus
{
    /// <summary>Funds are held in escrow and not yet available for withdrawal.</summary>
    Pending,

    /// <summary>Funds have cleared escrow and are available for withdrawal.</summary>
    Available,

    /// <summary>Funds have been disbursed to the vendor's bank account.</summary>
    Withdrawn
}
