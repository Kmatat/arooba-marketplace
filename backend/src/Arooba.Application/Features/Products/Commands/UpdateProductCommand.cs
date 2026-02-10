using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Products.Commands;

/// <summary>
/// Command to update an existing product's details and recalculate its pricing
/// through the pricing engine.
/// </summary>
public record UpdateProductCommand : IRequest<bool>
{
    /// <summary>Gets the product identifier to update.</summary>
    public int ProductId { get; init; }

    /// <summary>Gets the updated product title.</summary>
    public string? Title { get; init; }

    /// <summary>Gets the updated Arabic product title.</summary>
    public string? TitleAr { get; init; }

    /// <summary>Gets the updated description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the updated Arabic description.</summary>
    public string? DescriptionAr { get; init; }

    /// <summary>Gets the updated image URLs.</summary>
    public List<string>? Images { get; init; }

    /// <summary>Gets the updated cost price in EGP.</summary>
    public decimal? CostPrice { get; init; }

    /// <summary>Gets the updated selling price in EGP (triggers repricing).</summary>
    public decimal? SellingPrice { get; init; }

    /// <summary>Gets the updated quantity available.</summary>
    public int? QuantityAvailable { get; init; }

    /// <summary>Gets the updated weight in kilograms.</summary>
    public decimal? WeightKg { get; init; }

    /// <summary>Gets the updated length in centimeters.</summary>
    public decimal? LengthCm { get; init; }

    /// <summary>Gets the updated width in centimeters.</summary>
    public decimal? WidthCm { get; init; }

    /// <summary>Gets the updated height in centimeters.</summary>
    public decimal? HeightCm { get; init; }

    /// <summary>Gets whether the product is restricted to local delivery.</summary>
    public bool? IsLocalOnly { get; init; }
}

/// <summary>
/// Handles product updates including recalculation of pricing when the selling price changes.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateProductCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="pricingService">The pricing engine service.</param>
    /// <param name="dateTime">The date/time service.</param>
    public UpdateProductCommandHandler(
        IApplicationDbContext context,
        IPricingService pricingService,
        IDateTimeService dateTime)
    {
        _context = context;
        _pricingService = pricingService;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Applies updates to the product. If selling price changes, recalculates
    /// the full pricing breakdown using the pricing engine.
    /// </summary>
    /// <param name="request">The update product command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the update was successful.</returns>
    /// <exception cref="NotFoundException">Thrown when the product is not found.</exception>
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.ParentVendor)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        if (request.Title is not null) product.Title = request.Title;
        if (request.TitleAr is not null) product.TitleAr = request.TitleAr;
        if (request.Description is not null) product.Description = request.Description;
        if (request.DescriptionAr is not null) product.DescriptionAr = request.DescriptionAr;
        if (request.Images is not null) product.Images = request.Images;
        if (request.CostPrice.HasValue) product.CostPrice = request.CostPrice.Value;
        if (request.QuantityAvailable.HasValue) product.QuantityAvailable = request.QuantityAvailable.Value;
        if (request.WeightKg.HasValue) product.WeightKg = request.WeightKg.Value;
        if (request.LengthCm.HasValue) product.LengthCm = request.LengthCm.Value;
        if (request.WidthCm.HasValue) product.WidthCm = request.WidthCm.Value;
        if (request.HeightCm.HasValue) product.HeightCm = request.HeightCm.Value;
        if (request.IsLocalOnly.HasValue) product.IsLocalOnly = request.IsLocalOnly.Value;

        // Recalculate pricing if selling price changed
        if (request.SellingPrice.HasValue)
        {
            product.SellingPrice = request.SellingPrice.Value;

            // Determine uplift configuration if sub-vendor product
            string? parentUpliftType = null;
            decimal? parentUpliftValue = null;
            decimal? customUpliftOverride = null;

            if (product.SubVendorId.HasValue)
            {
                var subVendor = await _context.SubVendors
                    .FirstOrDefaultAsync(sv => sv.Id == product.SubVendorId.Value, cancellationToken);

                if (subVendor is not null)
                {
                    parentUpliftType = subVendor.UpliftType.ToString();
                    parentUpliftValue = subVendor.UpliftValue;
                    customUpliftOverride = subVendor.CustomUpliftOverride;
                }
            }

            var pricingInput = new PricingInput(
                VendorBasePrice: request.SellingPrice.Value,
                CategoryId: product.CategoryId.ToString(),
                IsVendorVatRegistered: product.ParentVendor!.IsVatRegistered,
                IsNonLegalizedVendor: product.ParentVendor.VendorType == VendorType.NonLegalized,
                ParentUpliftType: parentUpliftType,
                ParentUpliftValue: parentUpliftValue,
                CustomUpliftOverride: customUpliftOverride);

            var pricingResult = _pricingService.CalculatePrice(pricingInput);

            product.FinalPrice = pricingResult.FinalPrice;
            product.CommissionRate = pricingResult.CommissionRate;
            product.CommissionAmount = pricingResult.MarketplaceUplift;
            product.VatAmount = pricingResult.TotalVatAmount;
            product.ParentUpliftAmount = pricingResult.ParentUpliftAmount;
            product.WithholdingTaxAmount = pricingResult.CooperativeFee;
            product.VendorNetPayout = pricingResult.VendorNetPayout;
        }

        product.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Validates the <see cref="UpdateProductCommand"/>.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product updates.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(p => p.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(p => p.Title)
            .MaximumLength(300)
            .When(p => p.Title is not null)
            .WithMessage("Product title must not exceed 300 characters.");

        RuleFor(p => p.TitleAr)
            .MaximumLength(300)
            .When(p => p.TitleAr is not null)
            .WithMessage("Arabic product title must not exceed 300 characters.");

        RuleFor(p => p.CostPrice)
            .GreaterThan(0)
            .When(p => p.CostPrice.HasValue)
            .WithMessage("Cost price must be greater than zero.");

        RuleFor(p => p.SellingPrice)
            .GreaterThan(0)
            .When(p => p.SellingPrice.HasValue)
            .WithMessage("Selling price must be greater than zero.");

        RuleFor(p => p.QuantityAvailable)
            .GreaterThanOrEqualTo(0)
            .When(p => p.QuantityAvailable.HasValue)
            .WithMessage("Quantity available must be zero or greater.");

        RuleFor(p => p.WeightKg)
            .GreaterThan(0)
            .When(p => p.WeightKg.HasValue)
            .WithMessage("Weight must be greater than zero.");
    }
}
