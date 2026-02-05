using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Cooperative in the Arooba Marketplace domain.</summary>
public class Cooperative : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
