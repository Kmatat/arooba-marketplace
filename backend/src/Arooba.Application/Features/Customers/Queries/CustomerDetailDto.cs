namespace Arooba.Application.Features.Customers.Queries;

public record CustomerDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
