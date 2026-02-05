using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of the Arooba Marketplace pricing engine.
/// This is THE critical business logic service that calculates product prices,
/// shipping fees, escrow releases, and price deviation flags.
///
/// <para>
/// <b>The Additive Uplift Model</b> works as follows:
/// <list type="number">
///   <item>Cooperative fee = vendorBasePrice * 0.05 (if non-legalized)</item>
///   <item>Price after coop = vendorBasePrice + cooperativeFee</item>
///   <item>Parent uplift = fixed or percentage of vendorBasePrice</item>
///   <item>Marketplace uplift = priceAfterCoop * categoryRate (or MVP flat 20%)</item>
///   <item>Minimum uplift rule: 15 EGP floor</item>
///   <item>Low-price rule: items under 100 EGP get max(calculated, 20 EGP)</item>
///   <item>Logistics surcharge = 10 EGP</item>
///   <item>Bucket A = vendorBasePrice + parentUplift</item>
///   <item>Bucket B = bucketA * 0.14 (if vendor VAT registered, else 0)</item>
///   <item>Bucket C = cooperativeFee + marketplaceUplift + logisticsSurcharge</item>
///   <item>Bucket D = bucketC * 0.14 (always)</item>
///   <item>Final price = A + B + C + D</item>
/// </list>
/// </para>
/// </summary>
public class PricingService : IPricingService
{
    /// <summary>Egyptian VAT rate: 14%.</summary>
    private const decimal VatRate = 0.14m;

    /// <summary>Cooperative fee rate for non-legalized vendors: 5%.</summary>
    private const decimal CooperativeFeeRate = 0.05m;

    /// <summary>MVP flat marketplace uplift rate: 20%.</summary>
    private const decimal MvpFlatRate = 0.20m;

    /// <summary>Minimum fixed uplift in EGP to protect margin on cheap items.</summary>
    private const decimal MinimumFixedUplift = 15m;

    /// <summary>Items priced below this threshold in EGP receive a fixed markup.</summary>
    private const decimal LowPriceThreshold = 100m;

    /// <summary>Fixed markup in EGP applied to items below the low-price threshold.</summary>
    private const decimal LowPriceFixedMarkup = 20m;

    /// <summary>Logistics surcharge (SmartCom buffer) in EGP.</summary>
    private const decimal LogisticsSurchargeAmount = 10m;

    /// <summary>Volumetric weight divisor: (L * W * H) / 5000.</summary>
    private const decimal VolumetricDivisor = 5000m;

    /// <summary>Number of escrow hold days after delivery confirmation.</summary>
    private const int EscrowHoldDays = 14;

    /// <summary>
    /// Category-specific default uplift rates.
    /// Falls back to <see cref="MvpFlatRate"/> when no category match is found.
    /// </summary>
    private static readonly Dictionary<string, decimal> CategoryDefaultRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jewelry-accessories"] = 0.15m,
        ["fashion-apparel"] = 0.22m,
        ["home-decor-fragile"] = 0.25m,
        ["home-decor-textiles"] = 0.20m,
        ["leather-goods"] = 0.20m,
        ["beauty-personal"] = 0.20m,
        ["furniture-woodwork"] = 0.15m,
        ["food-essentials"] = 0.12m,
    };

    /// <inheritdoc />
    public PricingResult CalculatePrice(PricingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var vendorBasePrice = input.VendorBasePrice;

        // Step 1: Calculate Cooperative Fee (only for non-legalized vendors)
        var cooperativeFee = input.IsNonLegalizedVendor
            ? Round(vendorBasePrice * CooperativeFeeRate)
            : 0m;

        var priceAfterCoop = vendorBasePrice + cooperativeFee;

        // Step 2: Calculate Parent Vendor Uplift (if sub-vendor product)
        var parentUplift = CalculateParentUplift(input);

        // Step 3: Determine uplift rate and calculate marketplace uplift
        decimal marketplaceUplift;
        decimal upliftRate;

        if (input.CustomUpliftOverride.HasValue)
        {
            marketplaceUplift = input.CustomUpliftOverride.Value;
            upliftRate = vendorBasePrice > 0
                ? marketplaceUplift / vendorBasePrice
                : 0m;
        }
        else
        {
            upliftRate = CategoryDefaultRates.TryGetValue(input.CategoryId, out var categoryRate)
                ? categoryRate
                : MvpFlatRate;

            marketplaceUplift = Round(priceAfterCoop * upliftRate);

            // Apply Minimum Uplift Rule (15 EGP floor)
            marketplaceUplift = Math.Max(marketplaceUplift, MinimumFixedUplift);

            // For items under the low-price threshold, use max(calculated, 20 EGP fixed)
            if (vendorBasePrice < LowPriceThreshold)
            {
                marketplaceUplift = Math.Max(marketplaceUplift, LowPriceFixedMarkup);
            }
        }

        // Step 4: Logistics Surcharge
        var logisticsSurcharge = LogisticsSurchargeAmount;

        // Step 5: Bucket A — Vendor Revenue (base price + parent uplift)
        var bucketA = vendorBasePrice + parentUplift;

        // Step 6: Bucket B — Vendor VAT (only if vendor is VAT registered)
        var bucketB = input.IsVendorVatRegistered ? Round(bucketA * VatRate) : 0m;

        // Step 7: Bucket C — Arooba Revenue (coop fee + marketplace uplift + logistics surcharge)
        var bucketC = cooperativeFee + marketplaceUplift + logisticsSurcharge;

        // Step 8: Bucket D — Arooba VAT (always applies to Arooba's share)
        var bucketD = Round(bucketC * VatRate);

        // Step 9: Final Price
        var finalPrice = bucketA + bucketB + bucketC + bucketD;

        // Step 10: Summary metrics
        var vendorNetPayout = vendorBasePrice;
        var totalVat = bucketB + bucketD;
        var aroobaTotalMargin = bucketC;
        var effectiveMarginPercent = finalPrice > 0
            ? Round(aroobaTotalMargin / finalPrice * 100m)
            : 0m;

        return new PricingResult(
            FinalPrice: finalPrice,
            VendorBasePrice: vendorBasePrice,
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

    /// <inheritdoc />
    public ShippingFeeResult CalculateShippingFee(ShippingFeeInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Volumetric weight = (L x W x H) / 5000
        var volumetricWeight = Round(
            (input.LengthCm * input.WidthCm * input.HeightCm) / VolumetricDivisor);

        // Chargeable weight = whichever is higher
        var chargeableWeight = Math.Max(input.ActualWeightKg, volumetricWeight);

        // Base fee for first kg, then extra per additional kg
        var baseFee = 30m;
        var extraWeightFee = chargeableWeight > 1
            ? Round((chargeableWeight - 1) * 10m)
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public PriceDeviationResult CheckPriceDeviation(
        decimal productPrice,
        decimal categoryAvgPrice,
        decimal threshold = 0.20m)
    {
        var deviationPercent = categoryAvgPrice > 0
            ? Math.Abs(productPrice - categoryAvgPrice) / categoryAvgPrice
            : 0m;
        var isFlagged = deviationPercent > threshold;

        return new PriceDeviationResult(
            ProductPrice: productPrice,
            CategoryAvgPrice: categoryAvgPrice,
            DeviationPercent: Round(deviationPercent, 4),
            Threshold: threshold,
            IsFlagged: isFlagged
        );
    }

    /// <inheritdoc />
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
            "percentage" => Round(
                input.VendorBasePrice * input.ParentUpliftValue.Value / 100m),
            _ => 0m
        };
    }

    private static decimal Round(decimal value, int decimals = 2)
    {
        return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
    }
}
