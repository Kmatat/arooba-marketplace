using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Represents a product listing on the Arooba Marketplace.
/// Supports volumetric weight calculation and status lifecycle transitions.
/// </summary>
public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public decimal VendorBasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public StockMode StockMode { get; set; } = StockMode.ReadyStock;
    public int StockQuantity { get; set; }
    public decimal WeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public bool IsFragile { get; set; }

    /// <summary>
    /// Calculates the volumetric (dimensional) weight using the standard divisor of 5000.
    /// Formula: (Length x Width x Height) / 5000
    /// </summary>
    public decimal CalculateVolumetricWeight()
    {
        return (LengthCm * WidthCm * HeightCm) / 5000m;
    }

    /// <summary>
    /// Transitions the product from Draft to PendingReview.
    /// </summary>
    public Result SubmitForReview()
    {
        if (Status != ProductStatus.Draft)
            return Result.Failure("Only draft products can be submitted for review.");

        Status = ProductStatus.PendingReview;
        return Result.Success();
    }

    /// <summary>
    /// Approves a product that is pending review, making it Active.
    /// </summary>
    public Result Approve()
    {
        if (Status != ProductStatus.PendingReview)
            return Result.Failure("Only products pending review can be approved.");

        Status = ProductStatus.Active;
        return Result.Success();
    }

    /// <summary>
    /// Rejects a product that is pending review.
    /// </summary>
    public Result Reject()
    {
        if (Status != ProductStatus.PendingReview)
            return Result.Failure("Only products pending review can be rejected.");

        Status = ProductStatus.Rejected;
        return Result.Success();
    }

    /// <summary>
    /// Pauses an active product listing.
    /// </summary>
    public Result Pause()
    {
        if (Status != ProductStatus.Active)
            return Result.Failure("Only active products can be paused.");

        Status = ProductStatus.Paused;
        return Result.Success();
    }

    /// <summary>
    /// Reactivates a paused product listing.
    /// </summary>
    public Result Unpause()
    {
        if (Status != ProductStatus.Paused)
            return Result.Failure("Only paused products can be unpaused.");

        Status = ProductStatus.Active;
        return Result.Success();
    }
}
