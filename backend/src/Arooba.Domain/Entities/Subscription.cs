using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Subscription in the Arooba Marketplace domain.</summary>
public class Subscription : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
