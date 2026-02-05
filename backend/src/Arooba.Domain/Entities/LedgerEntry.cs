using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a LedgerEntry in the Arooba Marketplace domain.</summary>
public class LedgerEntry : AuditableEntity
{
    public Guid VendorId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
}
