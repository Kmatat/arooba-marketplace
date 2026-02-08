using Arooba.Application.Features.AdminConfig.Commands;
using Arooba.Application.Features.AdminConfig.Queries;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages dynamic platform configurations. Only accessible by admin roles.
/// Allows runtime modification of pricing rules, SLA thresholds, and business parameters
/// without code deployment.
/// </summary>
[Authorize]
public class AdminConfigController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all platform configurations, optionally filtered by category.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigs(
        [FromQuery] ConfigCategory? category,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetConfigsQuery { Category = category }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates a platform configuration entry.
    /// Changes are audit-logged automatically.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertConfig(
        [FromBody] UpsertConfigCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(id);
    }
}
