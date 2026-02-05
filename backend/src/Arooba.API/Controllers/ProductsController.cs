using Arooba.Application.Common.Models;
using Arooba.Application.Features.Products.Commands.ChangeProductStatus;
using Arooba.Application.Features.Products.Commands.CreateProduct;
using Arooba.Application.Features.Products.Commands.UpdateProduct;
using Arooba.Application.Features.Products.Queries.GetProductById;
using Arooba.Application.Features.Products.Queries.GetProducts;
using Arooba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages the product catalog (PIM) for the Arooba Marketplace.
/// Products follow the pricing engine pipeline: cost price, sub-vendor uplift,
/// cooperative fee, marketplace uplift, and VAT to arrive at the final customer price.
/// </summary>
[Authorize]
public class ProductsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a paginated, filterable list of products in the marketplace catalog.
    /// </summary>
    /// <param name="category">Filter by category identifier.</param>
    /// <param name="vendor">Filter by vendor (parent) identifier.</param>
    /// <param name="status">Filter by product status (Draft, PendingReview, Active, Paused, Rejected).</param>
    /// <param name="minPrice">Minimum final price filter (EGP).</param>
    /// <param name="maxPrice">Maximum final price filter (EGP).</param>
    /// <param name="search">Free-text search across product title, description, and SKU.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated list of product summaries with pricing information.</returns>
    /// <response code="200">Product list retrieved successfully.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] Guid? category,
        [FromQuery] Guid? vendor,
        [FromQuery] ProductStatus? status,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery
        {
            CategoryId = category,
            VendorId = vendor,
            Status = status,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single product by identifier with its full pricing breakdown.
    /// </summary>
    /// <remarks>
    /// The response includes the 5-bucket pricing breakdown:
    /// cost price, sub-vendor uplift, cooperative fee, marketplace uplift, and VAT.
    /// </remarks>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The product detail including pricing breakdown, vendor info, and logistics data.</returns>
    /// <response code="200">Product found and returned.</response>
    /// <response code="404">Product with the specified identifier was not found.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new product listing in the marketplace catalog.
    /// The product will be placed in <c>Draft</c> or <c>PendingReview</c> status depending on vendor settings.
    /// </summary>
    /// <param name="command">
    /// The product details including title, description, pricing, category, pickup location,
    /// stock mode, weight/dimensions, and image URLs.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the newly created product.</returns>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">Validation failed (e.g., missing required fields or invalid pricing).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id }, id);
    }

    /// <summary>
    /// Updates an existing product's details. Re-triggers the pricing engine to recalculate
    /// the final price if cost price or uplift parameters have changed.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="command">The updated product details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Product updated successfully.</response>
    /// <response code="400">Validation failed or identifier mismatch.</response>
    /// <response code="404">Product with the specified identifier was not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command,
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
    /// Changes the status of an existing product (e.g., submit for review, approve, pause, reject).
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="command">The new status and optional reason for the transition.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Product status updated successfully.</response>
    /// <response code="400">Invalid status transition or validation error.</response>
    /// <response code="404">Product with the specified identifier was not found.</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeProductStatus(
        Guid id,
        [FromBody] ChangeProductStatusCommand command,
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
