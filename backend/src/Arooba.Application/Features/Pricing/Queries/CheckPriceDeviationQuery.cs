using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.Pricing.Queries.CheckPriceDeviation;

/// <summary>
/// Query to check whether a product price deviates significantly from the category average.
/// </summary>
public record CheckPriceDeviationQuery : IRequest<PriceDeviationResultDto>
{
    public decimal ProductPrice { get; init; }
    public decimal CategoryAvgPrice { get; init; }
    public decimal Threshold { get; init; } = 0.20m;
}

/// <summary>
/// DTO for price deviation check result.
/// </summary>
public record PriceDeviationResultDto
{
    public bool IsFlagged { get; init; }
    public decimal DeviationPercent { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Handles price deviation checks using IPricingService.
/// </summary>
public class CheckPriceDeviationQueryHandler : IRequestHandler<CheckPriceDeviationQuery, PriceDeviationResultDto>
{
    private readonly IPricingService _pricingService;

    public CheckPriceDeviationQueryHandler(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    public Task<PriceDeviationResultDto> Handle(CheckPriceDeviationQuery request, CancellationToken cancellationToken)
    {
        var result = _pricingService.CheckPriceDeviation(
            request.ProductPrice,
            request.CategoryAvgPrice,
            request.Threshold);

        var direction = request.ProductPrice >= request.CategoryAvgPrice ? "above" : "below";

        var message = result.IsFlagged
            ? $"Product price is {result.DeviationPercent:F2}% {direction} category average. This product will be flagged for manual review."
            : $"Product price is within acceptable range ({result.DeviationPercent:F2}% {direction} category average).";

        var dto = new PriceDeviationResultDto
        {
            IsFlagged = result.IsFlagged,
            DeviationPercent = result.DeviationPercent,
            Direction = direction,
            Message = message
        };

        return Task.FromResult(dto);
    }
}

/// <summary>
/// Validates the CheckPriceDeviationQuery.
/// </summary>
public class CheckPriceDeviationQueryValidator : AbstractValidator<CheckPriceDeviationQuery>
{
    public CheckPriceDeviationQueryValidator()
    {
        RuleFor(q => q.ProductPrice)
            .GreaterThan(0).WithMessage("Product price must be greater than 0.");

        RuleFor(q => q.CategoryAvgPrice)
            .GreaterThan(0).WithMessage("Category average price must be greater than 0.");

        RuleFor(q => q.Threshold)
            .InclusiveBetween(0.01m, 1.0m).WithMessage("Threshold must be between 0.01 and 1.0.");
    }
}
