using Arooba.Domain.Enums;
using FluentValidation;

namespace Arooba.Application.Features.Products.Commands;

/// <summary>
/// Validates the <see cref="CreateProductCommand"/> ensuring all required fields
/// meet business rules for product creation on the Arooba Marketplace.
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(p => p.ParentVendorId)
            .NotEmpty().WithMessage("Parent vendor ID is required.");

        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Product title is required.")
            .MaximumLength(500).WithMessage("Product title must not exceed 500 characters.");

        RuleFor(p => p.TitleAr)
            .MaximumLength(500).WithMessage("Arabic title must not exceed 500 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(p => p.DescriptionAr)
            .MaximumLength(4000).WithMessage("Arabic description must not exceed 4000 characters.");

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(p => p.SellingPrice)
            .GreaterThan(0).WithMessage("Selling price must be greater than zero.");

        RuleFor(p => p.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative.");

        RuleFor(p => p.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(p => p.DimensionL)
            .GreaterThan(0).WithMessage("Length must be greater than zero.");

        RuleFor(p => p.DimensionW)
            .GreaterThan(0).WithMessage("Width must be greater than zero.");

        RuleFor(p => p.DimensionH)
            .GreaterThan(0).WithMessage("Height must be greater than zero.");

        RuleFor(p => p.QuantityAvailable)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.")
            .When(p => p.StockMode == StockMode.ReadyStock);

        RuleFor(p => p.LeadTimeDays)
            .GreaterThan(0).WithMessage("Lead time must be greater than zero.")
            .When(p => p.StockMode == StockMode.MadeToOrder);
    }
}
