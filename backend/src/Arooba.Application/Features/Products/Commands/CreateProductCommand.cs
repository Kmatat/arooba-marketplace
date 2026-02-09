using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Products.Commands;

/// <summary>
/// Command to create a new product listing on the marketplace.
/// The pricing engine automatically calculates the final price based on
/// the vendor base price, category uplifts, and vendor type.
/// </summary>
public record CreateProductCommand : IRequest<Guid>
{
    /// <summary>The parent vendor creating this product.</summary>
    public Guid ParentVendorId { get; init; }

    /// <summary>Optional sub-vendor who created this product.</summary>
    public Guid? SubVendorId { get; init; }

    /// <summary>Product title in English.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Product title in Arabic.</summary>
    public string? TitleAr { get; init; }

    /// <summary>Product description in English.</summary>
    public string? Description { get; init; }

    /// <summary>Product description in Arabic.</summary>
    public string? DescriptionAr { get; init; }

    /// <summary>Category ID (e.g., "jewelry-accessories", "home-decor-fragile").</summary>
    public string CategoryId { get; init; } = string.Empty;

    /// <summary>Vendor's cost price (for margin tracking).</summary>
    public decimal CostPrice { get; init; }

    /// <summary>Vendor's selling/base price before uplifts.</summary>
    public decimal SellingPrice { get; init; }

    /// <summary>Optional custom uplift override (null uses category default).</summary>
    public decimal? CustomUpliftOverride { get; init; }

    /// <summary>Product images (URLs).</summary>
    public List<string>? Images { get; init; }

    /// <summary>Stock mode: ReadyStock or MadeToOrder.</summary>
    public StockMode StockMode { get; init; }

    /// <summary>Available quantity (for ReadyStock mode).</summary>
    public int QuantityAvailable { get; init; }

    /// <summary>Lead time in days (for MadeToOrder mode).</summary>
    public int? LeadTimeDays { get; init; }

    /// <summary>Product weight in kilograms.</summary>
    public decimal WeightKg { get; init; }

    /// <summary>Product length in centimeters.</summary>
    public decimal DimensionL { get; init; }

    /// <summary>Product width in centimeters.</summary>
    public decimal DimensionW { get; init; }

    /// <summary>Product height in centimeters.</summary>
    public decimal DimensionH { get; init; }

    /// <summary>Whether the product is local-only (can't ship outside vendor zone).</summary>
    public bool IsLocalOnly { get; init; }

    /// <summary>Pickup location ID for this product.</summary>
    public Guid? PickupLocationId { get; init; }
}

/// <summary>
/// Handles product creation with full pricing engine integration.
/// Calculates the 4-bucket waterfall and sets final price automatically.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IDateTimeService _dateTime;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        IPricingService pricingService,
        IDateTimeService dateTime)
    {
        _context = context;
        _pricingService = pricingService;
        _dateTime = dateTime;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTime.UtcNow;

        // Step 1: Validate parent vendor exists and get VAT registration status
        var vendor = await _context.ParentVendors
            .FirstOrDefaultAsync(v => v.Id == request.ParentVendorId, cancellationToken);

        if (vendor is null)
        {
            throw new NotFoundException(nameof(ParentVendor), request.ParentVendorId);
        }

        // Step 2: Validate category exists
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(nameof(ProductCategory), request.CategoryId);
        }

        // Step 3: Get sub-vendor uplift if applicable
        decimal? parentUpliftValue = null;
        string? parentUpliftType = null;

        if (request.SubVendorId.HasValue)
        {
            var subVendor = await _context.SubVendors
                .FirstOrDefaultAsync(sv => sv.Id == request.SubVendorId.Value, cancellationToken);

            if (subVendor is not null)
            {
                parentUpliftType = subVendor.UpliftType.HasValue.ToString().ToLowerInvariant();
                parentUpliftValue = subVendor.UpliftValue;
            }
        }

        // Step 4: Calculate pricing using the pricing engine
        var pricingInput = new PricingInput(
            VendorBasePrice: request.SellingPrice,
            CategoryId: request.CategoryId,
            IsVendorVatRegistered: vendor.IsVatRegistered,
            IsNonLegalizedVendor: vendor.VendorType == VendorType.NonLegalized,
            ParentUpliftType: parentUpliftType,
            ParentUpliftValue: parentUpliftValue,
            CustomUpliftOverride: request.CustomUpliftOverride
        );

        var pricingResult = _pricingService.CalculatePrice(pricingInput);

        // Step 5: Generate SKU
        var sku = GenerateSku(vendor.BusinessNameAr, request.CategoryId, now);

        // Step 6: Calculate volumetric weight
        var volumetricWeight = (request.DimensionL * request.DimensionW * request.DimensionH) / 5000m;

        // Step 7: Create the product entity with all pricing fields
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Sku = sku,
            ParentVendorId = request.ParentVendorId,
            SubVendorId = request.SubVendorId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            TitleAr = request.TitleAr ?? string.Empty,
            Description = request.Description,
            DescriptionAr = request.DescriptionAr,
            Images = request.Images ?? new List<string>(),
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,

            // Pricing breakdown from engine
            FinalPrice = pricingResult.FinalPrice,
            VendorNetPayout = pricingResult.BucketA_VendorRevenue,
            CommissionAmount = pricingResult.BucketC_AroobaRevenue,
            VatAmount = pricingResult.BucketB_VendorVat + pricingResult.BucketD_AroobaVat,
            ParentUpliftAmount = pricingResult.ParentUpliftAmount,
            WithholdingTaxAmount = 0m, // Calculated at payout time

            // Stock management
            StockMode = request.StockMode,
            QuantityAvailable = request.QuantityAvailable,
            LeadTimeDays = request.LeadTimeDays,

            // Shipping dimensions
            WeightKg = request.WeightKg,
            DimensionL = request.DimensionL,
            DimensionW = request.DimensionW,
            DimensionH = request.DimensionH,
            VolumetricWeight = volumetricWeight,

            // Location and status
            PickupLocationId = request.PickupLocationId,
            IsLocalOnly = request.IsLocalOnly,
            Status = ProductStatus.PendingReview,
            IsFeatured = false,

            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return productId;
    }

    /// <summary>
    /// Generates a unique SKU for the product.
    /// Format: VENDOR-CATEGORY-YYYYMMDD-XXXX
    /// </summary>
    private static string GenerateSku(string vendorName, string categoryId, DateTime date)
    {
        var vendorPart = new string(vendorName
            .ToUpperInvariant()
            .Replace(" ", "")
            .Take(5)
            .ToArray());

        var categoryPart = new string(categoryId
            .ToUpperInvariant()
            .Replace("-", "")
            .Take(4)
            .ToArray());

        var datePart = date.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();

        return $"{vendorPart}-{categoryPart}-{datePart}-{randomPart}";
    }
}
