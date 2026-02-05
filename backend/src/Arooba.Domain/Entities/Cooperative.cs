using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Cooperative in the Arooba Marketplace domain.</summary>
public class Cooperative : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public decimal FeePercentage { get; set; }
}
