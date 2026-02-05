namespace Arooba.Application.Common.Models;

/// <summary>
/// Input parameters for calculating shipping fees based on weight, dimensions,
/// and delivery zone within Egypt.
/// </summary>
/// <param name="ActualWeightKg">The actual weight of the package in kilograms.</param>
/// <param name="LengthCm">The package length in centimeters.</param>
/// <param name="WidthCm">The package width in centimeters.</param>
/// <param name="HeightCm">The package height in centimeters.</param>
/// <param name="OriginZoneId">The shipping zone of the pickup location.</param>
/// <param name="DestinationZoneId">The shipping zone of the delivery address.</param>
/// <param name="RateCardId">The rate card to use for fee calculation.</param>
public record ShippingFeeInput(
    decimal ActualWeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    string OriginZoneId,
    string DestinationZoneId,
    string RateCardId);
