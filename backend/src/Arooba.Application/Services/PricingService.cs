using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;

namespace Arooba.Application.Services;

/// <summary>
/// Concrete implementation of the Arooba pricing engine.
/// Calculates final customer-facing prices using the 4-bucket financial model,
/// shipping fees, escrow release dates, and price deviation checks.
/// </summary>
public class PricingService : IPricingService
{
    private const decimal VatRate = 0.14m;
    private const decimal CooperativeFeeRate = 0.05m;
    private const decimal MvpFlatUpliftRate = 0.20m;
    private const decimal FragileUpliftRate = 0.25m;
    private const decimal JewelryUpliftRate = 0.15m;
    private const decimal MinimumFixedUplift = 15m;
    private const decimal LowPriceThreshold = 100m;
    private const decimal LowPriceFixedMarkup = 20m;
    private const decimal LogisticsSurchargeAmount = 10m;
    private const decimal VolumetricDivisor = 5000m;
    private const int EscrowHoldDays = 14;
    private const decimal DefaultDeviationThreshold = 0.20m;

    /// <summary>
    /// Calculates the full pricing breakdown for a product using the 4-bucket model.
    /// </summary>
    /// <param name="input">The pricing input parameters.</param>
    /// <returns>A <see cref="PricingResult"/> with the complete breakdown.</returns>
    public PricingResult CalculatePrice(PricingInput input)
    {
        // 1. Determine category uplift rate
        var upliftRate = GetCategoryUpliftRate(input.CategoryId);

        // 2. Calculate cooperative fee (non-legalized vendors only)
        var cooperativeFee = input.IsNonLegalizedVendor
            ? Math.Round(input.VendorBasePrice * CooperativeFeeRate, 2)
            : 0m;

        // 3. Calculate parent vendor uplift
        var parentUplift = CalculateParentUplift(input);

        // 4. Price after cooperative fee (base for marketplace uplift calculation)
        var priceAfterCoop = input.VendorBasePrice + cooperativeFee;

        // 5. Calculate marketplace uplift
        decimal marketplaceUplift;
        if (input.CustomUpliftOverride.HasValue)
        {
            marketplaceUplift = input.CustomUpliftOverride.Value;
        }
        else
        {
            marketplaceUplift = Math.Round(priceAfterCoop * upliftRate, 2);

            // Apply minimum uplift floor
            marketplaceUplift = Math.Max(marketplaceUplift, MinimumFixedUplift);

            // Apply low price threshold markup
            if (input.VendorBasePrice < LowPriceThreshold)
            {
                marketplaceUplift = Math.Max(marketplaceUplift, LowPriceFixedMarkup);
            }
        }

        // 6. Logistics surcharge
        var logisticsSurcharge = LogisticsSurchargeAmount;

        // 7. Calculate 4-bucket breakdown
        var bucketA = input.VendorBasePrice + parentUplift;
        var bucketB = input.IsVendorVatRegistered ? Math.Round(bucketA * VatRate, 2) : 0m;
        var bucketC = cooperativeFee + marketplaceUplift + logisticsSurcharge;
        var bucketD = Math.Round(bucketC * VatRate, 2);

        // 8. Final price
        var finalPrice = bucketA + bucketB + bucketC + bucketD;

        // 9. Vendor net payout
        var vendorNetPayout = input.VendorBasePrice;

        // 10. Summary metrics
        var totalVat = bucketB + bucketD;
        var aroobaTotalMargin = bucketC;
        var effectiveMarginPercent = finalPrice > 0
            ? Math.Round(aroobaTotalMargin / finalPrice * 100, 2)
            : 0m;

        return new PricingResult(
            FinalPrice: finalPrice,
            VendorBasePrice: input.VendorBasePrice,
            CooperativeFee: cooperativeFee,
            ParentUpliftAmount: parentUplift,
            MarketplaceUplift: marketplaceUplift,
            LogisticsSurcharge: logisticsSurcharge,
            BucketA_VendorRevenue: bucketA,
            BucketB_VendorVat: bucketB,
            BucketC_AroobaRevenue: bucketC,
            BucketD_AroobaVat: bucketD,
            VendorNetPayout: vendorNetPayout,
            CommissionRate: upliftRate,
            VatRate: VatRate,
            TotalVatAmount: totalVat,
            AroobaTotalMargin: aroobaTotalMargin,
            EffectiveMarginPercent: effectiveMarginPercent
        );
    }

