namespace Arooba.Application.Features.Customers.Queries;

public record CustomerAddressDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
