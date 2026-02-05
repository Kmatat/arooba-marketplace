using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Shipping.Queries;

/// <summary>
/// Query to calculate the shipping fee for given package parameters and delivery zone.
/// </summary>
public record CalculateShippingFeeQuery : IRequest<ShippingFeeResultDto>
{
    /// <summary>Gets the actual weight of the package in kilograms.</summary>
    public decimal ActualWeightKg { get; init; }

    /// <summary>Gets the package length in centimeters.</summary>
    public decimal LengthCm { get; init; }

    /// <summary>Gets the package width in centimeters.</summary>
    public decimal WidthCm { get; init; }

    /// <summary>Gets the package height in centimeters.</summary>
    public decimal HeightCm { get; init; }

    /// <summary>Gets the origin shipping zone identifier (pickup).</summary>
    public Guid OriginZoneId { get; init; }

    /// <summary>Gets the destination shipping zone identifier (delivery).</summary>
    public Guid DestinationZoneId { get; init; }
}

/// <summary>
/// DTO containing the complete shipping fee calculation result.
/// </summary>
public record ShippingFeeResultDto
{
    /// <summary>Gets the actual weight in kilograms.</summary>
    public decimal ActualWeightKg { get; init; }

    /// <summary>Gets the calculated volumetric weight in kilograms.</summary>
    public decimal VolumetricWeightKg { get; init; }

    /// <summary>Gets the chargeable weight (max of actual and volumetric).</summary>
    public decimal ChargeableWeightKg { get; init; }

    /// <summary>Gets the base shipping fee in EGP.</summary>
    public decimal BaseFee { get; init; }

    /// <summary>Gets the extra weight fee in EGP.</summary>
    public decimal ExtraWeightFee { get; init; }

    /// <summary>Gets the total shipping fee in EGP.</summary>
    public decimal TotalShippingFee { get; init; }

    /// <summary>Gets the Arooba subsidy amount in EGP.</summary>
    public decimal AroobaSubsidy { get; init; }

    /// <summary>Gets the customer-facing shipping fee in EGP.</summary>
    public decimal CustomerShippingFee { get; init; }

    /// <summary>Gets the estimated delivery days.</summary>
    public int EstimatedDeliveryDays { get; init; }

    /// <summary>Gets the origin zone name.</summary>
    public string OriginZoneName { get; init; } = default!;

    /// <summary>Gets the destination zone name.</summary>
    public string DestinationZoneName { get; init; } = default!;
}

/// <summary>
/// Handles shipping fee calculation using the Zone + Weight model.
/// Calculates volumetric weight, looks up zone-based rates, and applies any subsidies.
/// </summary>
public class CalculateShippingFeeQueryHandler : IRequestHandler<CalculateShippingFeeQuery, ShippingFeeResultDto>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// The volumetric divisor for shipping weight calculation.
    /// Standard industry divisor: 5000 cm3 = 1 kg.
    /// </summary>
    private const decimal VolumetricDivisor = 5000m;

    /// <summary>
    /// The base weight threshold in kg included in the base fee.
    /// </summary>
    private const decimal BaseWeightThresholdKg = 2m;

    /// <summary>
    /// Initializes a new instance of <see cref="CalculateShippingFeeQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CalculateShippingFeeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calculates the shipping fee by:
    /// 1. Computing volumetric weight from dimensions
    /// 2. Determining chargeable weight (max of actual vs volumetric)
    /// 3. Looking up zone-based rate card
    /// 4. Computing base fee + extra weight fee
    /// 5. Applying any Arooba subsidy
    /// </summary>
    /// <param name="request">The shipping fee query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A detailed shipping fee result DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when origin or destination zone is not found.</exception>
    public async Task<ShippingFeeResultDto> Handle(
        CalculateShippingFeeQuery request,
        CancellationToken cancellationToken)
    {
        // Validate zones exist
        var originZone = await _context.ShippingZones
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == request.OriginZoneId, cancellationToken);

        if (originZone is null)
        {
            throw new NotFoundException(nameof(ShippingZone), request.OriginZoneId);
        }

        var destinationZone = await _context.ShippingZones
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == request.DestinationZoneId, cancellationToken);

        if (destinationZone is null)
        {
            throw new NotFoundException(nameof(ShippingZone), request.DestinationZoneId);
        }

        // Look up applicable rate card
        var rateCard = await _context.RateCards
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.OriginZoneId == request.OriginZoneId &&
                r.DestinationZoneId == request.DestinationZoneId &&
                r.IsActive,
                cancellationToken);

        // Use default rates if no specific rate card found
        var baseFeeRate = rateCard?.BaseFee ?? 45m;  // Default 45 EGP base fee
        var perKgRate = rateCard?.PerKgRate ?? 10m;    // Default 10 EGP per extra kg

        // Step 1: Calculate volumetric weight
        var volumetricWeight = (request.LengthCm * request.WidthCm * request.HeightCm) / VolumetricDivisor;

        // Step 2: Determine chargeable weight
        var chargeableWeight = Math.Max(request.ActualWeightKg, volumetricWeight);
        chargeableWeight = Math.Ceiling(chargeableWeight); // Round up to nearest kg

        // Step 3: Calculate fees
        var baseFee = baseFeeRate;
        var extraWeight = Math.Max(0, chargeableWeight - BaseWeightThresholdKg);
        var extraWeightFee = extraWeight * perKgRate;
        var totalShippingFee = baseFee + extraWeightFee;

        // Step 4: Apply Arooba subsidy (e.g., subsidize first-time orders or promotions)
        var aroobaSubsidy = 0m;
        var customerShippingFee = totalShippingFee - aroobaSubsidy;

        return new ShippingFeeResultDto
        {
            ActualWeightKg = request.ActualWeightKg,
            VolumetricWeightKg = Math.Round(volumetricWeight, 2),
            ChargeableWeightKg = chargeableWeight,
            BaseFee = baseFee,
            ExtraWeightFee = extraWeightFee,
            TotalShippingFee = totalShippingFee,
            AroobaSubsidy = aroobaSubsidy,
            CustomerShippingFee = customerShippingFee,
            EstimatedDeliveryDays = destinationZone.EstimatedDeliveryDays,
            OriginZoneName = originZone.Name,
            DestinationZoneName = destinationZone.Name
        };
    }
}

/// <summary>
/// Validates the <see cref="CalculateShippingFeeQuery"/>.
/// </summary>
public class CalculateShippingFeeQueryValidator : AbstractValidator<CalculateShippingFeeQuery>
{
    /// <summary>
    /// Initializes validation rules for shipping fee calculation.
    /// </summary>
    public CalculateShippingFeeQueryValidator()
    {
        RuleFor(q => q.ActualWeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(q => q.LengthCm)
            .GreaterThan(0).WithMessage("Length must be greater than zero.");

        RuleFor(q => q.WidthCm)
            .GreaterThan(0).WithMessage("Width must be greater than zero.");

        RuleFor(q => q.HeightCm)
            .GreaterThan(0).WithMessage("Height must be greater than zero.");

        RuleFor(q => q.OriginZoneId)
            .NotEmpty().WithMessage("Origin zone ID is required.");

        RuleFor(q => q.DestinationZoneId)
            .NotEmpty().WithMessage("Destination zone ID is required.");
    }
}
