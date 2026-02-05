using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a OrderItem in the Arooba Marketplace domain.</summary>
public class OrderItem : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
