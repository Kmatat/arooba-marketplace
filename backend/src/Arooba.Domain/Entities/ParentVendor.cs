using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a parent vendor (storefront).</summary>
public class ParentVendor : AuditableEntity
{
    public string BusinessNameAr { get; set; } = string.Empty;
    public string BusinessNameEn { get; set; } = string.Empty;
    public VendorType VendorType { get; set; }
    public VendorStatus Status { get; set; } = VendorStatus.Pending;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string GovernorateId { get; set; } = string.Empty;
    public bool IsVatRegistered { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public UpliftType? SubVendorUpliftType { get; set; }
    public decimal? SubVendorUpliftValue { get; set; }
    public Guid UserId { get; set; }
    public List<SubVendor>? SubVendors { get; set; }
}
