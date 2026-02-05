using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a sub-vendor operating under a parent vendor.</summary>
public class SubVendor : AuditableEntity
{
    public string BusinessNameAr { get; set; } = string.Empty;
    public string BusinessNameEn { get; set; } = string.Empty;
    public VendorType VendorType { get; set; }
    public VendorStatus Status { get; set; } = VendorStatus.Pending;
    public Guid ParentVendorId { get; set; }
    public ParentVendor? ParentVendor { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public UpliftType? UpliftType { get; set; }
    public decimal? UpliftValue { get; set; }
    public decimal? CustomUpliftOverride { get; set; }

    /// <summary>Navigation property to products listed by this sub-vendor.</summary>
    public List<Product>? Products { get; set; }
}
