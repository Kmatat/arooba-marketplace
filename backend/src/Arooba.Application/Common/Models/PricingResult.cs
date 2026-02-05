namespace Arooba.Application.Common.Models;

/// <summary>
/// Complete pricing breakdown output from the Arooba pricing engine.
/// Uses the 4-bucket financial model: A (vendor revenue), B (vendor VAT),
/// C (Arooba revenue), D (Arooba VAT).
/// </summary>
/// <param name="FinalPrice">The final customer-facing price in EGP (A + B + C + D).</param>
/// <param name="VendorBasePrice">The original base price set by the vendor.</param>
/// <param name="CooperativeFee">The cooperative fee for non-legalized vendors (5%).</param>
/// <param name="ParentUpliftAmount">The parent vendor uplift amount in EGP.</param>
/// <param name="MarketplaceUplift">The marketplace uplift amount in EGP.</param>
/// <param name="LogisticsSurcharge">The logistics surcharge in EGP.</param>
/// <param name="BucketA_VendorRevenue">Bucket A: vendor base price + parent uplift.</param>
/// <param name="BucketB_VendorVat">Bucket B: VAT on Bucket A (14% if vendor is VAT-registered).</param>
/// <param name="BucketC_AroobaRevenue">Bucket C: cooperative fee + marketplace uplift + logistics surcharge.</param>
/// <param name="BucketD_AroobaVat">Bucket D: VAT on Bucket C (always applied at 14%).</param>
/// <param name="VendorNetPayout">The net amount payable to the vendor.</param>
/// <param name="CommissionRate">The category commission rate applied (as a decimal).</param>
/// <param name="VatRate">The VAT rate applied (0.14 for Egypt).</param>
/// <param name="TotalVatAmount">Total VAT collected (Bucket B + Bucket D).</param>
/// <param name="AroobaTotalMargin">Arooba's total margin (Bucket C).</param>
/// <param name="EffectiveMarginPercent">Arooba's effective margin as a percentage of the final price.</param>
public record PricingResult(
    decimal FinalPrice,
    decimal VendorBasePrice,
    decimal CooperativeFee,
    decimal ParentUpliftAmount,
    decimal MarketplaceUplift,
    decimal LogisticsSurcharge,
    decimal BucketA_VendorRevenue,
    decimal BucketB_VendorVat,
    decimal BucketC_AroobaRevenue,
    decimal BucketD_AroobaVat,
    decimal VendorNetPayout,
    decimal CommissionRate,
    decimal VatRate,
    decimal TotalVatAmount,
    decimal AroobaTotalMargin,
    decimal EffectiveMarginPercent);
