using Arooba.Application.Common.Models;
using Arooba.Application.Features.Approvals.Commands;
using Arooba.Application.Features.Approvals.Queries;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages the vendor action approval workflow.
/// Vendors submit action requests; admins review, approve, or reject them.
/// All decisions are audit-logged for compliance.
/// </summary>
[Authorize]
public class ApprovalsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves vendor action requests with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ActionRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequests(
        [FromQuery] ApprovalStatus? status,
        [FromQuery] VendorActionType? actionType,
        [FromQuery] Guid? vendorId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetActionRequestsQuery
        {
            Status = status,
            ActionType = actionType,
            VendorId = vendorId,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new vendor action request requiring admin approval.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequest(
        [FromBody] CreateActionRequestCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRequests), new { id }, id);
    }

    /// <summary>
    /// Reviews (approves or rejects) a pending vendor action request.
    /// Rejection requires review notes.
    /// </summary>
    [HttpPut("{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewRequest(
        Guid id,
        [FromBody] ReviewActionRequestCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.RequestId)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Identifier Mismatch",
                Detail = "The route identifier does not match the command identifier.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await Sender.Send(command, cancellationToken);
        return NoContent();
    }
}
