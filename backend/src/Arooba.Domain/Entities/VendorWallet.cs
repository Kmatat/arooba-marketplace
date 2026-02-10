using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a vendor's financial wallet for tracking balances, earnings, and payouts.
/// </summary>
public class VendorWallet : AuditableEntity
{
    /// <summary>The vendor this wallet belongs to.</summary>
    public int VendorId { get; set; }

    /// <summary>Alias for VendorId for handler compatibility.</summary>
    public int ParentVendorId
    {
        get => VendorId;
        set => VendorId = value;
    }

    /// <summary>Total balance (pending + available).</summary>
    public decimal TotalBalance { get; set; }

    /// <summary>Funds held in escrow (14-day hold).</summary>
    public decimal PendingBalance { get; set; }

    /// <summary>Funds available for withdrawal.</summary>
    public decimal AvailableBalance { get; set; }

    /// <summary>Total earnings over the lifetime of the account.</summary>
    public decimal LifetimeEarnings { get; set; }

    /// <summary>Alias for LifetimeEarnings for handler compatibility.</summary>
    public decimal TotalEarnings
    {
        get => LifetimeEarnings;
        set => LifetimeEarnings = value;
    }

    /// <summary>Total payouts disbursed to the vendor's bank account.</summary>
    public decimal TotalPayouts { get; set; }

    /// <summary>Navigation property to the parent vendor.</summary>
    public ParentVendor? ParentVendor { get; set; }
}
