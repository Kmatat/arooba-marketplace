using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a TransactionSplit in the Arooba Marketplace domain.</summary>
public class TransactionSplit : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
