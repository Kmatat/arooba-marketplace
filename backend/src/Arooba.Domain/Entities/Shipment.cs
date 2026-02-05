using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Shipment in the Arooba Marketplace domain.</summary>
public class Shipment : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
