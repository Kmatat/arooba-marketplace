using Arooba.Application.Features.Pricing.Commands.CalculatePrice;
using Arooba.Application.Features.Pricing.Queries.CheckPriceDeviation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Exposes the Arooba pricing engine for calculating product price breakdowns
/// and checking price deviations from category averages.
/// </summary>
[Authorize]
public class PricingController : ApiControllerBase
{
    /// <summary>
    /// Calculates a full price breakdown for a given product configuration.
    /// </summary>
    /// <remarks>
    /// The breakdown includes:
    /// - Cost price (vendor's base cost)
    /// - Sub-vendor uplift (fixed EGP or percentage)
    /// - Cooperative fee (5% for non-legalized vendors, 0% for legalized)
    /// - Marketplace uplift (Arooba's margin)
    /// - VAT (14%)
    /// - Final customer price
    /// </remarks>
    /// <param name="command">The pricing parameters including cost price, vendor type, and uplift configuration.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A detailed price breakdown showing each pricing component.</returns>
    /// <response code="200">Price breakdown calculated successfully.</response>
    /// <response code="400">Invalid pricing parameters provided.</response>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(PriceBreakdownDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculatePrice(
        [FromBody] CalculatePriceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Checks whether a product price deviates significantly from the category average.
    /// Products flagged will require manual review before listing.
    /// </summary>
    /// <param name="query">The price deviation check parameters.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Price deviation analysis with flagging status.</returns>
    /// <response code="200">Deviation check completed.</response>
    /// <response code="400">Invalid parameters provided.</response>
    [HttpPost("check-deviation")]
    [ProducesResponseType(typeof(PriceDeviationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckPriceDeviation(
        [FromBody] CheckPriceDeviationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
