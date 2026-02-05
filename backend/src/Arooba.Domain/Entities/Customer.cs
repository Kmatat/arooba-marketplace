using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Customer in the Arooba Marketplace domain.</summary>
public class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
