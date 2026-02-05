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
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(p => p.VendorBasePrice)
            .GreaterThan(0).WithMessage("Vendor base price must be greater than zero.");

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(p => p.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(p => p.LengthCm)
            .GreaterThan(0).WithMessage("Length must be greater than zero.");

        RuleFor(p => p.WidthCm)
            .GreaterThan(0).WithMessage("Width must be greater than zero.");

        RuleFor(p => p.HeightCm)
            .GreaterThan(0).WithMessage("Height must be greater than zero.");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }
}
