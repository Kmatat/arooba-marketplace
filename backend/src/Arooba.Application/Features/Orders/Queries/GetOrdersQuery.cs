using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using Arooba.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Orders.Queries;

/// <summary>
/// Query to retrieve a paginated list of orders with filtering by status,
/// customer, vendor, and date range.
/// </summary>
public record GetOrdersQuery : IRequest<PaginatedList<OrderDto>>
{
    /// <summary>Gets the page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the page size. Defaults to 10.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>Gets an optional order status filter.</summary>
    public OrderStatus? Status { get; init; }

    /// <summary>Gets an optional customer ID filter.</summary>
    public Guid? CustomerId { get; init; }

    /// <summary>Gets an optional vendor ID filter (filters by items belonging to this vendor).</summary>
    public Guid? VendorId { get; init; }

    /// <summary>Gets an optional start date filter (inclusive).</summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>Gets an optional end date filter (inclusive).</summary>
    public DateTime? DateTo { get; init; }

    /// <summary>Gets an optional search term to filter by order number.</summary>
    public string? SearchTerm { get; init; }
}

/// <summary>
/// DTO representing an order in list views.
/// </summary>
public record OrderDto
{
    /// <summary>Gets the order identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the human-readable order number.</summary>
    public string OrderNumber { get; init; } = default!;

    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the order status.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Gets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; init; }

    /// <summary>Gets the order subtotal in EGP.</summary>
    public decimal SubTotal { get; init; }

    /// <summary>Gets the total shipping fee in EGP.</summary>
    public decimal TotalShippingFee { get; init; }

    /// <summary>Gets the total order amount in EGP.</summary>
    public decimal TotalAmount { get; init; }

    /// <summary>Gets the number of items in the order.</summary>
    public int ItemCount { get; init; }

    /// <summary>Gets the number of shipments.</summary>
    public int ShipmentCount { get; init; }

    /// <summary>Gets the delivery city.</summary>
    public string DeliveryCity { get; init; } = default!;

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the delivery date, if delivered.</summary>
    public DateTime? DeliveredAt { get; init; }
}

/// <summary>
/// Handles the paginated order list query with filtering and search.
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedList<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetOrdersQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paginated, filtered list of orders.
    /// </summary>
    /// <param name="request">The query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paginated list of order DTOs.</returns>
    public async Task<PaginatedList<OrderDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Shipments)
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (request.VendorId.HasValue)
        {
            query = query.Where(o =>
                o.OrderItems != null &&
                o.OrderItems.Any(oi => oi.ParentVendorId == request.VendorId.Value));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(term));
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var projectedQuery = query.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerId = o.CustomerId,
            Status = o.Status,
            PaymentMethod = o.PaymentMethod,
            SubTotal = o.SubTotal,
            TotalShippingFee = o.TotalShippingFee,
            TotalAmount = o.TotalAmount,
            ItemCount = o.OrderItems != null ? o.OrderItems.Sum(oi => oi.Quantity) : 0,
            ShipmentCount = o.Shipments != null ? o.Shipments.Count : 0,
            DeliveryCity = o.DeliveryCity,
            CreatedAt = o.CreatedAt,
            DeliveredAt = o.DeliveredAt
        });

        return await PaginatedList<OrderDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
