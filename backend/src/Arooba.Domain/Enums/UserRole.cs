namespace Arooba.Domain.Enums;

/// <summary>
/// Represents the role assigned to a user within the Arooba Marketplace platform.
/// </summary>
public enum UserRole
{
    /// <summary>A regular customer who browses and purchases products.</summary>
    Customer,

    /// <summary>A parent vendor who owns a storefront and manages sub-vendors.</summary>
    ParentVendor,

    /// <summary>A sub-vendor operating under a parent vendor.</summary>
    SubVendor,

    /// <summary>A super administrator with full platform access.</summary>
    AdminSuper,

    /// <summary>An administrator focused on financial operations.</summary>
    AdminFinance,

    /// <summary>An administrator focused on day-to-day operations.</summary>
    AdminOperations,

    /// <summary>An administrator focused on customer and vendor support.</summary>
    AdminSupport
}
