namespace Arooba.Application.Features.Orders.Queries;

public record OrderItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
