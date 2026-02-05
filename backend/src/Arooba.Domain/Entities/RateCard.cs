using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a shipping rate card defining fees between two zones.
/// </summary>
public class RateCard : AuditableEntity
{
    public string FromZoneId { get; set; } = string.Empty;
    public string ToZoneId { get; set; } = string.Empty;

    /// <summary>Alias for FromZoneId used by query handlers.</summary>
    public string OriginZoneId { get => FromZoneId; set => FromZoneId = value; }

    /// <summary>Alias for ToZoneId used by query handlers.</summary>
    public string DestinationZoneId { get => ToZoneId; set => ToZoneId = value; }

    public decimal BasePrice { get; set; }
    public decimal PricePerKg { get; set; }

    /// <summary>Alias for BasePrice used by query handlers.</summary>
    public decimal BaseFee { get => BasePrice; set => BasePrice = value; }

    /// <summary>Alias for PricePerKg used by query handlers.</summary>
    public decimal PerKgRate { get => PricePerKg; set => PricePerKg = value; }

    public bool IsActive { get; set; } = true;
}
