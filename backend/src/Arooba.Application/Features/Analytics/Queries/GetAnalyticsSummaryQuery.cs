using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Analytics.Queries;

/// <summary>
/// Query to retrieve top-level analytics summary KPIs for the user analytics dashboard.
/// </summary>
public record GetAnalyticsSummaryQuery : IRequest<AnalyticsSummaryDto>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public record AnalyticsSummaryDto
{
    public int TotalSessions { get; init; }
    public int UniqueUsers { get; init; }
    public int TotalProductViews { get; init; }
    public int TotalCartAdds { get; init; }
    public int TotalPurchases { get; init; }
    public decimal OverallConversionRate { get; init; }
    public int TotalSearches { get; init; }
    public decimal AverageCartValue { get; init; }
    public List<ActivityTrendPointDto> DailyTrend { get; init; } = new();
    public List<DeviceBreakdownDto> DeviceBreakdown { get; init; } = new();
    public List<TopSearchDto> TopSearches { get; init; } = new();
}

public record ActivityTrendPointDto
{
    public DateTime Date { get; init; }
    public int Views { get; init; }
    public int CartAdds { get; init; }
    public int Purchases { get; init; }
    public int Sessions { get; init; }
}

public record DeviceBreakdownDto
{
    public string DeviceType { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record TopSearchDto
{
    public string Query { get; init; } = string.Empty;
    public int Count { get; init; }
}

public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetAnalyticsSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsSummaryDto> Handle(
        GetAnalyticsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.UserActivities.AsNoTracking().AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= request.DateTo.Value);

        var totalSessions = await query
            .Where(a => a.SessionId != null)
            .Select(a => a.SessionId)
            .Distinct()
            .CountAsync(cancellationToken);

        var uniqueUsers = await query
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalViews = await query.CountAsync(a => a.Action == UserActivityAction.ProductViewed, cancellationToken);
        var totalCartAdds = await query.CountAsync(a => a.Action == UserActivityAction.AddedToCart, cancellationToken);
        var totalPurchases = await query.CountAsync(a => a.Action == UserActivityAction.PurchaseCompleted, cancellationToken);
        var totalSearches = await query.CountAsync(a => a.Action == UserActivityAction.ProductSearched, cancellationToken);

        var avgCartValue = await query
            .Where(a => a.Action == UserActivityAction.CheckoutStarted && a.CartValue.HasValue)
            .Select(a => a.CartValue!.Value)
            .DefaultIfEmpty(0)
            .AverageAsync(cancellationToken);

        // Daily trend
        var dailyTrend = await query
            .GroupBy(a => a.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ActivityTrendPointDto
            {
                Date = g.Key,
                Views = g.Count(a => a.Action == UserActivityAction.ProductViewed),
                CartAdds = g.Count(a => a.Action == UserActivityAction.AddedToCart),
                Purchases = g.Count(a => a.Action == UserActivityAction.PurchaseCompleted),
                Sessions = g.Where(a => a.SessionId != null).Select(a => a.SessionId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        // Device breakdown
        var devices = await query
            .Where(a => a.DeviceType != null)
            .GroupBy(a => a.DeviceType!)
            .Select(g => new { DeviceType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalDevices = devices.Sum(d => d.Count);
        var deviceBreakdown = devices.Select(d => new DeviceBreakdownDto
        {
            DeviceType = d.DeviceType,
            Count = d.Count,
            Percentage = totalDevices > 0 ? Math.Round((decimal)d.Count / totalDevices * 100, 1) : 0
        }).ToList();

        // Top searches
        var topSearches = await query
            .Where(a => a.Action == UserActivityAction.ProductSearched && a.SearchQuery != null)
            .GroupBy(a => a.SearchQuery!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new TopSearchDto
            {
                Query = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return new AnalyticsSummaryDto
        {
            TotalSessions = totalSessions,
            UniqueUsers = uniqueUsers,
            TotalProductViews = totalViews,
            TotalCartAdds = totalCartAdds,
            TotalPurchases = totalPurchases,
            OverallConversionRate = totalViews > 0 ? Math.Round((decimal)totalPurchases / totalViews * 100, 2) : 0,
            TotalSearches = totalSearches,
            AverageCartValue = Math.Round(avgCartValue, 2),
            DailyTrend = dailyTrend,
            DeviceBreakdown = deviceBreakdown,
            TopSearches = topSearches
        };
    }
}
