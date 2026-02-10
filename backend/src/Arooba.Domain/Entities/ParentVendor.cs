using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a parent vendor (storefront).</summary>
public class ParentVendor : AuditableEntity
{
    public string BusinessName { get; set; } = null!;
    public string BusinessNameEn { get; set; } = null!;
    public VendorType VendorType { get; set; }
    public VendorStatus Status { get; set; } = VendorStatus.Pending;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public string City { get; set; } = null!;
    public string GovernorateId { get; set; } = null!;
    public bool IsVatRegistered { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public UpliftType? SubVendorUpliftType { get; set; }
    public decimal? SubVendorUpliftValue { get; set; }
    public string BankName { get; set; } = null!;
    public int UserId { get; set; }
    public decimal CommissionRate { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;
    public List<SubVendor>? SubVendors { get; set; }

    /// <summary>Navigation property to products listed by this vendor.</summary>
    public List<Product>? Products { get; set; }

    /// <summary>Navigation property to pickup locations for this vendor.</summary>
    public List<PickupLocation>? PickupLocations { get; set; }
}
