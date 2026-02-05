using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a RateCard in the Arooba Marketplace domain.</summary>
public class RateCard : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