    /// <summary>
    /// Calculates shipping fees based on actual vs volumetric weight.
    /// </summary>
    /// <param name="input">The shipping fee input parameters.</param>
    /// <returns>A <see cref="ShippingFeeResult"/> with the fee breakdown.</returns>
    public ShippingFeeResult CalculateShippingFee(ShippingFeeInput input)
    {
        var volumetricWeight = Math.Round(
            (input.LengthCm * input.WidthCm * input.HeightCm) / VolumetricDivisor, 2);
        var chargeableWeight = Math.Max(input.ActualWeightKg, volumetricWeight);

        // Base fee for first kg, then extra per additional kg
        var baseFee = 30m;
        var extraWeightFee = chargeableWeight > 1
            ? Math.Round((chargeableWeight - 1) * 10m, 2)
            : 0m;
        var totalFee = baseFee + extraWeightFee;

        return new ShippingFeeResult(
            ActualWeightKg: input.ActualWeightKg,
            VolumetricWeightKg: volumetricWeight,
            ChargeableWeightKg: chargeableWeight,
            BaseFee: baseFee,
            ExtraWeightFee: extraWeightFee,
            TotalShippingFee: totalFee,
            AroobaSubsidy: 0m,
            VendorShippingContribution: 0m,
            CustomerShippingFee: totalFee
        );
    }

    /// <summary>
    /// Calculates when escrowed funds should be released (14-day hold from delivery).
    /// </summary>
    /// <param name="deliveryDate">The date the order was delivered.</param>
    /// <returns>An <see cref="EscrowResult"/> with release timing information.</returns>
    public EscrowResult CalculateEscrowRelease(DateTime deliveryDate)
    {
        var releaseDate = deliveryDate.AddDays(EscrowHoldDays);
        var isReleased = DateTime.UtcNow >= releaseDate;

        return new EscrowResult(
            DeliveryDate: deliveryDate,
            ReleaseDate: releaseDate,
            HoldDays: EscrowHoldDays,
            IsReleased: isReleased
        );
    }

    /// <summary>
    /// Checks whether a product price deviates significantly from the category average.
    /// </summary>
    /// <param name="productPrice">The product price to check.</param>
    /// <param name="categoryAvgPrice">The average price for the category.</param>
    /// <param name="threshold">The deviation threshold (default 20%).</param>
    /// <returns>A <see cref="PriceDeviationResult"/> indicating whether the price is flagged.</returns>
    public PriceDeviationResult CheckPriceDeviation(
        decimal productPrice,
        decimal categoryAvgPrice,
        decimal threshold = DefaultDeviationThreshold)
    {
        var deviationPercent = categoryAvgPrice > 0
            ? Math.Abs(productPrice - categoryAvgPrice) / categoryAvgPrice
            : 0m;
        var isFlagged = deviationPercent > threshold;

        return new PriceDeviationResult(
            ProductPrice: productPrice,
            CategoryAvgPrice: categoryAvgPrice,
            DeviationPercent: Math.Round(deviationPercent, 4),
            Threshold: threshold,
            IsFlagged: isFlagged
        );
    }

    private static decimal GetCategoryUpliftRate(string categoryId)
    {
        return categoryId.ToLowerInvariant() switch
        {
            var c when c.Contains("jewelry") => JewelryUpliftRate,
            var c when c.Contains("fragile") => FragileUpliftRate,
            _ => MvpFlatUpliftRate
        };
    }

    /// <summary>
    /// Rounds a price up to the nearest customer-friendly increment (5 EGP).
    /// </summary>
    /// <param name="price">The raw price to round.</param>
    /// <returns>The customer-friendly rounded price.</returns>
    public decimal RoundToFriendlyPrice(decimal price)
    {
        return Math.Ceiling(price / 5m) * 5m;
    }

    private static decimal CalculateParentUplift(PricingInput input)
    {
        if (input.ParentUpliftType is null || !input.ParentUpliftValue.HasValue)
            return 0m;

        return input.ParentUpliftType.ToLowerInvariant() switch
        {
            "fixed" => input.ParentUpliftValue.Value,
            "percentage" => Math.Round(
                input.VendorBasePrice * input.ParentUpliftValue.Value / 100m, 2),
            _ => 0m
        };
    }
}
