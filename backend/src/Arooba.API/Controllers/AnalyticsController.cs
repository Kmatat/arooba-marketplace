using Arooba.Application.Features.Analytics.Commands;
using Arooba.Application.Features.Analytics.Queries;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Provides user analytics, conversion funnel data, product performance metrics,
/// and activity logs for the management analytics dashboard.
/// </summary>
[Authorize]
public class AnalyticsController : ApiControllerBase
{
    /// <summary>
    /// Records a user activity event.
    /// </summary>
    [HttpPost("track")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> TrackActivity(
        [FromBody] TrackUserActivityCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(id);
    }

    /// <summary>
    /// Retrieves the analytics summary KPIs (sessions, users, views, conversions, searches).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAnalyticsSummaryQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves conversion funnel data (views -> cart -> checkout -> purchase).
    /// </summary>
    [HttpGet("conversion-funnel")]
    [ProducesResponseType(typeof(ConversionFunnelDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversionFunnel(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetConversionFunnelQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves per-product analytics (views, cart adds, purchases, conversion rates).
    /// </summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(ProductAnalyticsResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductAnalytics(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int top = 20,
        [FromQuery] string sortBy = "views",
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetProductAnalyticsQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            Top = top,
            SortBy = sortBy
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated log of all user activity events.
    /// </summary>
    [HttpGet("activity-log")]
    [ProducesResponseType(typeof(UserActivityLogResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityLog(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] Guid? userId,
        [FromQuery] UserActivityAction? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetUserActivityLogQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            UserId = userId,
            ActionFilter = action,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(result);
    }
}
