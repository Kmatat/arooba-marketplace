using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a customer order on the Arooba Marketplace.
/// Enforces valid status transitions throughout the order lifecycle.
/// </summary>
public class Order : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal TotalDeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal VatAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryZoneId { get; set; }
    public List<OrderItem> Items { get; set; } = [];

    /// <summary>Navigation property to the customer who placed this order.</summary>
    public Customer? Customer { get; set; }

    /// <summary>Navigation property to shipments for this order.</summary>
    public List<Shipment>? Shipments { get; set; }

    /// <summary>
    /// Vendor accepts a pending order.
    /// </summary>
    public Result Accept()
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure("Only pending orders can be accepted.");

        Status = OrderStatus.Accepted;
        return Result.Success();
    }

    /// <summary>
    /// Marks an accepted order as ready for courier pickup.
    /// </summary>
    public Result MarkReadyToShip()
    {
        if (Status != OrderStatus.Accepted)
            return Result.Failure("Only accepted orders can be marked ready to ship.");

        Status = OrderStatus.ReadyToShip;
        return Result.Success();
    }

    /// <summary>
    /// Marks an order as in transit after courier pickup.
    /// </summary>
    public Result MarkInTransit()
    {
        if (Status != OrderStatus.ReadyToShip)
            return Result.Failure("Only orders ready to ship can be marked in transit.");

        Status = OrderStatus.InTransit;
        return Result.Success();
    }

    /// <summary>
    /// Marks an order as successfully delivered.
    /// </summary>
    public Result MarkDelivered()
    {
        if (Status != OrderStatus.InTransit)
            return Result.Failure("Only orders in transit can be marked as delivered.");

        Status = OrderStatus.Delivered;
        return Result.Success();
    }

    /// <summary>
    /// Cancels an order. Only allowed from Pending or Accepted status.
    /// </summary>
    public Result Cancel()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Accepted)
            return Result.Failure("Only pending or accepted orders can be cancelled.");

        Status = OrderStatus.Cancelled;
        return Result.Success();
    }

    /// <summary>
    /// Returns a delivered order.
    /// </summary>
    public Result Return()
    {
        if (Status != OrderStatus.Delivered)
            return Result.Failure("Only delivered orders can be returned.");

        Status = OrderStatus.Returned;
        return Result.Success();
    }

    /// <summary>
    /// Rejects shipping for an order ready to ship (e.g., address issues).
    /// </summary>
    public Result RejectShipping()
    {
        if (Status != OrderStatus.ReadyToShip)
            return Result.Failure("Only orders ready to ship can have shipping rejected.");

        Status = OrderStatus.RejectedShipping;
        return Result.Success();
    }
}
