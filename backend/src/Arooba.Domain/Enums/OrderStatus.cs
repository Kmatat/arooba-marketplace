namespace Arooba.Domain.Enums;

/// <summary>
/// Tracks the lifecycle of an order or shipment from placement through delivery.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order has been placed and is awaiting vendor confirmation.</summary>
    Pending,

    /// <summary>Vendor has accepted the order and is preparing it.</summary>
    Accepted,

    /// <summary>Order is packed and ready for courier pickup.</summary>
    ReadyToShip,

    /// <summary>Order has been picked up by the courier and is in transit.</summary>
    InTransit,

    /// <summary>Order has been successfully delivered to the customer.</summary>
    Delivered,

    /// <summary>Order has been returned by the customer.</summary>
    Returned,

    /// <summary>Order has been cancelled before shipment.</summary>
    Cancelled,

    /// <summary>Shipment was rejected at the shipping stage (e.g., address issues).</summary>
    RejectedShipping
}
