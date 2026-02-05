namespace Arooba.Application.Features.Orders.Queries;

public record ShipmentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
