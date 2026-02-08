using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Exceptions;
using Arooba.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve performance metrics and loyalty data for a customer.
/// Calculates RFM scores, spending trends, and referral statistics.
/// </summary>
public record GetCustomerPerformanceQuery : IRequest<CustomerPerformanceDto>
{
    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }
}

/// <summary>
/// DTO representing customer performance and loyalty metrics.
/// </summary>
public record CustomerPerformanceDto
{
    public Guid CustomerId { get; init; }

    // Loyalty
    public int PointsEarned { get; init; }
    public int PointsRedeemed { get; init; }
    public int PointsBalance { get; init; }
    public string CurrentTier { get; init; } = default!;
    public int TierProgress { get; init; }
    public string NextTier { get; init; } = default!;
    public int PointsToNextTier { get; init; }

    // Referrals
    public int ReferralCount { get; init; }
    public decimal ReferralEarnings { get; init; }

    // Order Stats
    public int TotalOrders { get; init; }
    public decimal TotalSpent { get; init; }
    public decimal AverageOrderValue { get; init; }
    public int TotalReturns { get; init; }
    public decimal ReturnRate { get; init; }

    // Engagement
    public int TotalReviews { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalSessions { get; init; }

    // Monthly Spending (last 6 months)
    public List<MonthlySpendingDto> MonthlySpending { get; init; } = new();
}

/// <summary>
/// DTO for monthly spending breakdown.
/// </summary>
public record MonthlySpendingDto
{
    public string Month { get; init; } = default!;
    public decimal Amount { get; init; }
}

/// <summary>
/// Handles calculation of customer performance metrics.
/// </summary>
public class GetCustomerPerformanceQueryHandler : IRequestHandler<GetCustomerPerformanceQuery, CustomerPerformanceDto>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerPerformanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerPerformanceDto> Handle(
        GetCustomerPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        // Calculate tier progress
        var tierThresholds = new[] { 0, 5000, 10000, 20000 };
        var tierNames = new[] { "Bronze", "Silver", "Gold", "Platinum" };
        var tierIndex = (int)customer.Tier;
        var nextTierIndex = Math.Min(tierIndex + 1, 3);
        var currentThreshold = tierThresholds[tierIndex];
        var nextThreshold = tierThresholds[nextTierIndex];
        var tierProgress = tierIndex >= 3 ? 100 :
            (int)((double)(customer.LifetimeLoyaltyPoints - currentThreshold) / (nextThreshold - currentThreshold) * 100);
        var pointsToNextTier = tierIndex >= 3 ? 0 : nextThreshold - customer.LifetimeLoyaltyPoints;

        // Order stats
        var totalOrders = await _context.Orders
            .CountAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        var totalReturns = await _context.Orders
            .CountAsync(o => o.CustomerId == request.CustomerId && o.Status == Domain.Enums.OrderStatus.Returned, cancellationToken);
        var returnRate = totalOrders > 0 ? (decimal)totalReturns / totalOrders * 100 : 0;

        // Monthly spending (last 6 months)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var monthlySpending = await _context.Orders
            .Where(o => o.CustomerId == request.CustomerId && o.CreatedAt >= sixMonthsAgo)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlySpendingDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Amount = g.Sum(o => o.TotalAmount),
            })
            .OrderBy(m => m.Month)
            .ToListAsync(cancellationToken);

        // Review stats
        var totalReviews = await _context.CustomerReviews
            .CountAsync(r => r.CustomerId == request.CustomerId, cancellationToken);
        var avgRating = totalReviews > 0
            ? await _context.CustomerReviews
                .Where(r => r.CustomerId == request.CustomerId)
                .AverageAsync(r => (decimal)r.Rating, cancellationToken)
            : 0m;

        return new CustomerPerformanceDto
        {
            CustomerId = request.CustomerId,
            PointsEarned = customer.LifetimeLoyaltyPoints,
            PointsRedeemed = customer.LifetimeLoyaltyPoints - customer.LoyaltyPoints,
            PointsBalance = customer.LoyaltyPoints,
            CurrentTier = tierNames[tierIndex],
            TierProgress = Math.Clamp(tierProgress, 0, 100),
            NextTier = tierNames[nextTierIndex],
            PointsToNextTier = Math.Max(0, pointsToNextTier),
            ReferralCount = customer.ReferralCount,
            ReferralEarnings = customer.ReferralCount * 50m, // Give 50 per referral
            TotalOrders = totalOrders,
            TotalSpent = customer.TotalSpent,
            AverageOrderValue = totalOrders > 0 ? customer.TotalSpent / totalOrders : 0,
            TotalReturns = totalReturns,
            ReturnRate = Math.Round(returnRate, 1),
            TotalReviews = totalReviews,
            AverageRating = Math.Round(avgRating, 1),
            TotalSessions = customer.TotalSessions,
            MonthlySpending = monthlySpending,
        };
    }
}
