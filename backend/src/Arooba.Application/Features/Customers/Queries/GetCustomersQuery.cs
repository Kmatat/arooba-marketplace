using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve a paginated list of customers with optional search filtering.
/// </summary>
public record GetCustomersQuery : IRequest<PaginatedList<CustomerDto>>
{
    /// <summary>Gets the page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the page size. Defaults to 10.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>Gets an optional search term to filter by name, mobile, or email.</summary>
    public string? SearchTerm { get; init; }
}

/// <summary>
/// DTO representing a customer in list views.
/// </summary>
public record CustomerDto
{
    /// <summary>Gets the customer identifier.</summary>
    public int Id { get; init; }

    /// <summary>Gets the customer's full name.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Gets the mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the email address.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the preferred language.</summary>
    public string PreferredLanguage { get; init; } = default!;

    /// <summary>Gets the number of orders placed.</summary>
    public int OrderCount { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handles the paginated customer list query with search.
/// </summary>
public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetCustomersQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a paginated, filtered list of customers with order counts.
    /// </summary>
    /// <param name="request">The query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paginated list of customer DTOs.</returns>
    public async Task<PaginatedList<CustomerDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(term) ||
                c.MobileNumber.Contains(request.SearchTerm) ||
                (c.Email != null && c.Email.ToLower().Contains(term)));
        }

        var projectedQuery = query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                MobileNumber = c.MobileNumber,
                Email = c.Email,
                PreferredLanguage = c.PreferredLanguage,
                OrderCount = _context.Orders.Count(o => o.CustomerId == c.Id),
                CreatedAt = c.CreatedAt
            });

        return await PaginatedList<CustomerDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
