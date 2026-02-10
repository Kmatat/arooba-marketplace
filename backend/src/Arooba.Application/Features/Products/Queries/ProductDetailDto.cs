namespace Arooba.Application.Features.Products.Queries;

public record ProductDetailDto_v2
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal VendorBasePrice { get; init; }
    public decimal FinalPrice { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public decimal LengthCm { get; init; }
    public decimal WidthCm { get; init; }
    public decimal HeightCm { get; init; }
}
