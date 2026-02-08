using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a line item in an order.</summary>
public class OrderItem : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    /// <summary>The vendor's net payout for this line item (Bucket A).</summary>
    public decimal VendorPayout { get; set; }

    /// <summary>Alias for VendorPayout used in some contexts.</summary>
    public decimal VendorNetPayout
    {
        get => VendorPayout;
        set => VendorPayout = value;
    }

    public decimal CommissionAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ParentUpliftAmount { get; set; }
    public decimal WithholdingTaxAmount { get; set; }

    /// <summary>The parent vendor who owns this product.</summary>
    public Guid ParentVendorId { get; set; }

    /// <summary>The sub-vendor who created this product (optional).</summary>
    public Guid? SubVendorId { get; set; }

    /// <summary>The pickup location for this item.</summary>
    public Guid? PickupLocationId { get; set; }

    /// <summary>The shipment this item is assigned to.</summary>
    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }
}
