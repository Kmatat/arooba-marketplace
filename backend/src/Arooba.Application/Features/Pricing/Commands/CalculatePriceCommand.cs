using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.Pricing.Commands.CalculatePrice;

/// <summary>
/// Command to calculate the full price breakdown for a product using the Additive Uplift Model.
/// </summary>
public record CalculatePriceCommand : IRequest<PriceBreakdownDto>
{
    public decimal VendorBasePrice { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public bool IsVendorVatRegistered { get; init; }
    public bool IsNonLegalizedVendor { get; init; }
    public string? ParentUpliftType { get; init; }
    public decimal? ParentUpliftValue { get; init; }
    public decimal? CustomUpliftOverride { get; init; }
}

/// <summary>
/// DTO containing the complete pricing breakdown result.
/// </summary>
public record PriceBreakdownDto
{
    public decimal FinalPrice { get; init; }
    public decimal VendorBasePrice { get; init; }
    public decimal CooperativeFee { get; init; }
    public decimal ParentUpliftAmount { get; init; }
    public decimal MarketplaceUplift { get; init; }
    public decimal LogisticsSurcharge { get; init; }
    public decimal BucketA_VendorRevenue { get; init; }
    public decimal BucketB_VendorVat { get; init; }
    public decimal BucketC_AroobaRevenue { get; init; }
    public decimal BucketD_AroobaVat { get; init; }
    public decimal VendorNetPayout { get; init; }
    public decimal CommissionRate { get; init; }
    public decimal VatRate { get; init; }
    public decimal TotalVatAmount { get; init; }
    public decimal AroobaTotalMargin { get; init; }
    public decimal EffectiveMarginPercent { get; init; }
}

/// <summary>
/// Handles the price calculation by delegating to IPricingService.
/// </summary>
public class CalculatePriceCommandHandler : IRequestHandler<CalculatePriceCommand, PriceBreakdownDto>
{
    private readonly IPricingService _pricingService;

    public CalculatePriceCommandHandler(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    public Task<PriceBreakdownDto> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        var input = new PricingInput(
            VendorBasePrice: request.VendorBasePrice,
            CategoryId: request.CategoryId,
            IsVendorVatRegistered: request.IsVendorVatRegistered,
            IsNonLegalizedVendor: request.IsNonLegalizedVendor,
            ParentUpliftType: request.ParentUpliftType,
            ParentUpliftValue: request.ParentUpliftValue,
            CustomUpliftOverride: request.CustomUpliftOverride);

        var result = _pricingService.CalculatePrice(input);

        var dto = new PriceBreakdownDto
        {
            FinalPrice = result.FinalPrice,
            VendorBasePrice = result.VendorBasePrice,
            CooperativeFee = result.CooperativeFee,
            ParentUpliftAmount = result.ParentUpliftAmount,
            MarketplaceUplift = result.MarketplaceUplift,
            LogisticsSurcharge = result.LogisticsSurcharge,
            BucketA_VendorRevenue = result.BucketA_VendorRevenue,
            BucketB_VendorVat = result.BucketB_VendorVat,
            BucketC_AroobaRevenue = result.BucketC_AroobaRevenue,
            BucketD_AroobaVat = result.BucketD_AroobaVat,
            VendorNetPayout = result.VendorNetPayout,
            CommissionRate = result.CommissionRate,
            VatRate = result.VatRate,
            TotalVatAmount = result.TotalVatAmount,
            AroobaTotalMargin = result.AroobaTotalMargin,
            EffectiveMarginPercent = result.EffectiveMarginPercent
        };

        return Task.FromResult(dto);
    }
}

/// <summary>
/// Validates the CalculatePriceCommand.
/// </summary>
public class CalculatePriceCommandValidator : AbstractValidator<CalculatePriceCommand>
{
    public CalculatePriceCommandValidator()
    {
        RuleFor(c => c.VendorBasePrice)
            .GreaterThan(0).WithMessage("Vendor base price must be greater than 0.");

        RuleFor(c => c.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
