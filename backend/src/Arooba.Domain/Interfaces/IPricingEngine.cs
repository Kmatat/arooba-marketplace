using Arooba.Domain.ValueObjects;

namespace Arooba.Domain.Interfaces;

/// <summary>
/// Defines the pricing engine contract for the Arooba Marketplace.
/// Responsible for calculating product prices, shipping fees, escrow release timing,
/// and detecting price anomalies.
/// </summary>
public interface IPricingEngine
{
    /// <summary>
    /// Calculates the full pricing breakdown for a product, including vendor base price,
    /// cooperative fees, uplifts, VAT, and the final customer-facing price.
    /// </summary>
    /// <param name="input">The pricing input parameters.</param>
    /// <returns>A <see cref="PricingBreakdown"/> with all price components and revenue bucket allocations.</returns>
    PricingBreakdown CalculatePrice(PricingInput input);

    /// <summary>
    /// Calculates the shipping fee for a delivery based on origin and destination zones,
    /// package weight, and volumetric weight.
    /// </summary>
    /// <param name="input">The shipping fee input parameters.</param>
    /// <returns>A <see cref="ShippingFeeResult"/> containing the calculated fee.</returns>
    ShippingFeeResult CalculateShippingFee(ShippingFeeInput input);

    /// <summary>
    /// Determines when escrowed funds should be released to the vendor after delivery.
    /// </summary>
    /// <param name="deliveryDate">The confirmed delivery date.</param>
    /// <returns>An <see cref="EscrowResult"/> with the release date and status.</returns>
    EscrowResult CalculateEscrowRelease(DateTime deliveryDate);

    /// <summary>
    /// Checks whether a product's price deviates significantly from the category average,
    /// which may indicate a pricing error or fraud.
    /// </summary>
    /// <param name="productPrice">The product's listed price.</param>
    /// <param name="categoryAvgPrice">The average price for products in the same category.</param>
    /// <param name="threshold">The maximum allowed deviation as a decimal (default 0.20 = 20%).</param>
    /// <returns>A <see cref="PriceDeviationResult"/> indicating whether the price is within acceptable bounds.</returns>
    PriceDeviationResult CheckPriceDeviation(decimal productPrice, decimal categoryAvgPrice, decimal threshold = 0.20m);
}

/// <summary>
/// Input parameters for product price calculation.
/// </summary>
public record PricingInput
{
    /// <summary>Gets the vendor's base selling price.</summary>
    public decimal VendorBasePrice { get; init; }

    /// <summary>Gets the cooperative fee percentage (e.g., 0.05 for 5%).</summary>
    public decimal CooperativeFeePercentage { get; init; }

    /// <summary>Gets the parent vendor uplift amount or percentage.</summary>
    public decimal ParentVendorUplift { get; init; }

    /// <summary>Gets the marketplace uplift percentage.</summary>
    public decimal MarketplaceUpliftPercentage { get; init; }

    /// <summary>Gets the logistics surcharge amount.</summary>
    public decimal LogisticsSurcharge { get; init; }

    /// <summary>Gets a value indicating whether the vendor is VAT-registered.</summary>
    public bool IsVatRegistered { get; init; }

    /// <summary>Gets the VAT rate (e.g., 0.14 for 14% in Egypt).</summary>
    public decimal VatRate { get; init; } = 0.14m;
}

/// <summary>
/// Input parameters for shipping fee calculation.
/// </summary>
public record ShippingFeeInput
{
    /// <summary>Gets the origin shipping zone identifier.</summary>
    public string FromZoneId { get; init; } = null!;

    /// <summary>Gets the destination shipping zone identifier.</summary>
    public string ToZoneId { get; init; } = null!;

    /// <summary>Gets the actual weight of the package in kilograms.</summary>
    public decimal WeightKg { get; init; }

    /// <summary>Gets the volumetric weight of the package in kilograms.</summary>
    public decimal? VolumetricWeightKg { get; init; }
}

/// <summary>
/// Result of a shipping fee calculation.
/// </summary>
public record ShippingFeeResult
{
    /// <summary>Gets the calculated shipping fee in EGP.</summary>
    public decimal Fee { get; init; }

    /// <summary>Gets the billable weight used for calculation (max of actual and volumetric).</summary>
    public decimal BillableWeightKg { get; init; }
}

/// <summary>
/// Result of an escrow release calculation.
/// </summary>
public record EscrowResult
{
    /// <summary>Gets the date when escrowed funds will be released to the vendor.</summary>
    public DateTime ReleaseDate { get; init; }

    /// <summary>Gets a value indicating whether the escrow hold period has elapsed.</summary>
    public bool IsReleasable { get; init; }
}

/// <summary>
/// Result of a price deviation check.
/// </summary>
public record PriceDeviationResult
{
    /// <summary>Gets a value indicating whether the price is within acceptable deviation bounds.</summary>
    public bool IsWithinBounds { get; init; }

    /// <summary>Gets the calculated deviation percentage.</summary>
    public decimal DeviationPercentage { get; init; }

    /// <summary>Gets a descriptive message about the deviation analysis.</summary>
    public string Message { get; init; } = null!;
}
