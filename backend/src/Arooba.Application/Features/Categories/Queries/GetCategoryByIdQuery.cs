using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Categories.Queries.GetCategoryById;

/// <summary>
/// Query to retrieve a single product category by its identifier.
/// </summary>
public record GetCategoryByIdQuery(string Id) : IRequest<CategoryDetailDto>;

/// <summary>
/// DTO for a single category with full detail.
/// </summary>
public record CategoryDetailDto
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
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of a single product category by ID.
/// </summary>
public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDetailDto> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CategoryDetailDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Icon = c.Icon,
                MinUpliftRate = c.MinUpliftRate,
                MaxUpliftRate = c.MaxUpliftRate,
                DefaultUpliftRate = c.DefaultUpliftRate,
                Risk = c.Risk,
                ProductCount = c.Products != null ? c.Products.Count : 0,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(nameof(ProductCategory), request.Id);
        }

        return category;
    }
}
