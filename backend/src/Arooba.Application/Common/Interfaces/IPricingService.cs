using Arooba.Domain.Enums;

namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for the Arooba Marketplace pricing engine.
/// Implements the Additive Uplift Model with 5-bucket payment waterfall,
/// shipping fee calculation, and escrow release logic.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculates the complete price breakdown for a product using the Additive Uplift Model.
    /// </summary>
    /// <param name="input">The pricing input parameters.</param>
    /// <returns>A detailed breakdown of all price components and the 5-bucket split.</returns>
    PricingResult CalculatePrice(PricingInput input);

    /// <summary>
    /// Calculates the shipping fee using the Zone + Weight model.
    /// </summary>
    /// <param name="input">The shipping fee input parameters.</param>
    /// <returns>A detailed breakdown of the shipping fee components.</returns>
    ShippingFeeResult CalculateShippingFee(ShippingFeeInput input);

    /// <summary>
    /// Determines when escrowed funds become available for vendor withdrawal.
    /// </summary>
    /// <param name="deliveryDate">The confirmed delivery date.</param>
    /// <returns>Escrow release details including the release date and remaining hold days.</returns>
    EscrowReleaseResult CalculateEscrowRelease(DateTime deliveryDate);

    /// <summary>
    /// Checks whether a product's price deviates significantly from the category average.
    /// </summary>
    /// <param name="productPrice">The product's current price.</param>
    /// <param name="categoryAvgPrice">The average price in the product's category.</param>
    /// <param name="threshold">The deviation threshold (default 0.20 = 20%).</param>
    /// <returns>Price deviation analysis result.</returns>
    PriceDeviationResult CheckPriceDeviation(decimal productPrice, decimal categoryAvgPrice, decimal threshold = 0.20m);

    /// <summary>
    /// Rounds a price up to the nearest customer-friendly increment (5 EGP).
    /// </summary>
    /// <param name="price">The raw price to round.</param>
    /// <returns>The customer-friendly rounded price.</returns>
    decimal RoundToFriendlyPrice(decimal price);
}

/// <summary>
/// Input parameters for the product pricing calculation.
/// </summary>
public sealed record PricingInput
{
    /// <summary>Gets the vendor's base price in EGP.</summary>
    public required decimal VendorBasePrice { get; init; }

    /// <summary>Gets the product category identifier.</summary>
    public required string CategoryId { get; init; }

    /// <summary>Gets whether the vendor is registered for Egyptian VAT.</summary>
    public required bool IsVendorVatRegistered { get; init; }

    /// <summary>Gets whether the vendor is non-legalized (requires cooperative).</summary>
    public required bool IsNonLegalizedVendor { get; init; }

    /// <summary>Gets the optional parent vendor uplift type.</summary>
    public UpliftType? ParentUpliftType { get; init; }

    /// <summary>Gets the optional parent vendor uplift value (fixed EGP or percentage).</summary>
    public decimal? ParentUpliftValue { get; init; }

    /// <summary>Gets an optional custom uplift override rate.</summary>
    public decimal? CustomUpliftOverride { get; init; }
}

/// <summary>
/// Complete price breakdown result from the Additive Uplift Model.
/// </summary>
public sealed record PricingResult
{
    /// <summary>Gets the final customer-facing price.</summary>
    public decimal FinalPrice { get; init; }

    /// <summary>Gets the original vendor base price.</summary>
    public decimal VendorBasePrice { get; init; }

    /// <summary>Gets the cooperative fee amount.</summary>
    public decimal CooperativeFee { get; init; }

    /// <summary>Gets the parent vendor uplift amount.</summary>
    public decimal ParentVendorUplift { get; init; }

    /// <summary>Gets the marketplace uplift amount.</summary>
    public decimal MarketplaceUplift { get; init; }

    /// <summary>Gets the logistics surcharge amount.</summary>
    public decimal LogisticsSurcharge { get; init; }

    /// <summary>Gets the VAT on the vendor's portion.</summary>
    public decimal VendorVat { get; init; }

