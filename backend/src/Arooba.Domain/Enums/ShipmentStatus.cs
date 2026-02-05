namespace Arooba.Domain.Enums;

/// <summary>Represents the status of a shipment.</summary>
public enum ShipmentStatus
{
    Pending,
    PickedUp,
    InTransit,
    Delivered,
    Failed,
    Returned
}
