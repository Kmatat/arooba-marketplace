using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Orders.Queries;

/// <summary>
/// Query to retrieve a single order by ID with full details including
/// items, shipments, and financial breakdown.
/// </summary>
public record GetOrderByIdQuery : IRequest<OrderDetailDto>
{
    /// <summary>Gets the order identifier.</summary>
    public Guid OrderId { get; init; }
}

/// <summary>
/// DTO for an individual order line item.
/// </summary>
public record OrderItemDto
{
    /// <summary>Gets the order item identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the product identifier.</summary>
    public Guid ProductId { get; init; }

    /// <summary>Gets the product title at time of order.</summary>
    public string ProductTitle { get; init; } = default!;

    /// <summary>Gets the product SKU at time of order.</summary>
    public string ProductSku { get; init; } = default!;

    /// <summary>Gets the quantity ordered.</summary>
    public int Quantity { get; init; }

    /// <summary>Gets the unit price in EGP.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Gets the total price for this line item in EGP.</summary>
    public decimal TotalPrice { get; init; }

    /// <summary>Gets the vendor net payout in EGP.</summary>
    public decimal VendorNetPayout { get; init; }

    /// <summary>Gets the commission amount in EGP.</summary>
    public decimal CommissionAmount { get; init; }

    /// <summary>Gets the VAT amount in EGP.</summary>
    public decimal VatAmount { get; init; }

    /// <summary>Gets the parent uplift amount in EGP.</summary>
    public decimal ParentUpliftAmount { get; init; }

    /// <summary>Gets the withholding tax amount in EGP.</summary>
    public decimal WithholdingTaxAmount { get; init; }

    /// <summary>Gets the parent vendor identifier.</summary>
    public Guid ParentVendorId { get; init; }

    /// <summary>Gets the sub-vendor identifier, if applicable.</summary>
    public Guid? SubVendorId { get; init; }

    /// <summary>Gets the shipment identifier this item belongs to.</summary>
    public Guid? ShipmentId { get; init; }
}

/// <summary>
/// DTO for a shipment within an order.
/// </summary>
public record ShipmentDto
{
    /// <summary>Gets the shipment identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the pickup location identifier.</summary>
    public Guid PickupLocationId { get; init; }

    /// <summary>Gets the shipment status.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Gets the tracking number.</summary>
    public string TrackingNumber { get; init; } = default!;

    /// <summary>Gets the total weight of the shipment in kilograms.</summary>
    public decimal TotalWeight { get; init; }

    /// <summary>Gets the number of items in the shipment.</summary>
    public int ItemCount { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the delivery date, if delivered.</summary>
    public DateTime? DeliveredAt { get; init; }
}

/// <summary>
/// Detailed DTO for a single order including items, shipments, and full financial breakdown.
/// </summary>
public record OrderDetailDto
{
    /// <summary>Gets the order identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the human-readable order number.</summary>
    public string OrderNumber { get; init; } = default!;

    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the customer name.</summary>
    public string CustomerName { get; init; } = default!;

    /// <summary>Gets the order status.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Gets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; init; }

    /// <summary>Gets the delivery address.</summary>
    public string DeliveryAddress { get; init; } = default!;

    /// <summary>Gets the delivery city.</summary>
    public string DeliveryCity { get; init; } = default!;

    /// <summary>Gets the delivery zone identifier.</summary>
    public Guid DeliveryZoneId { get; init; }

    /// <summary>Gets the order subtotal in EGP.</summary>
    public decimal SubTotal { get; init; }

    /// <summary>Gets the total shipping fee in EGP.</summary>
    public decimal TotalShippingFee { get; init; }

    /// <summary>Gets the total order amount in EGP.</summary>
    public decimal TotalAmount { get; init; }

    /// <summary>Gets the total commission collected in EGP.</summary>
    public decimal TotalCommission { get; init; }

    /// <summary>Gets the total VAT collected in EGP.</summary>
    public decimal TotalVat { get; init; }

    /// <summary>Gets the total vendor payouts in EGP.</summary>
    public decimal TotalVendorPayout { get; init; }

    /// <summary>Gets the list of order items.</summary>
    public List<OrderItemDto> Items { get; init; } = new();

    /// <summary>Gets the list of shipments.</summary>
    public List<ShipmentDto> Shipments { get; init; } = new();

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the last update date.</summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>Gets the delivery date, if delivered.</summary>
    public DateTime? DeliveredAt { get; init; }
}

/// <summary>
/// Handles retrieval of a single order with full detail.
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetOrderByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves the order with items, shipments, customer info, and computed financial totals.
    /// </summary>
    /// <param name="request">The query containing the order ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A detailed order DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the order is not found.</exception>
    public async Task<OrderDetailDto> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Shipments)
            .Include(o => o.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        var items = order.OrderItems?.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductTitle = oi.ProductTitle,
            ProductSku = oi.ProductSku,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            TotalPrice = oi.TotalPrice,
            VendorNetPayout = oi.VendorNetPayout,
            CommissionAmount = oi.CommissionAmount,
            VatAmount = oi.VatAmount,
            ParentUpliftAmount = oi.ParentUpliftAmount,
            WithholdingTaxAmount = oi.WithholdingTaxAmount,
            ParentVendorId = oi.ParentVendorId,
            SubVendorId = oi.SubVendorId,
            ShipmentId = oi.ShipmentId
        }).ToList() ?? new List<OrderItemDto>();

        var shipments = order.Shipments?.Select(s => new ShipmentDto
        {
            Id = s.Id,
            PickupLocationId = s.PickupLocationId,
            Status = s.Status,
            TrackingNumber = s.TrackingNumber,
            TotalWeight = s.TotalWeight,
            ItemCount = s.ItemCount,
            CreatedAt = s.CreatedAt,
            DeliveredAt = s.DeliveredAt
        }).ToList() ?? new List<ShipmentDto>();

        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.FullName ?? string.Empty,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryCity = order.DeliveryCity,
            DeliveryZoneId = order.DeliveryZoneId,
            SubTotal = order.SubTotal,
            TotalShippingFee = order.TotalShippingFee,
            TotalAmount = order.TotalAmount,
            TotalCommission = items.Sum(i => i.CommissionAmount),
            TotalVat = items.Sum(i => i.VatAmount),
            TotalVendorPayout = items.Sum(i => i.VendorNetPayout),
            Items = items,
            Shipments = shipments,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            DeliveredAt = order.DeliveredAt
        };
    }
}
