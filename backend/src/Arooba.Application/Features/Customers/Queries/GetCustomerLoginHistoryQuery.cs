using Arooba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve login history for a specific customer.
/// </summary>
public record GetCustomerLoginHistoryQuery : IRequest<List<CustomerLoginDto>>
{
    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the maximum number of entries to return. Defaults to 50.</summary>
    public int Limit { get; init; } = 50;
}

/// <summary>
/// DTO representing a customer login history entry.
/// </summary>
public record CustomerLoginDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime Timestamp { get; init; }
    public string Status { get; init; } = default!;
    public string IpAddress { get; init; } = default!;
    public string DeviceType { get; init; } = default!;
    public string DeviceInfo { get; init; } = default!;
    public string? Location { get; init; }
    public int? SessionDurationMinutes { get; init; }
}

/// <summary>
/// Handles retrieval of customer login history, most recent first.
/// </summary>
public class GetCustomerLoginHistoryQueryHandler : IRequestHandler<GetCustomerLoginHistoryQuery, List<CustomerLoginDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerLoginHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerLoginDto>> Handle(
        GetCustomerLoginHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.CustomerLoginHistory
            .AsNoTracking()
            .Where(l => l.CustomerId == request.CustomerId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(request.Limit)
            .Select(l => new CustomerLoginDto
            {
                Id = l.Id,
                CustomerId = l.CustomerId,
                Timestamp = l.CreatedAt,
                Status = l.Status.ToString(),
                IpAddress = l.IpAddress,
                DeviceType = l.DeviceType,
                DeviceInfo = l.DeviceInfo,
                Location = l.Location,
                SessionDurationMinutes = l.SessionDurationMinutes,
            })
            .ToListAsync(cancellationToken);
    }
}
