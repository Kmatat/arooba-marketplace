using Arooba.Application.Features.Categories.Queries.GetCategories;
using Arooba.Application.Features.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Provides read access to the product category taxonomy used across the marketplace.
/// Categories drive catalog browsing, product classification, and analytics segmentation.
/// </summary>
public class CategoriesController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all product categories available on the marketplace.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A flat list of all categories with their uplift configuration.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single product category by its identifier.
    /// </summary>
    /// <param name="id">The category identifier (e.g., "jewelry-accessories").</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The category details including uplift configuration.</returns>
    /// <response code="200">Category found and returned.</response>
    /// <response code="404">Category with the specified identifier was not found.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return Ok(result);
    }
}
