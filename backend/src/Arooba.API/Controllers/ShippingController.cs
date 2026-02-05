using Arooba.Application.Features.Shipping.Queries;
using Arooba.Application.Features.Shipping.Queries.GetRateCards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages shipping zones, rate cards, and delivery fee calculations for the Arooba Marketplace.
/// Egypt is divided into shipping zones, and delivery fees are calculated based on
/// origin-to-destination zone pairs, product weight, and volumetric dimensions.
/// </summary>
[Authorize]
public class ShippingController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all shipping zones defined in the system.
    /// </summary>
    /// <remarks>
    /// Shipping zones represent geographic regions in Egypt (e.g., Greater Cairo, Alexandria,
    /// Delta, Upper Egypt). Each zone covers specific cities and is used for rate card lookups.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of all shipping zones with their covered cities.</returns>
    /// <response code="200">Shipping zones retrieved successfully.</response>
    [HttpGet("zones")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ShippingZoneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShippingZones(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetShippingZonesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all active shipping rate cards between zones.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of active rate cards with zone-to-zone pricing.</returns>
    /// <response code="200">Rate cards retrieved successfully.</response>
    [HttpGet("rates")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<RateCardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRateCards(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetRateCardsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calculates the delivery fee for shipping a product between two zones.
    /// </summary>
    /// <remarks>
    /// The fee is calculated using the rate card for the origin-destination zone pair:
    /// <code>Fee = BasePrice + (ChargeableWeight * PricePerKg)</code>
    /// Where chargeable weight = max(actual weight, volumetric weight).
    /// Volumetric weight = (Length * Width * Height) / 5000.
    /// </remarks>
    /// <param name="query">
    /// The shipping fee calculation parameters including origin zone, destination zone,
    /// actual weight (kg), and optional dimensions (cm) for volumetric weight calculation.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The calculated shipping fee breakdown in EGP.</returns>
    /// <response code="200">Shipping fee calculated successfully.</response>
    /// <response code="400">Invalid zone identifiers or missing required parameters.</response>
    [HttpPost("calculate-fee")]
    [ProducesResponseType(typeof(ShippingFeeResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateShippingFee(
        [FromBody] CalculateShippingFeeQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
