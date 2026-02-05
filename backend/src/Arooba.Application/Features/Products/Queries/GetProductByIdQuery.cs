using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Products.Queries;

/// <summary>
/// Query to retrieve a single product by ID with full pricing breakdown
/// and related information.
/// </summary>
public record GetProductByIdQuery : IRequest<ProductDetailDto>
{
    /// <summary>Gets the product identifier.</summary>
    public Guid ProductId { get; init; }
}

/// <summary>
/// Detailed DTO for a single product including full pricing breakdown.
/// </summary>
public record ProductDetailDto
{
    /// <summary>Gets the product identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the parent vendor identifier.</summary>
    public Guid ParentVendorId { get; init; }

    /// <summary>Gets the parent vendor business name.</summary>
    public string VendorBusinessName { get; init; } = default!;

    /// <summary>Gets the sub-vendor identifier, if applicable.</summary>
    public Guid? SubVendorId { get; init; }

    /// <summary>Gets the category identifier.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Gets the category name.</summary>
    public string CategoryName { get; init; } = default!;

    /// <summary>Gets the product title.</summary>
    public string Title { get; init; } = default!;

    /// <summary>Gets the Arabic product title.</summary>
    public string TitleAr { get; init; } = default!;

    /// <summary>Gets the description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the Arabic description.</summary>
    public string? DescriptionAr { get; init; }

    /// <summary>Gets the unique SKU.</summary>
    public string Sku { get; init; } = default!;

    /// <summary>Gets the product image URLs.</summary>
    public List<string> Images { get; init; } = new();

    /// <summary>Gets the product status.</summary>
    public ProductStatus Status { get; init; }

    /// <summary>Gets the status change reason.</summary>
    public string? StatusReason { get; init; }

    // Pricing breakdown
    /// <summary>Gets the vendor's cost price in EGP.</summary>
    public decimal CostPrice { get; init; }

    /// <summary>Gets the vendor's selling price in EGP.</summary>
    public decimal SellingPrice { get; init; }

    /// <summary>Gets the final customer-facing price in EGP.</summary>
    public decimal FinalPrice { get; init; }

    /// <summary>Gets the commission rate applied.</summary>
    public decimal CommissionRate { get; init; }

    /// <summary>Gets the commission amount in EGP.</summary>
    public decimal CommissionAmount { get; init; }

    /// <summary>Gets the VAT amount in EGP.</summary>
    public decimal VatAmount { get; init; }

    /// <summary>Gets the parent uplift amount in EGP.</summary>
    public decimal ParentUpliftAmount { get; init; }

    /// <summary>Gets the withholding tax amount in EGP.</summary>
    public decimal WithholdingTaxAmount { get; init; }

    /// <summary>Gets the vendor net payout in EGP.</summary>
    public decimal VendorNetPayout { get; init; }

    // Inventory and shipping
    /// <summary>Gets the stock mode.</summary>
    public StockMode StockMode { get; init; }

    /// <summary>Gets the lead time in days.</summary>
    public int? LeadTimeDays { get; init; }

    /// <summary>Gets the quantity available.</summary>
    public int QuantityAvailable { get; init; }

    /// <summary>Gets the weight in kilograms.</summary>
    public decimal WeightKg { get; init; }

    /// <summary>Gets the length in centimeters.</summary>
    public decimal? LengthCm { get; init; }

    /// <summary>Gets the width in centimeters.</summary>
    public decimal? WidthCm { get; init; }

    /// <summary>Gets the height in centimeters.</summary>
    public decimal? HeightCm { get; init; }

    /// <summary>Gets whether the product is local delivery only.</summary>
    public bool IsLocalOnly { get; init; }

    /// <summary>Gets the pickup location identifier.</summary>
    public Guid PickupLocationId { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the last update date.</summary>
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of a single product with full pricing breakdown
/// and related vendor/category information.
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetProductByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves the product with its vendor and category details
    /// and maps to a detailed DTO with full pricing breakdown.
    /// </summary>
    /// <param name="request">The query containing the product ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A detailed product DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the product is not found.</exception>
    public async Task<ProductDetailDto> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.ParentVendor)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        return new ProductDetailDto
        {
            Id = product.Id,
            ParentVendorId = product.ParentVendorId,
            VendorBusinessName = product.ParentVendor?.BusinessName ?? string.Empty,
            SubVendorId = product.SubVendorId,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Title = product.Title,
            TitleAr = product.TitleAr,
            Description = product.Description,
            DescriptionAr = product.DescriptionAr,
            Sku = product.Sku,
            Images = product.Images ?? new List<string>(),
            Status = product.Status,
            StatusReason = product.StatusReason,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            FinalPrice = product.FinalPrice,
            CommissionRate = product.CommissionRate,
            CommissionAmount = product.CommissionAmount,
            VatAmount = product.VatAmount,
            ParentUpliftAmount = product.ParentUpliftAmount,
            WithholdingTaxAmount = product.WithholdingTaxAmount,
            VendorNetPayout = product.VendorNetPayout,
            StockMode = product.StockMode,
            LeadTimeDays = product.LeadTimeDays,
            QuantityAvailable = product.QuantityAvailable,
            WeightKg = product.WeightKg,
            LengthCm = product.LengthCm,
            WidthCm = product.WidthCm,
            HeightCm = product.HeightCm,
            IsLocalOnly = product.IsLocalOnly,
            PickupLocationId = product.PickupLocationId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
