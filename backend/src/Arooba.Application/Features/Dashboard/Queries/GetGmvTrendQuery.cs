using Arooba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Dashboard.Queries;

/// <summary>
/// Query to retrieve time series GMV data for dashboard charts.
/// </summary>
public record GetGmvTrendQuery : IRequest<List<GmvTrendDataPoint>>
{
    /// <summary>Gets the start date of the trend period.</summary>
    public DateTime DateFrom { get; init; }

    /// <summary>Gets the end date of the trend period.</summary>
    public DateTime DateTo { get; init; }

    /// <summary>Gets the grouping granularity (Day, Week, Month). Defaults to Day.</summary>
    public TrendGranularity Granularity { get; init; } = TrendGranularity.Day;
}

/// <summary>
/// Specifies the time grouping granularity for trend data.
/// </summary>
public enum TrendGranularity
{
    /// <summary>Group by day.</summary>
    Day,

    /// <summary>Group by week.</summary>
    Week,

    /// <summary>Group by month.</summary>
    Month
}

/// <summary>
/// A single data point in the GMV trend time series.
/// </summary>
public record GmvTrendDataPoint
{
    /// <summary>Gets the date for this data point.</summary>
    public DateTime Date { get; init; }

    /// <summary>Gets the total GMV for this period in EGP.</summary>
    public decimal Gmv { get; init; }

    /// <summary>Gets the total number of orders for this period.</summary>
    public int OrderCount { get; init; }

    /// <summary>Gets the average order value for this period in EGP.</summary>
    public decimal AverageOrderValue { get; init; }

    /// <summary>Gets the total commission earned for this period in EGP.</summary>
    public decimal Commission { get; init; }
}

/// <summary>
/// Handles computation of time series GMV data grouped by the specified granularity.
/// </summary>
public class GetGmvTrendQueryHandler : IRequestHandler<GetGmvTrendQuery, List<GmvTrendDataPoint>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetGmvTrendQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetGmvTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Queries orders within the specified date range and groups them by the
    /// requested granularity to produce a time series of GMV data points.
    /// </summary>
    /// <param name="request">The query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of GMV trend data points.</returns>
    public async Task<List<GmvTrendDataPoint>> Handle(
        GetGmvTrendQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CreatedAt >= request.DateFrom && o.CreatedAt <= request.DateTo)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var grouped = request.Granularity switch
        {
            TrendGranularity.Day => orders.GroupBy(o => o.CreatedAt.Date),
            TrendGranularity.Week => orders.GroupBy(o => StartOfWeek(o.CreatedAt)),
            TrendGranularity.Month => orders.GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1)),
            _ => orders.GroupBy(o => o.CreatedAt.Date)
        };

        var result = grouped
            .Select(g =>
            {
                var gmv = g.Sum(o => o.TotalAmount);
                var orderCount = g.Count();
                var commission = g.SelectMany(o => o.Items ?? Enumerable.Empty<Domain.Entities.OrderItem>())
                    .Sum(oi => oi.CommissionAmount);

                return new GmvTrendDataPoint
                {
                    Date = g.Key,
                    Gmv = gmv,
                    OrderCount = orderCount,
                    AverageOrderValue = orderCount > 0 ? Math.Round(gmv / orderCount, 2) : 0m,
                    Commission = commission
                };
            })
            .OrderBy(dp => dp.Date)
            .ToList();

        return result;
    }

    /// <summary>
    /// Calculates the start of the ISO week (Monday) for a given date.
    /// </summary>
    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