    /// <summary>Gets the VAT on Arooba's portion.</summary>
    public decimal AroobaVat { get; init; }

    /// <summary>Gets Bucket A: vendor revenue (base + parent uplift).</summary>
    public decimal BucketA_VendorRevenue { get; init; }

    /// <summary>Gets Bucket B: vendor VAT.</summary>
    public decimal BucketB_VendorVat { get; init; }

    /// <summary>Gets Bucket C: Arooba revenue (coop + uplift + logistics).</summary>
    public decimal BucketC_AroobaRevenue { get; init; }

    /// <summary>Gets Bucket D: Arooba VAT.</summary>
    public decimal BucketD_AroobaVat { get; init; }

    /// <summary>Gets Arooba's gross margin before VAT and costs.</summary>
    public decimal AroobaGrossMargin { get; init; }

    /// <summary>Gets Arooba's margin as a percentage of the final price.</summary>
    public decimal AroobaMarginPercent { get; init; }
}

/// <summary>
/// Input parameters for shipping fee calculation.
/// </summary>
public sealed record ShippingFeeInput
{
    /// <summary>Gets the actual weight of the package in kilograms.</summary>
    public required decimal ActualWeightKg { get; init; }

    /// <summary>Gets the package length in centimeters.</summary>
    public required decimal DimensionL { get; init; }

    /// <summary>Gets the package width in centimeters.</summary>
    public required decimal DimensionW { get; init; }

    /// <summary>Gets the package height in centimeters.</summary>
    public required decimal DimensionH { get; init; }

    /// <summary>Gets the origin shipping zone identifier.</summary>
    public required string FromZoneId { get; init; }

    /// <summary>Gets the destination shipping zone identifier.</summary>
    public required string ToZoneId { get; init; }

    /// <summary>Gets the base rate for this zone pair in EGP.</summary>
    public required decimal BaseRate { get; init; }

    /// <summary>Gets the per-kilogram rate for excess weight.</summary>
    public required decimal PerKgRate { get; init; }
}

/// <summary>
/// Detailed shipping fee calculation result.
/// </summary>
public sealed record ShippingFeeResult
{
    /// <summary>Gets the actual weight in kilograms.</summary>
    public decimal ActualWeight { get; init; }

    /// <summary>Gets the calculated volumetric weight.</summary>
    public decimal VolumetricWeight { get; init; }

    /// <summary>Gets the chargeable weight (max of actual and volumetric).</summary>
    public decimal ChargeableWeight { get; init; }

    /// <summary>Gets the base fee component.</summary>
    public decimal BaseFee { get; init; }

    /// <summary>Gets the excess weight fee component.</summary>
    public decimal ExcessWeightFee { get; init; }

    /// <summary>Gets the total shipping fee before subsidy.</summary>
    public decimal TotalFee { get; init; }

    /// <summary>Gets the customer-facing fee after SmartCom buffer subsidy.</summary>
    public decimal SubsidizedCustomerFee { get; init; }

    /// <summary>Gets the amount subsidized by Arooba.</summary>
    public decimal AroobaSubsidy { get; init; }
}

/// <summary>
/// Result of the escrow release calculation.
/// </summary>
public sealed record EscrowReleaseResult
{
    /// <summary>Gets the date when escrowed funds will be released.</summary>
    public DateTime ReleaseDate { get; init; }

    /// <summary>Gets the number of days remaining until release.</summary>
    public int DaysRemaining { get; init; }

    /// <summary>Gets whether the funds are currently releasable.</summary>
    public bool IsReleasable { get; init; }
}

/// <summary>
/// Result of the price deviation analysis.
/// </summary>
public sealed record PriceDeviationResult
{
    /// <summary>Gets whether the price is flagged for manual review.</summary>
    public bool IsFlagged { get; init; }

    /// <summary>Gets the deviation percentage from the category average.</summary>
    public decimal Deviation { get; init; }

    /// <summary>Gets the direction of deviation (above, below, or normal).</summary>
    public string Direction { get; init; } = "normal";
}
