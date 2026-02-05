namespace Arooba.Application.Features.Products.Queries;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal VendorBasePrice { get; init; }
    public decimal FinalPrice { get; init; }
    public string CategoryId { get; init; } = string.Empty;
}
