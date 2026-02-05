namespace Arooba.Domain.Enums;

/// <summary>
/// Determines how a sub-vendor's price uplift is calculated.
/// </summary>
public enum UpliftType
{
    /// <summary>A fixed monetary amount is added to the base price.</summary>
    Fixed,

    /// <summary>A percentage of the base price is added as uplift.</summary>
    Percentage
}
