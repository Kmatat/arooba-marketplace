using Arooba.Application.Features.Categories.Queries.GetCategories;
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
    /// <returns>A flat list of all categories with their hierarchy information.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }
}
