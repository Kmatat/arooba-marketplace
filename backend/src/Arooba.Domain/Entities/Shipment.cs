using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Shipment in the Arooba Marketplace domain.</summary>
public class Shipment : AuditableEntity
{
    public int OrderId { get; set; }

    /// <summary>The pickup location for this shipment.</summary>
    public int PickupLocationId { get; set; }

    public string? TrackingNumber { get; set; }
    public string? CourierProvider { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal CodAmountDue { get; set; }
    public ShipmentStatus Status { get; set; }

    /// <summary>Total weight of all items in this shipment (kg).</summary>
    public decimal TotalWeight { get; set; }

    /// <summary>Total number of items in this shipment.</summary>
    public int ItemCount { get; set; }

    /// <summary>Estimated delivery date.</summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    /// <summary>Navigation property to the associated order.</summary>
    public Order? Order { get; set; }

    /// <summary>Navigation property to the pickup location.</summary>
    public PickupLocation? PickupLocation { get; set; }

    /// <summary>Navigation property to order items in this shipment.</summary>
    public List<OrderItem>? Items { get; set; }
    public DateTime DeliveredAt { get; set; }

}
