using Arooba.Application.Common.Models;
using Arooba.Application.Features.Customers.Commands;
using Arooba.Application.Features.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Complete Customer CRM controller for the Arooba Marketplace.
/// Manages customer profiles, addresses, orders, reviews, performance metrics,
/// login history, activity logs, and audit trails.
/// </summary>
[Authorize]
public class CustomersController : ApiControllerBase
{
    // ──────────────────────────────────────────────
    // CUSTOMER LIST & PROFILE
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves a paginated list of customers with optional search filtering.
    /// </summary>
    /// <param name="search">Free-text search across customer name, email, and mobile number.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated list of customer summaries.</returns>
    /// <response code="200">Customer list retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomersQuery
        {
            SearchTerm = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single customer by identifier with addresses, order statistics,
    /// loyalty points, and wallet balance.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The customer detail including addresses and aggregated statistics.</returns>
    /// <response code="200">Customer found and returned.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerByIdQuery { CustomerId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registers a new customer on the marketplace.
    /// </summary>
    /// <param name="command">The customer registration details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created customer.</returns>
    /// <response code="201">Customer registered successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id }, id);
    }

    /// <summary>
    /// Updates an existing customer's profile information.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="command">The updated customer details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Customer profile updated successfully.</response>
    /// <response code="400">Validation failed or identifier mismatch.</response>
    /// <response code="404">Customer not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(
        int id,
        [FromBody] UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.CustomerId)
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
    /// Updates a customer's account status (activate, deactivate, block, unblock).
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="command">The status update details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Customer status updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Customer not found.</response>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerStatus(
        int id,
        [FromBody] UpdateCustomerStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.CustomerId)
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

    // ──────────────────────────────────────────────
    // ADDRESSES
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves a customer's delivery addresses.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The customer detail with addresses.</returns>
    /// <response code="200">Addresses retrieved successfully.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{id:int}/addresses")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddresses(
        int id,
        CancellationToken cancellationToken)
    {
        var customer = await Sender.Send(new GetCustomerByIdQuery { CustomerId = id }, cancellationToken);
        return Ok(customer);
    }

    /// <summary>
    /// Adds a new delivery address to a customer's address book.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="command">The address details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created address.</returns>
    /// <response code="201">Address added successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Customer not found.</response>
    [HttpPost("{id:int}/addresses")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAddress(
        int id,
        [FromBody] AddCustomerAddressCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.CustomerId)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Identifier Mismatch",
                Detail = "The route customer identifier does not match the command.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var addressId = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id }, addressId);
    }

    // ──────────────────────────────────────────────
    // REVIEWS & RATINGS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves all reviews submitted by a specific customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of customer reviews with product and order context.</returns>
    /// <response code="200">Reviews retrieved successfully.</response>
    [HttpGet("{id:int}/reviews")]
    [ProducesResponseType(typeof(List<CustomerReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerReviews(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerReviewsQuery { CustomerId = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submits a product review from a customer for a specific order.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="command">The review details including rating and optional text.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created review.</returns>
    /// <response code="201">Review submitted successfully.</response>
    /// <response code="400">Validation failed or duplicate review.</response>
    /// <response code="404">Customer or order not found.</response>
    [HttpPost("{id:int}/reviews")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitReview(
        int id,
        [FromBody] SubmitReviewCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.CustomerId)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Identifier Mismatch",
                Detail = "The route customer identifier does not match the command.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var reviewId = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomerReviews), new { id }, reviewId);
    }

    // ──────────────────────────────────────────────
    // PERFORMANCE & LOYALTY
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves performance metrics for a customer including RFM scores,
    /// loyalty tier progress, spending trends, and referral statistics.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Customer performance and loyalty metrics.</returns>
    /// <response code="200">Performance data retrieved successfully.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{id:int}/performance")]
    [ProducesResponseType(typeof(CustomerPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerPerformance(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerPerformanceQuery { CustomerId = id }, cancellationToken);
        return Ok(result);
    }

    // ──────────────────────────────────────────────
    // LOGIN HISTORY
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves login history for a customer with device, IP, and session data.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="limit">Maximum number of entries to return. Defaults to 50.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of login history entries.</returns>
    /// <response code="200">Login history retrieved successfully.</response>
    [HttpGet("{id:int}/logins")]
    [ProducesResponseType(typeof(List<CustomerLoginDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginHistory(
        int id,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetCustomerLoginHistoryQuery { CustomerId = id, Limit = limit },
            cancellationToken);
        return Ok(result);
    }

    // ──────────────────────────────────────────────
    // ACTIVITY LOG
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves user activity timeline for a customer including product views,
    /// searches, cart actions, purchases, and more.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="limit">Maximum number of entries to return. Defaults to 50.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of activity entries.</returns>
    /// <response code="200">Activity log retrieved successfully.</response>
    [HttpGet("{id:int}/activity")]
    [ProducesResponseType(typeof(List<CustomerActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerActivity(
        int id,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetCustomerActivityQuery { CustomerId = id, Limit = limit },
            cancellationToken);
        return Ok(result);
    }

    // ──────────────────────────────────────────────
    // AUDIT LOGS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Retrieves audit log entries related to a specific customer including
    /// account changes, tier upgrades, wallet transactions, and admin actions.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="limit">Maximum number of entries to return. Defaults to 50.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of audit log entries.</returns>
    /// <response code="200">Audit logs retrieved successfully.</response>
    [HttpGet("{id:int}/audit-logs")]
    [ProducesResponseType(typeof(List<CustomerAuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAuditLogs(
        int id,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetCustomerAuditLogsQuery { CustomerId = id, Limit = limit },
            cancellationToken);
        return Ok(result);
    }
}
