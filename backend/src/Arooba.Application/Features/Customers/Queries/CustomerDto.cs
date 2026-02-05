namespace Arooba.Application.Features.Customers.Queries;

public record CustomerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
