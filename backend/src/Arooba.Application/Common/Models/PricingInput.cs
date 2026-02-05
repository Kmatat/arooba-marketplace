namespace Arooba.Application.Common.Models;

/// <summary>
/// Input parameters for the Arooba pricing engine. Contains all information
/// needed to calculate the final customer-facing price and 5-bucket financial breakdown.
/// </summary>
/// <param name="VendorBasePrice">The base price set by the vendor in EGP.</param>
/// <param name="CategoryId">The product category identifier (determines commission rate).</param>
/// <param name="IsVendorVatRegistered">Whether the vendor is VAT-registered with the Egyptian Tax Authority.</param>
/// <param name="IsNonLegalizedVendor">Whether the vendor operates without formal commercial registration.</param>
/// <param name="ParentUpliftType">The type of parent vendor uplift (Fixed or Percentage), if applicable.</param>
/// <param name="ParentUpliftValue">The parent vendor uplift value, if applicable.</param>
/// <param name="CustomUpliftOverride">An optional custom uplift override that bypasses the standard calculation.</param>
public record PricingInput(
    decimal VendorBasePrice,
    string CategoryId,
    bool IsVendorVatRegistered,
    bool IsNonLegalizedVendor,
    string? ParentUpliftType,
    decimal? ParentUpliftValue,
    decimal? CustomUpliftOverride);
