namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a shipping zone in the Arooba Marketplace.
/// Uses string-based identifiers (e.g., "cairo", "alexandria") for readability.
/// </summary>
public class ShippingZone
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public List<string> CitiesCovered { get; set; } = [];
    public int EstimatedDeliveryDays { get; set; } = 3;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
