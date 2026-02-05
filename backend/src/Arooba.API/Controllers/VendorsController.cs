using Arooba.Application.Common.Models;
using Arooba.Application.Features.Vendors.Commands.CreateSubVendor;
using SubVendorDto = Arooba.Application.Features.Vendors.Queries.SubVendorDto;
using Arooba.Application.Features.Vendors.Commands.CreateVendor;
using Arooba.Application.Features.Vendors.Commands.UpdateVendor;
using Arooba.Application.Features.Vendors.Queries.GetVendorById;
using Arooba.Application.Features.Vendors.Queries.GetVendors;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages the vendor ecosystem including parent vendors, sub-vendors, and vendor lifecycle.
/// Supports both legalized vendors (with commercial registration) and non-legalized vendors
/// (artisans, home businesses) operating through a cooperative umbrella.
/// </summary>
[Authorize]
public class VendorsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of vendors with optional filtering by status, type, and search term.
    /// </summary>
    /// <param name="status">Filter by vendor approval status (Pending, Active, Suspended, Rejected).</param>
    /// <param name="type">Filter by vendor type (Legalized, NonLegalized).</param>
    /// <param name="search">Free-text search across business name and contact information.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated list of vendor summaries.</returns>
    /// <response code="200">Vendor list retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendors(
        [FromQuery] VendorStatus? status,
        [FromQuery] VendorType? type,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVendorsQuery
        {
            Status = status,
            Type = type,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single vendor by identifier, including their sub-vendor hierarchy and metrics.
    /// </summary>
    /// <param name="id">The unique identifier of the vendor.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The vendor detail including sub-vendors, financial metrics, and status history.</returns>
    /// <response code="200">Vendor found and returned.</response>
    /// <response code="404">Vendor with the specified identifier was not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VendorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVendor(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVendorByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registers a new parent vendor on the marketplace.
    /// The vendor will be placed in <c>Pending</c> status awaiting admin approval.
    /// </summary>
    /// <param name="command">
    /// The vendor registration details including business name, type (legalized/non-legalized),
    /// legal documents (commercial reg, tax ID), bank details, and cooperative assignment.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created vendor.</returns>
    /// <response code="201">Vendor created successfully and pending review.</response>
    /// <response code="400">Validation failed (e.g., missing required legal documents for legalized vendors).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVendor(
        [FromBody] CreateVendorCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVendor), new { id }, id);
    }

    /// <summary>
    /// Updates an existing vendor's profile information.
    /// </summary>
    /// <param name="id">The unique identifier of the vendor to update.</param>
    /// <param name="command">The updated vendor details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Vendor updated successfully.</response>
    /// <response code="400">Validation failed or identifier mismatch.</response>
    /// <response code="404">Vendor with the specified identifier was not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVendor(
        Guid id,
        [FromBody] UpdateVendorCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
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

    /// <summary>
    /// Retrieves the list of sub-vendors under a parent vendor.
    /// </summary>
    /// <param name="id">The unique identifier of the parent vendor.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of sub-vendors belonging to the parent vendor.</returns>
    /// <response code="200">Sub-vendors retrieved successfully.</response>
    /// <response code="404">Parent vendor with the specified identifier was not found.</response>
    [HttpGet("{id:guid}/sub-vendors")]
    [ProducesResponseType(typeof(IReadOnlyList<SubVendorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubVendors(
        Guid id,
        CancellationToken cancellationToken)
    {
        var vendor = await Sender.Send(new GetVendorByIdQuery(id), cancellationToken);
        // The vendor detail DTO includes sub-vendors; extract them
        return Ok(vendor);
    }

    /// <summary>
    /// Creates a sub-vendor under the specified parent vendor.
    /// Sub-vendors represent individual sellers (e.g., "Aunt Nadia") managed by the parent.
    /// </summary>
    /// <param name="id">The unique identifier of the parent vendor.</param>
    /// <param name="command">
    /// The sub-vendor details including internal name, default lead time, and uplift configuration.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created sub-vendor.</returns>
    /// <response code="201">Sub-vendor created successfully.</response>
    /// <response code="400">Validation failed or parent vendor identifier mismatch.</response>
    /// <response code="404">Parent vendor with the specified identifier was not found.</response>
    [HttpPost("{id:guid}/sub-vendors")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSubVendor(
        Guid id,
        [FromBody] CreateSubVendorCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ParentVendorId)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Identifier Mismatch",
                Detail = "The route parent vendor identifier does not match the command.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var subVendorId = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVendor), new { id }, subVendorId);
    }
}
