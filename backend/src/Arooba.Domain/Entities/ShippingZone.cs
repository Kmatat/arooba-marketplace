using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a ShippingZone in the Arooba Marketplace domain.</summary>
public class ShippingZone : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
