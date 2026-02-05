namespace Arooba.Application.Features.Products.Queries;

public record ProductCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
