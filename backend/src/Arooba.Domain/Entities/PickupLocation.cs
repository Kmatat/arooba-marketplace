using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a pickup location for a vendor in the Arooba Marketplace domain.</summary>
public class PickupLocation : AuditableEntity
{
    public Guid VendorId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Navigation property to the parent vendor.</summary>
    public ParentVendor? ParentVendor { get; set; }
}
