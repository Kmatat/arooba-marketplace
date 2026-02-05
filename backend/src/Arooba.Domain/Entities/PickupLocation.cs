using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a PickupLocation in the Arooba Marketplace domain.</summary>
public class PickupLocation : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
