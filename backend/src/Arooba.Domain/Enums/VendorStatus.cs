namespace Arooba.Domain.Enums;

/// <summary>
/// Represents the current approval and operational status of a vendor.
/// </summary>
public enum VendorStatus
{
    /// <summary>Vendor registration is pending admin review.</summary>
    Pending,

    /// <summary>Vendor has been approved and is actively operating.</summary>
    Active,

    /// <summary>Vendor has been temporarily suspended by an administrator.</summary>
    Suspended,

    /// <summary>Vendor registration was rejected by an administrator.</summary>
    Rejected
}
