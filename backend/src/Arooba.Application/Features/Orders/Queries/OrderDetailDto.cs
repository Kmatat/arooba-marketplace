namespace Arooba.Application.Features.Orders.Queries;

public record OrderDetailDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal ShippingFee { get; init; }
    public decimal VatAmount { get; init; }
}
