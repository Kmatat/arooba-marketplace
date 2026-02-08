using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Analytics.Queries;

/// <summary>
/// Query to retrieve a paginated list of recent user activity events.
/// </summary>
public record GetUserActivityLogQuery : IRequest<UserActivityLogResultDto>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public Guid? UserId { get; init; }
    public UserActivityAction? ActionFilter { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record UserActivityLogItemDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public UserActivityAction Action { get; init; }
    public Guid? ProductId { get; init; }
    public string? ProductTitle { get; init; }
    public string? CategoryId { get; init; }
    public Guid? OrderId { get; init; }
    public string? SearchQuery { get; init; }
    public string? SessionId { get; init; }
    public string? DeviceType { get; init; }
    public string? PageUrl { get; init; }
    public decimal? CartValue { get; init; }
    public int? CartItemCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UserActivityLogResultDto
{
    public List<UserActivityLogItemDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public class GetUserActivityLogQueryHandler : IRequestHandler<GetUserActivityLogQuery, UserActivityLogResultDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserActivityLogQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserActivityLogResultDto> Handle(
        GetUserActivityLogQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.UserActivities
            .Include(a => a.User)
            .Include(a => a.Product)
            .AsNoTracking()
            .AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= request.DateTo.Value);
        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);
        if (request.ActionFilter.HasValue)
            query = query.Where(a => a.Action == request.ActionFilter.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new UserActivityLogItemDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FullName : "Unknown",
                Action = a.Action,
                ProductId = a.ProductId,
                ProductTitle = a.Product != null ? a.Product.Title : null,
                CategoryId = a.CategoryId,
                OrderId = a.OrderId,
                SearchQuery = a.SearchQuery,
                SessionId = a.SessionId,
                DeviceType = a.DeviceType,
                PageUrl = a.PageUrl,
                CartValue = a.CartValue,
                CartItemCount = a.CartItemCount,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new UserActivityLogResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }
}
