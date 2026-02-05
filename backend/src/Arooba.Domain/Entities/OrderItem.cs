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
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal VendorPayout { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ParentUpliftAmount { get; set; }
    public decimal WithholdingTaxAmount { get; set; }
}
