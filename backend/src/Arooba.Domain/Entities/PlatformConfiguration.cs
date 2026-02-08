using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Stores dynamic platform configuration entries that can be managed by admin.
/// Replaces hardcoded constants with admin-configurable values.
/// Each configuration entry has a unique key, a JSON value, and belongs to a category.
/// </summary>
public class PlatformConfiguration : AuditableEntity
{
    /// <summary>The unique configuration key (e.g., "uplift.mvpFlatRate", "escrow.holdDays").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>The configuration value stored as JSON string for flexibility.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>The category this configuration belongs to.</summary>
    public ConfigCategory Category { get; set; }

    /// <summary>Human-readable label for admin UI display.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Arabic label for admin UI display.</summary>
    public string LabelAr { get; set; } = string.Empty;

    /// <summary>Description of what this configuration controls.</summary>
    public string? Description { get; set; }

    /// <summary>Arabic description.</summary>
    public string? DescriptionAr { get; set; }

    /// <summary>The data type of the value (number, percentage, boolean, json, string).</summary>
    public string ValueType { get; set; } = "string";

    /// <summary>Default value used as fallback.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Minimum allowed value (for numeric types).</summary>
    public decimal? MinValue { get; set; }

    /// <summary>Maximum allowed value (for numeric types).</summary>
    public decimal? MaxValue { get; set; }

    /// <summary>Whether this configuration is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether changes to this config require super admin approval.</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Sort order for display in admin UI.</summary>
    public int SortOrder { get; set; }
}
