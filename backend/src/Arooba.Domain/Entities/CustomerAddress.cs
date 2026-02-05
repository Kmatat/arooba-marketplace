using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a CustomerAddress in the Arooba Marketplace domain.</summary>
public class CustomerAddress : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
