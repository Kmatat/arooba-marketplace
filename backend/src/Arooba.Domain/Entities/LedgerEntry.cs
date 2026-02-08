using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a financial ledger entry tracking vendor transactions.
/// </summary>
public class LedgerEntry : AuditableEntity
{
    /// <summary>The vendor this entry belongs to.</summary>
    public Guid VendorId { get; set; }

    /// <summary>Alias for VendorId for handler compatibility.</summary>
    public Guid ParentVendorId
    {
        get => VendorId;
        set => VendorId = value;
    }

    /// <summary>The order this entry is associated with (if applicable).</summary>
    public Guid? OrderId { get; set; }

    /// <summary>A unique transaction identifier.</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>The type of transaction (Sale, Payout, Refund, etc.).</summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>Total amount of this transaction.</summary>
    public decimal Amount { get; set; }

    /// <summary>Vendor's portion of the amount (Bucket A).</summary>
    public decimal VendorAmount { get; set; }

    /// <summary>Arooba's commission amount (Bucket C).</summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>VAT amount (Buckets B + D).</summary>
    public decimal VatAmount { get; set; }

    /// <summary>Balance after this transaction.</summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>Human-readable description of the transaction.</summary>
    public string? Description { get; set; }

    /// <summary>Status of the funds (Pending, Available, Withdrawn).</summary>
    public BalanceStatus BalanceStatus { get; set; }

    /// <summary>Navigation property to the order.</summary>
    public Order? Order { get; set; }

    /// <summary>Navigation property to the vendor.</summary>
    public ParentVendor? ParentVendor { get; set; }
}
