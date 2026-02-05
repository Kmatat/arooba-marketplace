namespace Arooba.Application.Common.Models;

/// <summary>
/// Result of a shipping fee calculation including weight breakdowns,
/// fees, and any applicable subsidy.
/// </summary>
/// <param name="ActualWeightKg">The actual weight of the package in kilograms.</param>
/// <param name="VolumetricWeightKg">The volumetric (dimensional) weight in kilograms.</param>
/// <param name="ChargeableWeightKg">The chargeable weight (max of actual vs volumetric).</param>
/// <param name="BaseFee">The base shipping fee before any adjustments in EGP.</param>
/// <param name="ExtraWeightFee">The additional fee for weight above the base threshold in EGP.</param>
/// <param name="TotalShippingFee">The total shipping fee charged to the customer in EGP.</param>
/// <param name="AroobaSubsidy">Any shipping subsidy absorbed by Arooba in EGP.</param>
/// <param name="VendorShippingContribution">Any shipping contribution from the vendor in EGP.</param>
/// <param name="CustomerShippingFee">The net shipping fee paid by the customer in EGP.</param>
public record ShippingFeeResult(
    decimal ActualWeightKg,
    decimal VolumetricWeightKg,
    decimal ChargeableWeightKg,
    decimal BaseFee,
    decimal ExtraWeightFee,
    decimal TotalShippingFee,
    decimal AroobaSubsidy,
    decimal VendorShippingContribution,
    decimal CustomerShippingFee);
