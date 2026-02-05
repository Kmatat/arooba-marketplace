using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using Arooba.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Vendors.Queries;

/// <summary>
/// Query to retrieve a paginated list of parent vendors with optional filtering
/// by status, vendor type, and search term.
/// </summary>
public record GetVendorsQuery : IRequest<PaginatedList<VendorDto>>
{
    /// <summary>Gets the page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the page size. Defaults to 10.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>Gets an optional status filter.</summary>
    public VendorStatus? Status { get; init; }

    /// <summary>Gets an optional vendor type filter.</summary>
    public VendorType? VendorType { get; init; }

    /// <summary>Gets an optional search term to filter by business name or email.</summary>
    public string? SearchTerm { get; init; }
}

/// <summary>
/// DTO representing a vendor in list views.
/// </summary>
public record VendorDto
{
    /// <summary>Gets the vendor identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the business name.</summary>
    public string BusinessName { get; init; } = default!;

    /// <summary>Gets the Arabic business name.</summary>
    public string BusinessNameAr { get; init; } = default!;

    /// <summary>Gets the vendor type.</summary>
    public VendorType VendorType { get; init; }

    /// <summary>Gets the vendor status.</summary>
    public VendorStatus Status { get; init; }

    /// <summary>Gets the mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the email address.</summary>
    public string Email { get; init; } = default!;

    /// <summary>Gets whether the vendor is VAT registered.</summary>
    public bool IsVatRegistered { get; init; }

    /// <summary>Gets the commission rate.</summary>
    public decimal CommissionRate { get; init; }

    /// <summary>Gets the count of sub-vendors under this parent.</summary>
    public int SubVendorCount { get; init; }

    /// <summary>Gets the date the vendor was created.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handles the paginated vendor list query with filtering and search.
/// </summary>
public class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, PaginatedList<VendorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetVendorsQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetVendorsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paginated, filtered list of vendors.
    /// </summary>
    /// <param name="request">The query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paginated list of vendor DTOs.</returns>
    public async Task<PaginatedList<VendorDto>> Handle(
        GetVendorsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ParentVendors
            .Include(v => v.SubVendors)
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(v => v.Status == request.Status.Value);
        }

        if (request.VendorType.HasValue)
        {
            query = query.Where(v => v.VendorType == request.VendorType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(v =>
                v.BusinessName.ToLower().Contains(term) ||
                v.BusinessNameAr.Contains(request.SearchTerm) ||
                v.Email.ToLower().Contains(term) ||
                v.MobileNumber.Contains(request.SearchTerm));
        }

        query = query.OrderByDescending(v => v.CreatedAt);

        var projectedQuery = query.ProjectTo<VendorDto>(_mapper.ConfigurationProvider);

        return await PaginatedList<VendorDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
