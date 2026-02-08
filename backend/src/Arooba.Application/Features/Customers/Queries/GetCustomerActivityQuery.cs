using Arooba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve user activity log for a specific customer.
/// Returns activity timeline including views, searches, purchases, etc.
/// </summary>
public record GetCustomerActivityQuery : IRequest<List<CustomerActivityDto>>
{
    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the maximum number of entries to return. Defaults to 50.</summary>
    public int Limit { get; init; } = 50;

    /// <summary>Gets optional action filter.</summary>
    public string? ActionFilter { get; init; }
}

/// <summary>
/// DTO representing a single customer activity entry.
/// </summary>
public record CustomerActivityDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Action { get; init; } = default!;
    public Guid? ProductId { get; init; }
    public string? ProductTitle { get; init; }
    public string? CategoryId { get; init; }
    public Guid? OrderId { get; init; }
    public string? SearchQuery { get; init; }
    public string? Metadata { get; init; }
    public string? SessionId { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? PageUrl { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Handles retrieval of customer activity history from the UserActivities table.
/// </summary>
public class GetCustomerActivityQueryHandler : IRequestHandler<GetCustomerActivityQuery, List<CustomerActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerActivityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerActivityDto>> Handle(
        GetCustomerActivityQuery request,
        CancellationToken cancellationToken)
    {
        // First get the UserId from the customer
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
            return new List<CustomerActivityDto>();

        var query = _context.UserActivities
            .AsNoTracking()
            .Where(a => a.UserId == customer.UserId);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(request.Limit)
            .Select(a => new CustomerActivityDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action.ToString(),
                ProductId = a.ProductId,
                ProductTitle = a.Product != null ? a.Product.Title : null,
                CategoryId = a.CategoryId,
                OrderId = a.OrderId,
                SearchQuery = a.SearchQuery,
                Metadata = a.Metadata,
                SessionId = a.SessionId,
                DeviceType = a.DeviceType,
                IpAddress = a.IpAddress,
                PageUrl = a.PageUrl,
                Timestamp = a.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return activities;
    }
}
