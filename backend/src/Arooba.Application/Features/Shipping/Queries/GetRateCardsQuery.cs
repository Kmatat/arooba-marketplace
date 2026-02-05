using Arooba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Shipping.Queries.GetRateCards;

/// <summary>
/// Query to retrieve all active shipping rate cards.
/// </summary>
public record GetRateCardsQuery : IRequest<IReadOnlyList<RateCardDto>>;

/// <summary>
/// DTO representing a shipping rate card between two zones.
/// </summary>
public record RateCardDto
{
    public Guid Id { get; init; }
    public string FromZoneId { get; init; } = string.Empty;
    public string ToZoneId { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public decimal PricePerKg { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Handles retrieval of all shipping rate cards.
/// </summary>
public class GetRateCardsQueryHandler : IRequestHandler<GetRateCardsQuery, IReadOnlyList<RateCardDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRateCardsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RateCardDto>> Handle(
        GetRateCardsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.RateCards
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.FromZoneId)
            .ThenBy(r => r.ToZoneId)
            .Select(r => new RateCardDto
            {
                Id = r.Id,
                FromZoneId = r.FromZoneId,
                ToZoneId = r.ToZoneId,
                BasePrice = r.BasePrice,
                PricePerKg = r.PricePerKg,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
