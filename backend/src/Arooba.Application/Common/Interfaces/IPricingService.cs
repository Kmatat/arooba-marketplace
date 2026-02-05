using Arooba.Application.Common.Models;

namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Application-layer contract for the Arooba Marketplace pricing engine.
/// Uses the Application DTOs (<see cref="Models.PricingInput"/>, <see cref="Models.PricingResult"/>, etc.)
/// and implements the Additive Uplift Model with 4-bucket payment waterfall,
/// shipping fee calculation, and escrow release logic.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculates the complete price breakdown for a product using the Additive Uplift Model.
    /// </summary>
    /// <param name="input">The pricing input parameters.</param>
    /// <returns>A detailed breakdown of all price components and the 4-bucket split.</returns>
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
    /// <returns>Escrow release details including the release date and hold status.</returns>
    EscrowResult CalculateEscrowRelease(DateTime deliveryDate);

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
