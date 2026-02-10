namespace Arooba.Application.Features.Orders.Queries;

public record OrderDetailDto_v2
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal ShippingFee { get; init; }
    public decimal VatAmount { get; init; }
}
