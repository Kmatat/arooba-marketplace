using Arooba.Application.Features.Dashboard.Queries.GetDashboardStats;
using Arooba.Application.Features.Dashboard.Queries.GetGmvTrend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Provides aggregated dashboard KPIs and analytics data for the Arooba Marketplace
/// admin panel. Powers the executive overview, operational monitoring, and trend analysis.
/// </summary>
[Authorize]
public class DashboardController : ApiControllerBase
{
    /// <summary>
    /// Retrieves aggregate key performance indicators (KPIs) for the marketplace.
    /// </summary>
    /// <remarks>
    /// Returns real-time aggregated statistics including:
    /// - Total GMV (Gross Merchandise Value in EGP)
    /// - Total orders (lifetime count)
    /// - Active vendors (approved and operating)
    /// - Registered customers
    /// - Average order value (EGP)
    /// - COD ratio (percentage of orders using Cash on Delivery)
    /// - Return rate (percentage of delivered orders that were returned)
    /// - Monthly growth rate (GMV month-over-month)
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The dashboard statistics snapshot.</returns>
    /// <response code="200">Dashboard statistics retrieved successfully.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDashboardStatsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a GMV (Gross Merchandise Value) time series for trend analysis.
    /// </summary>
    /// <remarks>
    /// Returns monthly GMV data points for the specified number of trailing months.
    /// Each data point includes the month label and the total GMV value in EGP.
    /// This powers the GMV trend chart on the admin dashboard.
    /// </remarks>
    /// <param name="months">
    /// The number of trailing months to include in the trend. Defaults to 6. Maximum 24.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A time series of monthly GMV values.</returns>
    /// <response code="200">GMV trend data retrieved successfully.</response>
    /// <response code="400">Invalid months parameter (must be between 1 and 24).</response>
    [HttpGet("gmv-trend")]
    [ProducesResponseType(typeof(IReadOnlyList<GmvTrendDataPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGmvTrend(
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetGmvTrendQuery(months), cancellationToken);
        return Ok(result);
    }
}
