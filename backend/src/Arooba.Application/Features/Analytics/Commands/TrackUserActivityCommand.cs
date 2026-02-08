using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.Analytics.Commands;

/// <summary>
/// Command to record a user activity event for analytics tracking.
/// </summary>
public record TrackUserActivityCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public UserActivityAction Action { get; init; }
    public Guid? ProductId { get; init; }
    public string? CategoryId { get; init; }
    public Guid? OrderId { get; init; }
    public string? SearchQuery { get; init; }
    public string? Metadata { get; init; }
    public string? SessionId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? PageUrl { get; init; }
    public string? ReferrerUrl { get; init; }
    public string? DeviceType { get; init; }
    public decimal? CartValue { get; init; }
    public int? CartItemCount { get; init; }
}

/// <summary>
/// Handles persisting a user activity event to the database.
/// </summary>
public class TrackUserActivityCommandHandler : IRequestHandler<TrackUserActivityCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public TrackUserActivityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(TrackUserActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = new UserActivity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Action = request.Action,
            ProductId = request.ProductId,
            CategoryId = request.CategoryId,
            OrderId = request.OrderId,
            SearchQuery = request.SearchQuery,
            Metadata = request.Metadata,
            SessionId = request.SessionId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            PageUrl = request.PageUrl,
            ReferrerUrl = request.ReferrerUrl,
            DeviceType = request.DeviceType,
            CartValue = request.CartValue,
            CartItemCount = request.CartItemCount
        };

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);

        return activity.Id;
    }
}
