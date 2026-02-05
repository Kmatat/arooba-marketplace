using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Implementation of the Arooba Marketplace pricing engine.
/// This is THE critical business logic service that calculates product prices,
/// shipping fees, escrow releases, and price deviation flags.
///
/// <para>
/// <b>The Additive Uplift Model</b> works as follows:
/// <list type="number">
///   <item>Cooperative fee = vendorBasePrice * 0.05 (if non-legalized)</item>
///   <item>Price after coop = vendorBasePrice + cooperativeFee</item>
///   <item>Parent uplift = fixed or percentage of priceAfterCoop</item>
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
    // ──────────────────────────────────────────────
    // BUSINESS CONSTANTS
    // ──────────────────────────────────────────────

    /// <summary>Egyptian VAT rate: 14%.</summary>
    private const decimal VatRate = 0.14m;

    /// <summary>Cooperative fee rate for non-legalized vendors: 5%.</summary>
    private const decimal CooperativeFeeRate = 0.05m;

    /// <summary>MVP flat marketplace uplift rate: 20%.</summary>
    private const decimal MvpFlatRate = 0.20m;

    /// <summary>Fragile product uplift override: 25%.</summary>
    private const decimal FragileOverride = 0.25m;

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

    // ──────────────────────────────────────────────
    // CORE PRICING CALCULATION
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// Implements the complete Additive Uplift Model as specified in the Arooba business rules.
    /// All monetary calculations use <see cref="Math.Round(decimal, int, MidpointRounding)"/>
    /// with 2 decimal places and <see cref="MidpointRounding.AwayFromZero"/>.
    /// </remarks>
    public PricingResult CalculatePrice(PricingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var vendorBasePrice = input.VendorBasePrice;

        // Step 1: Calculate Cooperative Fee (only for non-legalized vendors)
        var cooperativeFee = input.IsNonLegalizedVendor
            ? vendorBasePrice * CooperativeFeeRate
            : 0m;

        var priceAfterCoop = vendorBasePrice + cooperativeFee;

        // Step 2: Calculate Parent Vendor Uplift (if sub-vendor product)
        var parentVendorUplift = 0m;
        if (input.ParentUpliftType.HasValue && input.ParentUpliftValue.HasValue && input.ParentUpliftValue.Value > 0)
        {
            parentVendorUplift = input.ParentUpliftType.Value == UpliftType.Fixed
                ? input.ParentUpliftValue.Value
                : priceAfterCoop * input.ParentUpliftValue.Value;
        }

        // Step 3: Calculate Marketplace Uplift
        var upliftRate = input.CustomUpliftOverride
            ?? (CategoryDefaultRates.TryGetValue(input.CategoryId, out var categoryRate)
                ? categoryRate
                : MvpFlatRate);

        var marketplaceUplift = priceAfterCoop * upliftRate;

        // Step 3b: Apply Minimum Uplift Rule (15 EGP floor)
        if (marketplaceUplift < MinimumFixedUplift)
        {
            marketplaceUplift = MinimumFixedUplift;
        }

        // Step 3c: For items under the low-price threshold, use max(calculated, 20 EGP fixed)
        if (vendorBasePrice < LowPriceThreshold)
        {
            marketplaceUplift = Math.Max(marketplaceUplift, LowPriceFixedMarkup);
        }

        // Step 4: Logistics Surcharge (SmartCom Buffer)
        var logisticsSurcharge = LogisticsSurchargeAmount;

        // Step 5: Bucket A — Vendor Revenue (base price + parent uplift)
        var bucketA = vendorBasePrice + parentVendorUplift;

        // Step 6: Bucket B — Vendor VAT (only if vendor is VAT registered)
        var bucketB = input.IsVendorVatRegistered ? bucketA * VatRate : 0m;

        // Step 7: Bucket C — Arooba Revenue (coop fee + marketplace uplift + logistics surcharge)
        var bucketC = cooperativeFee + marketplaceUplift + logisticsSurcharge;

        // Step 8: Bucket D — Arooba VAT (always applies to Arooba's share)
        var bucketD = bucketC * VatRate;

        // Step 9: Final Price (excluding delivery — that's Bucket E)
        var finalPrice = bucketA + bucketB + bucketC + bucketD;

        // Step 10: Calculate Arooba margins
        var aroobaGrossMargin = bucketC;
        var aroobaMarginPercent = finalPrice > 0
            ? (aroobaGrossMargin / finalPrice) * 100m
            : 0m;

        return new PricingResult
        {
            FinalPrice = Round(finalPrice),
            VendorBasePrice = vendorBasePrice,
            CooperativeFee = Round(cooperativeFee),
            ParentVendorUplift = Round(parentVendorUplift),
            MarketplaceUplift = Round(marketplaceUplift),
            LogisticsSurcharge = logisticsSurcharge,
            VendorVat = Round(bucketB),
            AroobaVat = Round(bucketD),
            BucketA_VendorRevenue = Round(bucketA),
            BucketB_VendorVat = Round(bucketB),
            BucketC_AroobaRevenue = Round(bucketC),
            BucketD_AroobaVat = Round(bucketD),
            AroobaGrossMargin = Round(aroobaGrossMargin),
            AroobaMarginPercent = Round(aroobaMarginPercent),
        };
    }

    // ──────────────────────────────────────────────
    // SHIPPING FEE CALCULATION
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// Uses the Zone + Weight model:
    /// <list type="number">
    ///   <item>Volumetric weight = (L * W * H) / 5000</item>
    ///   <item>Chargeable weight = max(actual, volumetric)</item>
    ///   <item>Fee = baseRate + max(0, chargeableWeight - 1) * perKgRate</item>
    ///   <item>Subsidized fee = max(totalFee - 10, totalFee * 0.75)</item>
    /// </list>
    /// </remarks>
    public ShippingFeeResult CalculateShippingFee(ShippingFeeInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Volumetric weight = (L x W x H) / 5000
        var volumetricWeight = (input.DimensionL * input.DimensionW * input.DimensionH) / VolumetricDivisor;

        // Chargeable weight = whichever is higher
        var chargeableWeight = Math.Max(input.ActualWeightKg, volumetricWeight);

        // Base fee + excess weight charge (first 1 kg included in base)
        var excessWeight = Math.Max(0m, chargeableWeight - 1m);
        var excessWeightFee = excessWeight * input.PerKgRate;
        var totalFee = input.BaseRate + excessWeightFee;

        // SmartCom Buffer: subsidize part of the fee
        // Customer pays the higher of (totalFee - 10 EGP) or (75% of totalFee)
        var subsidizedCustomerFee = Math.Max(
            totalFee - LogisticsSurchargeAmount,
            totalFee * 0.75m);

        var aroobaSubsidy = totalFee - subsidizedCustomerFee;

        return new ShippingFeeResult
        {
            ActualWeight = input.ActualWeightKg,
            VolumetricWeight = Round(volumetricWeight),
            ChargeableWeight = Round(chargeableWeight),
            BaseFee = input.BaseRate,
            ExcessWeightFee = Round(excessWeightFee),
            TotalFee = Round(totalFee),
            SubsidizedCustomerFee = Round(subsidizedCustomerFee),
            AroobaSubsidy = Round(aroobaSubsidy),
        };
    }

    // ──────────────────────────────────────────────
    // ESCROW CALCULATION
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// After delivery confirmation, funds sit in escrow for 14 days.
    /// If no return is initiated during this period, funds become withdrawable.
    /// </remarks>
    public EscrowReleaseResult CalculateEscrowRelease(DateTime deliveryDate)
    {
        var releaseDate = deliveryDate.AddDays(EscrowHoldDays);
        var now = DateTime.UtcNow;
        var daysRemaining = Math.Max(0, (int)Math.Ceiling((releaseDate - now).TotalDays));

        return new EscrowReleaseResult
        {
            ReleaseDate = releaseDate,
            DaysRemaining = daysRemaining,
            IsReleasable = daysRemaining == 0,
        };
    }

    // ──────────────────────────────────────────────
    // PRICE DEVIATION CHECK
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// Any product priced +/-20% from the category average is flagged for manual review.
    /// This protects customers from price-gouging and maintains marketplace integrity.
    /// </remarks>
    public PriceDeviationResult CheckPriceDeviation(
        decimal productPrice,
        decimal categoryAvgPrice,
        decimal threshold = 0.20m)
    {
        if (categoryAvgPrice == 0)
        {
            return new PriceDeviationResult
            {
                IsFlagged = false,
                Deviation = 0m,
                Direction = "normal",
            };
        }

        var deviation = (productPrice - categoryAvgPrice) / categoryAvgPrice;
        var isFlagged = Math.Abs(deviation) > threshold;

        var direction = deviation > threshold
            ? "above"
            : deviation < -threshold
                ? "below"
                : "normal";

        return new PriceDeviationResult
        {
            IsFlagged = isFlagged,
            Deviation = Round(deviation * 100m),
            Direction = direction,
        };
    }

    // ──────────────────────────────────────────────
    // FRIENDLY PRICE ROUNDING
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// Rounds up to the nearest 5 EGP for customer-friendly display pricing.
    /// For example, 46.50 EGP becomes 50 EGP.
    /// </remarks>
    public decimal RoundToFriendlyPrice(decimal price)
    {
        return Math.Ceiling(price / 5m) * 5m;
    }

    // ──────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Rounds a decimal value to 2 decimal places using banker's rounding away from zero,
    /// consistent with the TypeScript <c>Math.round(value * 100) / 100</c> approach.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value with 2 decimal places.</returns>
    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
