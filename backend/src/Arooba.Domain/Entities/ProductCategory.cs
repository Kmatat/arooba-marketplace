using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a ProductCategory in the Arooba Marketplace domain.</summary>
public class ProductCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
