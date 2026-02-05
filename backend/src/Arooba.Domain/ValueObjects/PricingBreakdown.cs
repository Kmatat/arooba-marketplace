namespace Arooba.Domain.ValueObjects;

/// <summary>
/// Represents the complete pricing breakdown for a product, including all fees, uplifts,
/// taxes, and the resulting revenue buckets for financial reconciliation.
/// </summary>
/// <remarks>
/// <para>The pricing flow is:</para>
/// <para>VendorBasePrice + CooperativeFee + ParentVendorUplift + MarketplaceUplift + LogisticsSurcharge + VAT = FinalPrice</para>
/// <para>Revenue is split into buckets:</para>
/// <list type="bullet">
///   <item><description>Bucket A: Vendor revenue (base price minus cooperative fee)</description></item>
///   <item><description>Bucket B: VAT collected on behalf of the vendor</description></item>
///   <item><description>Bucket C: Arooba marketplace revenue (commissions and uplifts)</description></item>
///   <item><description>Bucket D: VAT collected on behalf of Arooba</description></item>
/// </list>
/// </remarks>
public sealed record PricingBreakdown
{
    /// <summary>Gets the vendor's base price before any fees or uplifts.</summary>
    public decimal VendorBasePrice { get; init; }

    /// <summary>Gets the cooperative fee charged when the vendor belongs to a cooperative.</summary>
    public decimal CooperativeFee { get; init; }

    /// <summary>Gets the uplift applied by the parent vendor on sub-vendor products.</summary>
    public decimal ParentVendorUplift { get; init; }

    /// <summary>Gets the uplift applied by the Arooba marketplace.</summary>
    public decimal MarketplaceUplift { get; init; }

    /// <summary>Gets the logistics surcharge added based on weight and zone.</summary>
    public decimal LogisticsSurcharge { get; init; }

    /// <summary>Gets the VAT amount owed by the vendor (applicable to legalized vendors).</summary>
    public decimal VendorVat { get; init; }

    /// <summary>Gets the VAT amount owed by Arooba on its commission and uplift revenue.</summary>
    public decimal AroobaVat { get; init; }

    /// <summary>Gets the final customer-facing price inclusive of all fees and taxes.</summary>
    public decimal FinalPrice { get; init; }

    /// <summary>Gets Bucket A: the net revenue payable to the vendor after cooperative fees.</summary>
    public decimal BucketA_VendorRevenue { get; init; }

    /// <summary>Gets Bucket B: the VAT amount collected on behalf of the vendor.</summary>
    public decimal BucketB_VendorVat { get; init; }

    /// <summary>Gets Bucket C: the total Arooba marketplace revenue (uplifts and commissions).</summary>
    public decimal BucketC_AroobaRevenue { get; init; }

    /// <summary>Gets Bucket D: the VAT amount collected on behalf of Arooba.</summary>
    public decimal BucketD_AroobaVat { get; init; }
}
