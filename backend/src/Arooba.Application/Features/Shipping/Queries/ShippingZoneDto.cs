namespace Arooba.Application.Features.Shipping.Queries;

public record ShippingZoneDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
