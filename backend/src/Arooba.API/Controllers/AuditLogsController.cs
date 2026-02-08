using Arooba.Application.Common.Models;
using Arooba.Application.Features.AuditLogs.Queries;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Provides read-only access to the platform audit trail.
/// Records all significant actions including vendor operations, admin decisions,
/// configuration changes, and status transitions.
/// </summary>
[Authorize]
public class AuditLogsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves audit log entries with filtering by user, action type, entity, and date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? userId,
        [FromQuery] AuditAction? action,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetAuditLogsQuery
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(result);
    }
}
