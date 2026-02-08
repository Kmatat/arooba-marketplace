using Arooba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve audit log entries related to a specific customer.
/// </summary>
public record GetCustomerAuditLogsQuery : IRequest<List<CustomerAuditLogDto>>
{
    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the maximum number of entries to return. Defaults to 50.</summary>
    public int Limit { get; init; } = 50;
}

/// <summary>
/// DTO representing a customer audit log entry.
/// </summary>
public record CustomerAuditLogDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string UserRole { get; init; } = default!;
    public string Action { get; init; } = default!;
    public string EntityType { get; init; } = default!;
    public string? EntityId { get; init; }
    public string Description { get; init; } = default!;
    public string? DescriptionAr { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of audit logs for a specific customer entity.
/// </summary>
public class GetCustomerAuditLogsQueryHandler : IRequestHandler<GetCustomerAuditLogsQuery, List<CustomerAuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerAuditLogDto>> Handle(
        GetCustomerAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var customerIdStr = request.CustomerId.ToString();

        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a =>
                (a.EntityType == "Customer" && a.EntityId == customerIdStr) ||
                a.UserId == customerIdStr)
            .OrderByDescending(a => a.CreatedAt)
            .Take(request.Limit)
            .Select(a => new CustomerAuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName,
                UserRole = a.UserRole,
                Action = a.Action.ToString(),
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Description = a.Description,
                DescriptionAr = a.DescriptionAr,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
