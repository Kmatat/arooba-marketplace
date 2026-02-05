using Arooba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Query to retrieve all product categories with uplift configuration.
/// </summary>
public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

/// <summary>
/// DTO representing a product category with pricing configuration.
/// </summary>
public record CategoryDto
{
    public string Id { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public decimal MinUpliftRate { get; init; }
    public decimal MaxUpliftRate { get; init; }
    public decimal DefaultUpliftRate { get; init; }
    public string Risk { get; init; } = string.Empty;
    public int ProductCount { get; init; }
}

/// <summary>
/// Handles retrieval of all product categories.
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Icon = c.Icon,
                MinUpliftRate = c.MinUpliftRate,
                MaxUpliftRate = c.MaxUpliftRate,
                DefaultUpliftRate = c.DefaultUpliftRate,
                Risk = c.Risk,
                ProductCount = c.Products != null ? c.Products.Count : 0
            })
            .ToListAsync(cancellationToken);
    }
}
