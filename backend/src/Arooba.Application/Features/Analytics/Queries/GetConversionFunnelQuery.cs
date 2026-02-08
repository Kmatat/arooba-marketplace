using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Analytics.Queries;

/// <summary>
/// Query to retrieve conversion funnel data showing drop-off at each stage.
/// </summary>
public record GetConversionFunnelQuery : IRequest<ConversionFunnelDto>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

/// <summary>
/// Conversion funnel DTO with counts at each stage of the purchase journey.
/// </summary>
public record ConversionFunnelDto
{
    public int ProductViews { get; init; }
    public int AddedToCart { get; init; }
    public int CheckoutsStarted { get; init; }
    public int PurchasesCompleted { get; init; }
    public decimal ViewToCartRate { get; init; }
    public decimal CartToCheckoutRate { get; init; }
    public decimal CheckoutToCompletionRate { get; init; }
    public decimal OverallConversionRate { get; init; }
}

public class GetConversionFunnelQueryHandler : IRequestHandler<GetConversionFunnelQuery, ConversionFunnelDto>
{
    private readonly IApplicationDbContext _context;

    public GetConversionFunnelQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversionFunnelDto> Handle(
        GetConversionFunnelQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.UserActivities.AsNoTracking().AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= request.DateTo.Value);

        var productViews = await query.CountAsync(a => a.Action == UserActivityAction.ProductViewed, cancellationToken);
        var addedToCart = await query.CountAsync(a => a.Action == UserActivityAction.AddedToCart, cancellationToken);
        var checkoutsStarted = await query.CountAsync(a => a.Action == UserActivityAction.CheckoutStarted, cancellationToken);
        var purchasesCompleted = await query.CountAsync(a => a.Action == UserActivityAction.PurchaseCompleted, cancellationToken);

        return new ConversionFunnelDto
        {
            ProductViews = productViews,
            AddedToCart = addedToCart,
            CheckoutsStarted = checkoutsStarted,
            PurchasesCompleted = purchasesCompleted,
            ViewToCartRate = productViews > 0 ? Math.Round((decimal)addedToCart / productViews * 100, 2) : 0,
            CartToCheckoutRate = addedToCart > 0 ? Math.Round((decimal)checkoutsStarted / addedToCart * 100, 2) : 0,
            CheckoutToCompletionRate = checkoutsStarted > 0 ? Math.Round((decimal)purchasesCompleted / checkoutsStarted * 100, 2) : 0,
            OverallConversionRate = productViews > 0 ? Math.Round((decimal)purchasesCompleted / productViews * 100, 2) : 0
        };
    }
}
