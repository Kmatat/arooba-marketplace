namespace Arooba.Application.Features.Orders.Queries;

public record OrderDto_v2
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}
