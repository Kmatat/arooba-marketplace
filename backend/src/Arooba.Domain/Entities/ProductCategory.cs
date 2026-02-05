using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a product category in the Arooba Marketplace.
/// Uses string-based identifiers (e.g., "jewelry-accessories") for readability.
/// </summary>
public class ProductCategory
{
    public string Id { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public decimal MinUpliftRate { get; set; }
    public decimal MaxUpliftRate { get; set; }
    public decimal DefaultUpliftRate { get; set; }
    public string Risk { get; set; } = "medium";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation property to products in this category.</summary>
    public List<Product>? Products { get; set; }
}
