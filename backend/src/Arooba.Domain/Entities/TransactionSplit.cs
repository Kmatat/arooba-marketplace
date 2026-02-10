using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a 5-bucket financial split for an order line item.
/// Each bucket represents a portion of the payment going to different parties.
/// </summary>
public class TransactionSplit : AuditableEntity
{
    /// <summary>The order this split belongs to.</summary>
    public int OrderId { get; set; }

    /// <summary>The order item this split is for.</summary>
    public int OrderItemId { get; set; }

    /// <summary>The product being purchased.</summary>
    public int ProductId { get; set; }

    /// <summary>The parent vendor who will receive payment.</summary>
    public int ParentVendorId { get; set; }

    /// <summary>The sub-vendor who created the product (optional).</summary>
    public int? SubVendorId { get; set; }

    /// <summary>Gross amount of the transaction (total price).</summary>
    public decimal GrossAmount { get; set; }

    /// <summary>Bucket A: Vendor payout (base price + parent uplift).</summary>
    public decimal VendorPayoutBucket { get; set; }

    /// <summary>Bucket B: Arooba commission (marketplace uplift + cooperative fee).</summary>
    public decimal AroobaBucket { get; set; }

    /// <summary>Bucket C: Combined VAT amount.</summary>
    public decimal VatBucket { get; set; }

    /// <summary>Bucket D: Parent vendor uplift (for sub-vendor products).</summary>
    public decimal ParentUpliftBucket { get; set; }

    /// <summary>Bucket E: Withholding tax (if applicable).</summary>
    public decimal WithholdingTaxBucket { get; set; }

    /// <summary>Alias for VendorPayoutBucket for compatibility.</summary>
    public decimal BucketA
    {
        get => VendorPayoutBucket;
        set => VendorPayoutBucket = value;
    }

    /// <summary>Alias for ParentUpliftBucket for compatibility.</summary>
    public decimal BucketB
    {
        get => ParentUpliftBucket;
        set => ParentUpliftBucket = value;
    }

    /// <summary>Alias for AroobaBucket for compatibility.</summary>
    public decimal BucketC
    {
        get => AroobaBucket;
        set => AroobaBucket = value;
    }

    /// <summary>Alias for VatBucket for compatibility.</summary>
    public decimal BucketD
    {
        get => VatBucket;
        set => VatBucket = value;
    }

    /// <summary>Alias for WithholdingTaxBucket for compatibility.</summary>
    public decimal BucketE
    {
        get => WithholdingTaxBucket;
        set => WithholdingTaxBucket = value;
    }

    /// <summary>Total amount of all buckets.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Navigation property to the associated order item.</summary>
    public OrderItem? OrderItem { get; set; }

    /// <summary>Navigation property to the order.</summary>
    public Order? Order { get; set; }
}
