namespace Arooba.Domain.Enums;

/// <summary>
/// Classifies a vendor based on their legal registration status in Egypt.
/// </summary>
public enum VendorType
{
    /// <summary>Vendor has a commercial registration and tax identification.</summary>
    Legalized,

    /// <summary>Vendor operates informally without formal registration (e.g., artisan, home business).</summary>
    NonLegalized
}
