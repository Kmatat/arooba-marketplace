namespace Arooba.Domain.Enums;

/// <summary>
/// Indicates how a product's inventory is managed.
/// </summary>
public enum StockMode
{
    /// <summary>Product is available in stock and ready for immediate shipment.</summary>
    ReadyStock,

    /// <summary>Product is crafted on demand after an order is placed.</summary>
    MadeToOrder
}
