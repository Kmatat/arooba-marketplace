using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a LedgerEntry in the Arooba Marketplace domain.</summary>
public class LedgerEntry : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
