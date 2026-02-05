using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.Products.Commands;

/// <summary>
/// Command to create a new product listing on the marketplace.
/// </summary>
public record CreateProductCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public decimal VendorBasePrice { get; init; }
    public StockMode StockMode { get; init; }
    public int StockQuantity { get; init; }
    public decimal WeightKg { get; init; }
    public decimal LengthCm { get; init; }
    public decimal WidthCm { get; init; }
    public decimal HeightCm { get; init; }
    public bool IsFragile { get; init; }
}
