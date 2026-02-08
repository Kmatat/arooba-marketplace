using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Analytics.Queries;

/// <summary>
/// Query to retrieve per-product analytics including views, add-to-cart, and conversion rates.
/// </summary>
public record GetProductAnalyticsQuery : IRequest<ProductAnalyticsResultDto>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int Top { get; init; } = 20;
    public string SortBy { get; init; } = "views";
}

public record ProductAnalyticsItemDto
{
    public Guid ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public string ProductTitleAr { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
    public int Views { get; init; }
    public int AddedToCart { get; init; }
    public int Purchases { get; init; }
    public decimal ConversionRate { get; init; }
    public int RelatedProductClicks { get; init; }
}

public record ProductAnalyticsResultDto
{
    public List<ProductAnalyticsItemDto> Products { get; init; } = new();
    public List<CategoryAnalyticsDto> CategoryBreakdown { get; init; } = new();
}

public record CategoryAnalyticsDto
{
    public string CategoryId { get; init; } = string.Empty;
    public int Views { get; init; }
    public int CartAdds { get; init; }
    public int Purchases { get; init; }
}

public class GetProductAnalyticsQueryHandler : IRequestHandler<GetProductAnalyticsQuery, ProductAnalyticsResultDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductAnalyticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductAnalyticsResultDto> Handle(
        GetProductAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.UserActivities.AsNoTracking().AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= request.DateTo.Value);

        // Product-level analytics
        var productActions = query.Where(a => a.ProductId.HasValue);

        var productGroups = await productActions
            .GroupBy(a => a.ProductId!.Value)
            .Select(g => new
            {
                ProductId = g.Key,
                Views = g.Count(a => a.Action == UserActivityAction.ProductViewed),
                CartAdds = g.Count(a => a.Action == UserActivityAction.AddedToCart),
                Purchases = g.Count(a => a.Action == UserActivityAction.PurchaseCompleted),
                RelatedClicks = g.Count(a => a.Action == UserActivityAction.RelatedProductClicked)
            })
            .ToListAsync(cancellationToken);

        // Load product details
        var productIds = productGroups.Select(p => p.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Title, p.TitleAr, p.CategoryId })
            .ToListAsync(cancellationToken);

        var productLookup = products.ToDictionary(p => p.Id);

        var productAnalytics = productGroups
            .Select(g =>
            {
                productLookup.TryGetValue(g.ProductId, out var product);
                return new ProductAnalyticsItemDto
                {
                    ProductId = g.ProductId,
                    ProductTitle = product?.Title ?? "Unknown",
                    ProductTitleAr = product?.TitleAr ?? "",
                    CategoryId = product?.CategoryId ?? "",
                    Views = g.Views,
                    AddedToCart = g.CartAdds,
                    Purchases = g.Purchases,
                    ConversionRate = g.Views > 0 ? Math.Round((decimal)g.Purchases / g.Views * 100, 2) : 0,
                    RelatedProductClicks = g.RelatedClicks
                };
            });

        var sorted = request.SortBy.ToLowerInvariant() switch
        {
            "cart" => productAnalytics.OrderByDescending(p => p.AddedToCart),
            "purchases" => productAnalytics.OrderByDescending(p => p.Purchases),
            "conversion" => productAnalytics.OrderByDescending(p => p.ConversionRate),
            _ => productAnalytics.OrderByDescending(p => p.Views)
        };

        // Category breakdown
        var categoryBreakdown = await query
            .Where(a => a.CategoryId != null)
            .GroupBy(a => a.CategoryId!)
            .Select(g => new CategoryAnalyticsDto
            {
                CategoryId = g.Key,
                Views = g.Count(a => a.Action == UserActivityAction.CategoryViewed || a.Action == UserActivityAction.ProductViewed),
                CartAdds = g.Count(a => a.Action == UserActivityAction.AddedToCart),
                Purchases = g.Count(a => a.Action == UserActivityAction.PurchaseCompleted)
            })
            .ToListAsync(cancellationToken);

        return new ProductAnalyticsResultDto
        {
            Products = sorted.Take(request.Top).ToList(),
            CategoryBreakdown = categoryBreakdown
        };
    }
}
