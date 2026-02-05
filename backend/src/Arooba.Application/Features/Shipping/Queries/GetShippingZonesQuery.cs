using Arooba.Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Shipping.Queries;

/// <summary>
/// Query to retrieve all shipping zones (Egyptian governorates and regions).
/// </summary>
public record GetShippingZonesQuery : IRequest<List<ShippingZoneDto>>;

/// <summary>
/// DTO representing a shipping zone (Egyptian governorate or region).
/// </summary>
public record ShippingZoneDto
{
    /// <summary>Gets the zone identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the zone name in English.</summary>
    public string Name { get; init; } = default!;

    /// <summary>Gets the zone name in Arabic.</summary>
    public string NameAr { get; init; } = default!;

    /// <summary>Gets the zone code.</summary>
    public string Code { get; init; } = default!;

    /// <summary>Gets the governorate this zone belongs to.</summary>
    public string Governorate { get; init; } = default!;

    /// <summary>Gets whether this zone is currently served by the logistics network.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the estimated delivery days to this zone from Cairo.</summary>
    public int EstimatedDeliveryDays { get; init; }
}

/// <summary>
/// Handles retrieval of all shipping zones.
/// </summary>
public class GetShippingZonesQueryHandler : IRequestHandler<GetShippingZonesQuery, List<ShippingZoneDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetShippingZonesQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetShippingZonesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all shipping zones ordered by governorate and name.
    /// </summary>
    /// <param name="request">The query (no parameters).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of shipping zone DTOs.</returns>
    public async Task<List<ShippingZoneDto>> Handle(
        GetShippingZonesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ShippingZones
            .AsNoTracking()
            .OrderBy(z => z.Governorate)
            .ThenBy(z => z.Name)
            .Select(z => new ShippingZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                NameAr = z.NameAr,
                Code = z.Code,
                Governorate = z.Governorate,
                IsActive = z.IsActive,
                EstimatedDeliveryDays = z.EstimatedDeliveryDays
            })
            .ToListAsync(cancellationToken);
    }
}
