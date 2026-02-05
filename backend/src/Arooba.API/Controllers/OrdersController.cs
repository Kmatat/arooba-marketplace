using Arooba.Application.Common.Models;
using Arooba.Application.Features.Orders.Commands.CreateOrder;
using Arooba.Application.Features.Orders.Commands.UpdateOrderStatus;
using Arooba.Application.Features.Orders.Queries.GetOrderById;
using Arooba.Application.Features.Orders.Queries.GetOrders;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages the order lifecycle on the Arooba Marketplace (OMS).
/// Handles order creation (including multi-vendor cart splitting into shipments),
/// status transitions, and order retrieval with full financial breakdown.
/// </summary>
[Authorize]
public class OrdersController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of orders with optional filtering.
    /// </summary>
    /// <param name="status">Filter by order status (Pending, Accepted, ReadyToShip, InTransit, Delivered, Returned, Cancelled).</param>
    /// <param name="customerId">Filter by the customer who placed the order.</param>
    /// <param name="vendorId">Filter by vendor involved in the order.</param>
    /// <param name="from">Filter orders created on or after this date (UTC).</param>
    /// <param name="to">Filter orders created on or before this date (UTC).</param>
    /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated list of order summaries.</returns>
    /// <response code="200">Order list retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? vendorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersQuery
        {
            Status = status,
            CustomerId = customerId,
            VendorId = vendorId,
            From = from,
            To = to,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single order by identifier with complete details including
    /// line items, shipments, and the 5-bucket financial split per item.
    /// </summary>
    /// <remarks>
    /// The 5-bucket split for each order item:
    /// <list type="bullet">
    ///   <item>Bucket A: Vendor revenue (cost price)</item>
    ///   <item>Bucket B: Vendor VAT (14% on vendor revenue)</item>
    ///   <item>Bucket C: Arooba revenue (marketplace uplift + cooperative fee)</item>
    ///   <item>Bucket D: Arooba VAT (14% on Arooba revenue)</item>
    ///   <item>Bucket E: Logistics fee (shipping/delivery)</item>
    /// </list>
    /// </remarks>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The order detail including items, shipments, and financial breakdown.</returns>
    /// <response code="200">Order found and returned.</response>
    /// <response code="404">Order with the specified identifier was not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order on the marketplace.
    /// </summary>
    /// <remarks>
    /// This is the most complex operation in the system. It:
    /// <list type="number">
    ///   <item>Validates product availability and pricing</item>
    ///   <item>Runs the pricing engine to compute the 5-bucket split for each item</item>
    ///   <item>Groups items by pickup location to create separate shipments</item>
    ///   <item>Calculates delivery fees per shipment based on zone-to-zone rate cards</item>
    ///   <item>Creates escrow holds on vendor wallets (14-day escrow period)</item>
    ///   <item>Records ledger entries for all financial movements</item>
    /// </list>
    /// </remarks>
    /// <param name="command">
    /// The order details including customer, delivery address, line items, and payment method.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created order.</returns>
    /// <response code="201">Order created successfully.</response>
    /// <response code="400">Validation failed (e.g., out-of-stock items, invalid address zone).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id }, id);
    }

    /// <summary>
    /// Updates the status of an existing order (e.g., accept, mark ready to ship, deliver, cancel).
    /// </summary>
    /// <remarks>
    /// Status transitions are validated against the order state machine:
    /// Pending -> Accepted -> ReadyToShip -> InTransit -> Delivered
    /// Pending -> Cancelled (before acceptance)
    /// InTransit -> Returned (after delivery attempt)
    /// Any -> RejectedShipping (logistics rejection)
    /// </remarks>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="command">The new status and optional notes/reason.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Order status updated successfully.</response>
    /// <response code="400">Invalid status transition.</response>
    /// <response code="404">Order with the specified identifier was not found.</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusCommand command,
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
}
