using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using Arooba.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Products.Queries;

/// <summary>
/// Query to retrieve a paginated list of products with filtering by category,
/// vendor, status, price range, and search term.
/// </summary>
public record GetProductsQuery : IRequest<PaginatedList<ProductDto>>
{
    /// <summary>Gets the page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the page size. Defaults to 10.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>Gets an optional category ID filter.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>Gets an optional parent vendor ID filter.</summary>
    public Guid? VendorId { get; init; }

    /// <summary>Gets an optional product status filter.</summary>
    public ProductStatus? Status { get; init; }

    /// <summary>Gets the minimum price filter in EGP.</summary>
    public decimal? MinPrice { get; init; }

    /// <summary>Gets the maximum price filter in EGP.</summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>Gets an optional search term to filter by title.</summary>
    public string? SearchTerm { get; init; }
}

/// <summary>
/// DTO representing a product in list views.
/// </summary>
public record ProductDto
{
    /// <summary>Gets the product identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the product title.</summary>
    public string Title { get; init; } = default!;

    /// <summary>Gets the Arabic product title.</summary>
    public string TitleAr { get; init; } = default!;

    /// <summary>Gets the unique SKU.</summary>
    public string Sku { get; init; } = default!;

    /// <summary>Gets the category identifier.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Gets the parent vendor identifier.</summary>
    public Guid ParentVendorId { get; init; }

    /// <summary>Gets the product status.</summary>
    public ProductStatus Status { get; init; }

    /// <summary>Gets the vendor's selling price in EGP.</summary>
    public decimal SellingPrice { get; init; }

    /// <summary>Gets the final customer-facing price in EGP.</summary>
    public decimal FinalPrice { get; init; }

    /// <summary>Gets the primary image URL.</summary>
    public string? PrimaryImageUrl { get; init; }

    /// <summary>Gets the stock mode.</summary>
    public StockMode StockMode { get; init; }

    /// <summary>Gets the quantity available.</summary>
    public int QuantityAvailable { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO for product category information.
/// </summary>
public record ProductCategoryDto
{
    /// <summary>Gets the category identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the category name.</summary>
    public string Name { get; init; } = default!;

    /// <summary>Gets the Arabic category name.</summary>
    public string NameAr { get; init; } = default!;

    /// <summary>Gets the category commission rate.</summary>
    public decimal CommissionRate { get; init; }
}

/// <summary>
/// Handles the paginated product list query with filtering and search.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetProductsQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paginated, filtered list of products.
    /// </summary>
    /// <param name="request">The query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paginated list of product DTOs.</returns>
    public async Task<PaginatedList<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.VendorId.HasValue)
        {
            query = query.Where(p => p.ParentVendorId == request.VendorId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.FinalPrice >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.FinalPrice <= request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.TitleAr.Contains(request.SearchTerm) ||
                p.Sku.ToLower().Contains(term));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var projectedQuery = query.ProjectTo<ProductDto>(_mapper.ConfigurationProvider);

        return await PaginatedList<ProductDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
