using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Dashboard.Queries;

/// <summary>
/// Query to retrieve aggregate dashboard KPIs for the Arooba Marketplace admin panel.
/// </summary>
public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
    /// <summary>Gets an optional start date to scope the statistics.</summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>Gets an optional end date to scope the statistics.</summary>
    public DateTime? DateTo { get; init; }
}

/// <summary>
/// DTO containing all dashboard key performance indicators.
/// </summary>
public record DashboardStatsDto
{
    /// <summary>Gets the total Gross Merchandise Value in EGP.</summary>
    public decimal TotalGmv { get; init; }

    /// <summary>Gets the total number of orders.</summary>
    public int TotalOrders { get; init; }

    /// <summary>Gets the total number of active vendors.</summary>
    public int TotalActiveVendors { get; init; }

    /// <summary>Gets the total number of registered customers.</summary>
    public int TotalCustomers { get; init; }

    /// <summary>Gets the average order value in EGP.</summary>
    public decimal AverageOrderValue { get; init; }

    /// <summary>Gets the Cash on Delivery ratio (percentage of orders using COD).</summary>
    public decimal CodRatio { get; init; }

    /// <summary>Gets the return rate (percentage of orders returned).</summary>
    public decimal ReturnRate { get; init; }

    /// <summary>Gets the total number of active products.</summary>
    public int TotalActiveProducts { get; init; }

    /// <summary>Gets the total commission earned by Arooba in EGP.</summary>
    public decimal TotalCommission { get; init; }

    /// <summary>Gets the total pending vendor payouts in EGP.</summary>
    public decimal TotalPendingPayouts { get; init; }

    /// <summary>Gets the number of pending vendor applications.</summary>
    public int PendingVendorApplications { get; init; }

    /// <summary>Gets the number of products pending review.</summary>
    public int PendingProductReviews { get; init; }
}

/// <summary>
/// Handles computation of aggregate dashboard statistics from all major entities.
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetDashboardStatsQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Computes all dashboard KPIs by querying orders, vendors, customers,
    /// products, and financial data.
    /// </summary>
    /// <param name="request">The query parameters with optional date range.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A dashboard stats DTO with all computed KPIs.</returns>
    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Scope orders by date range if provided
        var ordersQuery = _context.Orders.AsNoTracking().AsQueryable();

        if (request.DateFrom.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CreatedAt <= request.DateTo.Value);
        }

        // GMV and order stats
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);
        var totalGmv = totalOrders > 0
            ? await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var averageOrderValue = totalOrders > 0 ? totalGmv / totalOrders : 0m;

        // COD ratio
        var codOrders = await ordersQuery.CountAsync(o => o.PaymentMethod == PaymentMethod.Cod, cancellationToken);
        var codRatio = totalOrders > 0 ? (decimal)codOrders / totalOrders * 100m : 0m;

        // Return rate
        var returnedOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Returned, cancellationToken);
        var deliveredOrReturned = await ordersQuery.CountAsync(
            o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Returned, cancellationToken);
        var returnRate = deliveredOrReturned > 0 ? (decimal)returnedOrders / deliveredOrReturned * 100m : 0m;

        // Vendor stats
        var totalActiveVendors = await _context.ParentVendors
            .CountAsync(v => v.Status == VendorStatus.Active, cancellationToken);

        var pendingVendorApplications = await _context.ParentVendors
            .CountAsync(v => v.Status == VendorStatus.Pending, cancellationToken);

        // Customer stats
        var totalCustomers = await _context.Customers.CountAsync(cancellationToken);

        // Product stats
        var totalActiveProducts = await _context.Products
            .CountAsync(p => p.Status == ProductStatus.Active, cancellationToken);

        var pendingProductReviews = await _context.Products
            .CountAsync(p => p.Status == ProductStatus.PendingReview, cancellationToken);

        // Commission stats from order items in the date range
        var orderIds = await ordersQuery.Select(o => o.Id).ToListAsync(cancellationToken);
        var totalCommission = orderIds.Count > 0
            ? await _context.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .SumAsync(oi => oi.CommissionAmount, cancellationToken)
            : 0m;

        // Pending payouts from wallets
        var totalPendingPayouts = await _context.VendorWallets
            .SumAsync(w => w.PendingBalance, cancellationToken);

        return new DashboardStatsDto
        {
            TotalGmv = totalGmv,
            TotalOrders = totalOrders,
            TotalActiveVendors = totalActiveVendors,
            TotalCustomers = totalCustomers,
            AverageOrderValue = Math.Round(averageOrderValue, 2),
            CodRatio = Math.Round(codRatio, 2),
            ReturnRate = Math.Round(returnRate, 2),
            TotalActiveProducts = totalActiveProducts,
            TotalCommission = totalCommission,
            TotalPendingPayouts = totalPendingPayouts,
            PendingVendorApplications = pendingVendorApplications,
            PendingProductReviews = pendingProductReviews
        };
    }
}
