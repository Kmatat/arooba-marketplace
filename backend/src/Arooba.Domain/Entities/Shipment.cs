using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Shipment in the Arooba Marketplace domain.</summary>
public class Shipment : AuditableEntity
{
    public Guid OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CourierProvider { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal CodAmountDue { get; set; }
    public ShipmentStatus Status { get; set; }

    /// <summary>Navigation property to the associated order.</summary>
    public Order? Order { get; set; }
}
