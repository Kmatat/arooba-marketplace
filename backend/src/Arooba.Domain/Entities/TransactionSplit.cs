using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a TransactionSplit in the Arooba Marketplace domain.</summary>
public class TransactionSplit : AuditableEntity
{
    public Guid OrderItemId { get; set; }
    public decimal BucketA { get; set; }
    public decimal BucketB { get; set; }
    public decimal BucketC { get; set; }
    public decimal BucketD { get; set; }
    public decimal BucketE { get; set; }
    public decimal TotalAmount { get; set; }

    /// <summary>Navigation property to the associated order item.</summary>
    public OrderItem? OrderItem { get; set; }
}
