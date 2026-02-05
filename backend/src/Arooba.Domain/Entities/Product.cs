using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a product listing on the Arooba Marketplace.
/// Supports volumetric weight calculation, pricing breakdown storage,
/// and status lifecycle transitions.
/// </summary>
public class Product : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public ProductCategory? Category { get; set; }
    public Guid ParentVendorId { get; set; }
    public ParentVendor? ParentVendor { get; set; }
    public Guid? SubVendorId { get; set; }
    public SubVendor? SubVendor { get; set; }
    public Guid? PickupLocationId { get; set; }
    public PickupLocation? PickupLocation { get; set; }
    public List<string>? Images { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public string? StatusReason { get; set; }
    public StockMode StockMode { get; set; } = StockMode.ReadyStock;
    public int? LeadTimeDays { get; set; }
    public int QuantityAvailable { get; set; }

    // Pricing fields
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal VendorBasePrice { get; set; }
    public decimal CooperativeFee { get; set; }
    public decimal MarketplaceUplift { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ParentUpliftAmount { get; set; }
    public decimal WithholdingTaxAmount { get; set; }
    public decimal VendorNetPayout { get; set; }

    // Dimensions and weight
    public decimal WeightKg { get; set; }
    public decimal DimensionL { get; set; }
    public decimal DimensionW { get; set; }
    public decimal DimensionH { get; set; }
    public decimal LengthCm { get => DimensionL; set => DimensionL = value; }
    public decimal WidthCm { get => DimensionW; set => DimensionW = value; }
    public decimal HeightCm { get => DimensionH; set => DimensionH = value; }
    public decimal VolumetricWeight { get; set; }
    public bool IsFragile { get; set; }
    public bool IsLocalOnly { get; set; }
    public List<string>? AllowedZoneIds { get; set; }

    /// <summary>
    /// Calculates the volumetric (dimensional) weight using the standard divisor of 5000.
    /// </summary>
    public decimal CalculateVolumetricWeight()
    {
        return (DimensionL * DimensionW * DimensionH) / 5000m;
    }

    /// <summary>Transitions the product from Draft to PendingReview.</summary>
    public Result SubmitForReview()
    {
        if (Status != ProductStatus.Draft)
            return Result.Failure("Only draft products can be submitted for review.");
        Status = ProductStatus.PendingReview;
        return Result.Success();
    }

    /// <summary>Approves a product that is pending review.</summary>
    public Result Approve()
    {
        if (Status != ProductStatus.PendingReview)
            return Result.Failure("Only products pending review can be approved.");
        Status = ProductStatus.Active;
        return Result.Success();
    }

    /// <summary>Rejects a product that is pending review.</summary>
    public Result Reject()
    {
        if (Status != ProductStatus.PendingReview)
            return Result.Failure("Only products pending review can be rejected.");
        Status = ProductStatus.Rejected;
        return Result.Success();
    }

    /// <summary>Pauses an active product listing.</summary>
    public Result Pause()
    {
        if (Status != ProductStatus.Active)
            return Result.Failure("Only active products can be paused.");
        Status = ProductStatus.Paused;
        return Result.Success();
    }

    /// <summary>Reactivates a paused product listing.</summary>
    public Result Unpause()
    {
        if (Status != ProductStatus.Paused)
            return Result.Failure("Only paused products can be unpaused.");
        Status = ProductStatus.Active;
        return Result.Success();
    }
}
