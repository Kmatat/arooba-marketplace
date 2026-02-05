using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a VendorWallet in the Arooba Marketplace domain.</summary>
public class VendorWallet : AuditableEntity
{
    public Guid VendorId { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal LifetimeEarnings { get; set; }

    /// <summary>Navigation property to the parent vendor.</summary>
    public ParentVendor? ParentVendor { get; set; }
}
