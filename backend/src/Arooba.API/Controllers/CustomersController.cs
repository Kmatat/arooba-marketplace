using Arooba.Application.Common.Models;
using Arooba.Application.Features.Customers.Commands.AddCustomerAddress;
using Arooba.Application.Features.Customers.Commands.CreateCustomer;
using Arooba.Application.Features.Customers.Commands.UpdateCustomer;
using Arooba.Application.Features.Customers.Queries.GetCustomerById;
using Arooba.Application.Features.Customers.Queries.GetCustomers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages customer profiles, addresses, and account information on the Arooba Marketplace.
/// Customers can maintain multiple delivery addresses and track their order history,
/// loyalty points, and wallet balance.
/// </summary>
[Authorize]
public class CustomersController : ApiControllerBase
{
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
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single customer by identifier with their addresses, order statistics,
    /// loyalty points, and wallet balance.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The customer detail including addresses and aggregated statistics.</returns>
    /// <response code="200">Customer found and returned.</response>
    /// <response code="404">Customer with the specified identifier was not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registers a new customer on the marketplace.
    /// </summary>
    /// <param name="command">
    /// The customer registration details including name, mobile number (+20 format),
    /// optional email, and optional referral code.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created customer.</returns>
    /// <response code="201">Customer registered successfully.</response>
    /// <response code="400">Validation failed (e.g., duplicate mobile number).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
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
    /// <response code="404">Customer with the specified identifier was not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerCommand command,
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
    /// Retrieves a customer's delivery addresses.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of the customer's addresses.</returns>
    /// <response code="200">Addresses retrieved successfully.</response>
    /// <response code="404">Customer with the specified identifier was not found.</response>
    [HttpGet("{id:guid}/addresses")]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddresses(
        Guid id,
        CancellationToken cancellationToken)
    {
        // GetCustomerByIdQuery includes addresses; return the customer with addresses
        var customer = await Sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(customer);
    }

    /// <summary>
    /// Adds a new delivery address to a customer's address book.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="command">
    /// The address details including label (Home, Office), full address, city,
    /// zone identifier, and whether this should be the default address.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created address.</returns>
    /// <response code="201">Address added successfully.</response>
    /// <response code="400">Validation failed or customer identifier mismatch.</response>
    /// <response code="404">Customer with the specified identifier was not found.</response>
    [HttpPost("{id:guid}/addresses")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAddress(
        Guid id,
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
}
