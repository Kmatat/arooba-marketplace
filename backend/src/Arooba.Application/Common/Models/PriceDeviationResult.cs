namespace Arooba.Application.Common.Models;

/// <summary>
/// Result of a price deviation check comparing a product price against category average.
/// </summary>
/// <param name="ProductPrice">The product price being checked.</param>
/// <param name="CategoryAvgPrice">The average price for the product category.</param>
/// <param name="DeviationPercent">The absolute deviation percentage from the average.</param>
/// <param name="Threshold">The threshold used for flagging.</param>
/// <param name="IsFlagged">Whether the price deviation exceeds the threshold.</param>
public record PriceDeviationResult(
    decimal ProductPrice,
    decimal CategoryAvgPrice,
    decimal DeviationPercent,
    decimal Threshold,
    bool IsFlagged);
