using FluentValidation;

namespace Arooba.Application.Features.Vendors.Commands;

/// <summary>
/// Validates the <see cref="CreateVendorCommand"/> ensuring all required fields
/// meet business rules for vendor registration on the Arooba Marketplace.
/// </summary>
public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(v => v.BusinessNameAr)
            .NotEmpty().WithMessage("Arabic business name is required.")
            .MaximumLength(200).WithMessage("Arabic business name must not exceed 200 characters.");

        RuleFor(v => v.BusinessNameEn)
            .NotEmpty().WithMessage("English business name is required.")
            .MaximumLength(200).WithMessage("English business name must not exceed 200 characters.");

        RuleFor(v => v.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+20[1][0-9]{9}$").WithMessage("Phone number must be a valid Egyptian number in +201XXXXXXXXX format.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(v => v.NationalId)
            .NotEmpty().WithMessage("National ID is required.")
            .Matches(@"^\d{14}$").WithMessage("National ID must be exactly 14 digits.");

        RuleFor(v => v.VendorType)
            .IsInEnum().WithMessage("A valid vendor type is required.");

        RuleFor(v => v.City)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(v => v.GovernorateId)
            .NotEmpty().WithMessage("Governorate is required.");
    }
}
