using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Orders.Commands;

/// <summary>
/// Command to update the status of an order or its shipments through the delivery lifecycle.
/// Handles status transitions and triggers financial operations (e.g., releasing funds on delivery).
/// </summary>
public record UpdateOrderStatusCommand : IRequest<bool>
{
    /// <summary>Gets the order identifier.</summary>
    public Guid OrderId { get; init; }

    /// <summary>Gets the optional shipment identifier (if updating a specific shipment).</summary>
    public Guid? ShipmentId { get; init; }

    /// <summary>Gets the new status to apply.</summary>
    public OrderStatus NewStatus { get; init; }

    /// <summary>Gets an optional note about the status change.</summary>
    public string? Note { get; init; }
}

/// <summary>
/// Handles order and shipment status transitions including financial operations
/// like releasing pending funds to available balance on delivery.
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateOrderStatusCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Updates the order or shipment status and performs associated financial operations.
    /// When a shipment is delivered, pending funds are moved to available balance.
    /// When an order is cancelled or returned, stock is restored and funds are reversed.
    /// </summary>
    /// <param name="request">The update status command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the status was updated successfully.</returns>
    /// <exception cref="NotFoundException">Thrown when the order or shipment is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the status transition is not allowed.</exception>
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTime.UtcNow;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Shipments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        // If updating a specific shipment
        if (request.ShipmentId.HasValue)
        {
            var shipment = order.Shipments?.FirstOrDefault(s => s.Id == request.ShipmentId.Value);

            if (shipment is null)
            {
                throw new NotFoundException(nameof(Shipment), request.ShipmentId.Value);
            }

            ValidateStatusTransition(shipment.Status, request.NewStatus);

            shipment.Status = request.NewStatus;
            shipment.UpdatedAt = now;

            // On delivery, release pending funds to available balance for items in this shipment
            if (request.NewStatus == OrderStatus.Delivered)
            {
                await ReleaseFundsForShipmentAsync(order, shipment, now, cancellationToken);
                shipment.DeliveredAt = now;
            }

            // On return, reverse funds and restore stock
            if (request.NewStatus == OrderStatus.Returned)
            {
                await ReverseFundsForShipmentAsync(order, shipment, now, cancellationToken);
            }

            // Check if all shipments are delivered/completed to update order status
            if (order.Shipments!.All(s => s.Status == OrderStatus.Delivered))
            {
                order.Status = OrderStatus.Delivered;
                order.DeliveredAt = now;
            }
        }
        else
        {
            // Updating the entire order status
            ValidateStatusTransition(order.Status, request.NewStatus);

            order.Status = request.NewStatus;

            if (request.NewStatus == OrderStatus.Cancelled)
            {
                await HandleOrderCancellationAsync(order, now, cancellationToken);
            }

            // Update all shipments to match the order status
            if (order.Shipments is not null)
            {
                foreach (var shipment in order.Shipments)
                {
                    if (shipment.Status != OrderStatus.Delivered && shipment.Status != OrderStatus.Returned)
                    {
                        shipment.Status = request.NewStatus;
                        shipment.UpdatedAt = now;
                    }
                }
            }
        }

        order.UpdatedAt = now;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Validates that the status transition is allowed.
    /// </summary>
    private static void ValidateStatusTransition(OrderStatus current, OrderStatus next)
    {
        var isValid = (current, next) switch
        {
            (OrderStatus.Pending, OrderStatus.Accepted) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Accepted, OrderStatus.ReadyToShip) => true,
            (OrderStatus.Accepted, OrderStatus.Cancelled) => true,
            (OrderStatus.ReadyToShip, OrderStatus.InTransit) => true,
            (OrderStatus.InTransit, OrderStatus.Delivered) => true,
            (OrderStatus.InTransit, OrderStatus.RejectedShipping) => true,
            (OrderStatus.Delivered, OrderStatus.Returned) => true,
            _ => false
        };

        if (!isValid)
        {
            throw new BadRequestException(
                $"Cannot transition order from {current} to {next}.");
        }
    }

    /// <summary>
    /// Releases pending funds to available balance when a shipment is delivered.
    /// </summary>
    private async Task ReleaseFundsForShipmentAsync(
        Order order,
        Shipment shipment,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var shipmentItems = order.OrderItems?
            .Where(oi => oi.ShipmentId == shipment.Id)
            .ToList() ?? new List<OrderItem>();

        var vendorGroups = shipmentItems.GroupBy(oi => oi.ParentVendorId);

        foreach (var vendorGroup in vendorGroups)
        {
            var vendorPayout = vendorGroup.Sum(oi => oi.VendorNetPayout);

            var wallet = await _context.VendorWallets
                .FirstOrDefaultAsync(w => w.ParentVendorId == vendorGroup.Key, cancellationToken);

            if (wallet is not null)
            {
                wallet.PendingBalance -= vendorPayout;
                wallet.AvailableBalance += vendorPayout;
            }

            // Update ledger entry to available
            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                ParentVendorId = vendorGroup.Key,
                OrderId = order.Id,
                TransactionType = TransactionType.Sale,
                Amount = vendorGroup.Sum(oi => oi.TotalPrice),
                VendorAmount = vendorPayout,
                CommissionAmount = vendorGroup.Sum(oi => oi.CommissionAmount),
                VatAmount = vendorGroup.Sum(oi => oi.VatAmount),
                Description = $"Funds released for delivered shipment {shipment.TrackingNumber}",
                BalanceStatus = BalanceStatus.Available,
                CreatedAt = now
            };

            _context.LedgerEntries.Add(ledgerEntry);
        }
    }

    /// <summary>
    /// Reverses funds and restores stock when a shipment is returned.
    /// </summary>
    private async Task ReverseFundsForShipmentAsync(
        Order order,
        Shipment shipment,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var shipmentItems = order.OrderItems?
            .Where(oi => oi.ShipmentId == shipment.Id)
            .ToList() ?? new List<OrderItem>();

        var vendorGroups = shipmentItems.GroupBy(oi => oi.ParentVendorId);

        foreach (var vendorGroup in vendorGroups)
        {
            var vendorPayout = vendorGroup.Sum(oi => oi.VendorNetPayout);

            var wallet = await _context.VendorWallets
                .FirstOrDefaultAsync(w => w.ParentVendorId == vendorGroup.Key, cancellationToken);

            if (wallet is not null)
            {
                // Deduct from available (if already released) or pending
                if (wallet.AvailableBalance >= vendorPayout)
                {
                    wallet.AvailableBalance -= vendorPayout;
                }
                else
                {
                    wallet.PendingBalance -= vendorPayout;
                }

                wallet.TotalEarnings -= vendorPayout;
            }

            // Record refund ledger entry
            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                ParentVendorId = vendorGroup.Key,
                OrderId = order.Id,
                TransactionType = TransactionType.Refund,
                Amount = -vendorGroup.Sum(oi => oi.TotalPrice),
                VendorAmount = -vendorPayout,
                CommissionAmount = -vendorGroup.Sum(oi => oi.CommissionAmount),
                VatAmount = -vendorGroup.Sum(oi => oi.VatAmount),
                Description = $"Return reversal for shipment {shipment.TrackingNumber}",
                BalanceStatus = BalanceStatus.Available,
                CreatedAt = now
            };

            _context.LedgerEntries.Add(ledgerEntry);
        }

        // Restore stock for returned items
        foreach (var item in shipmentItems)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

            if (product is not null && product.StockMode == StockMode.ReadyStock)
            {
                product.QuantityAvailable += item.Quantity;
            }
        }
    }

    /// <summary>
    /// Handles full order cancellation: reverses all pending funds and restores stock.
    /// </summary>
    private async Task HandleOrderCancellationAsync(
        Order order,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (order.OrderItems is null) return;

        var vendorGroups = order.OrderItems.GroupBy(oi => oi.ParentVendorId);

        foreach (var vendorGroup in vendorGroups)
        {
            var vendorPayout = vendorGroup.Sum(oi => oi.VendorNetPayout);

            var wallet = await _context.VendorWallets
                .FirstOrDefaultAsync(w => w.ParentVendorId == vendorGroup.Key, cancellationToken);

            if (wallet is not null)
            {
                wallet.PendingBalance -= vendorPayout;
                wallet.TotalEarnings -= vendorPayout;
            }

            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                ParentVendorId = vendorGroup.Key,
                OrderId = order.Id,
                TransactionType = TransactionType.Refund,
                Amount = -vendorGroup.Sum(oi => oi.TotalPrice),
                VendorAmount = -vendorPayout,
                CommissionAmount = -vendorGroup.Sum(oi => oi.CommissionAmount),
                VatAmount = -vendorGroup.Sum(oi => oi.VatAmount),
                Description = $"Order {order.OrderNumber} cancelled",
                BalanceStatus = BalanceStatus.Available,
                CreatedAt = now
            };

            _context.LedgerEntries.Add(ledgerEntry);
        }

        // Restore stock for all items
        foreach (var item in order.OrderItems)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

            if (product is not null && product.StockMode == StockMode.ReadyStock)
            {
                product.QuantityAvailable += item.Quantity;
            }
        }
    }
}

/// <summary>
/// Validates the <see cref="UpdateOrderStatusCommand"/>.
/// </summary>
public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    /// <summary>
    /// Initializes validation rules for order status updates.
    /// </summary>
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(o => o.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(o => o.NewStatus)
            .IsInEnum().WithMessage("A valid order status is required.");
    }
}
