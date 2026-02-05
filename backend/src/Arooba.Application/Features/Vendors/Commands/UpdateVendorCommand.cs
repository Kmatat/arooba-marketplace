using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Vendors.Commands;

/// <summary>
/// Command to update an existing vendor's details including status, commission rate,
/// and banking information.
/// </summary>
public record UpdateVendorCommand : IRequest<bool>
{
    /// <summary>Gets the vendor identifier to update.</summary>
    public Guid VendorId { get; init; }

    /// <summary>Gets the updated vendor status, if changing.</summary>
    public VendorStatus? Status { get; init; }

    /// <summary>Gets the updated commission rate (as a decimal, e.g., 0.15 for 15%).</summary>
    public decimal? CommissionRate { get; init; }

    /// <summary>Gets the updated bank name.</summary>
    public string? BankName { get; init; }

    /// <summary>Gets the updated bank account number.</summary>
    public string? BankAccountNumber { get; init; }

    /// <summary>Gets the updated business name in English.</summary>
    public string? BusinessName { get; init; }

    /// <summary>Gets the updated business name in Arabic.</summary>
    public string? BusinessNameAr { get; init; }

    /// <summary>Gets the updated VAT registration status.</summary>
    public bool? IsVatRegistered { get; init; }
}

/// <summary>
/// Handles updating vendor details. Only non-null fields in the command are applied.
/// </summary>
public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateVendorCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public UpdateVendorCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Applies the requested updates to the vendor entity and persists changes.
    /// </summary>
    /// <param name="request">The update vendor command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the update was successful.</returns>
    /// <exception cref="NotFoundException">Thrown when the vendor is not found.</exception>
    public async Task<bool> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.ParentVendors
            .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        if (vendor is null)
        {
            throw new NotFoundException(nameof(ParentVendor), request.VendorId);
        }

        if (request.Status.HasValue)
        {
            vendor.Status = request.Status.Value;
        }

        if (request.CommissionRate.HasValue)
        {
            vendor.CommissionRate = request.CommissionRate.Value;
        }

        if (request.BankName is not null)
        {
            vendor.BankName = request.BankName;
        }

        if (request.BankAccountNumber is not null)
        {
            vendor.BankAccountNumber = request.BankAccountNumber;
        }

        if (request.BusinessName is not null)
        {
            vendor.BusinessName = request.BusinessName;
        }

        if (request.BusinessNameAr is not null)
        {
            vendor.BusinessNameAr = request.BusinessNameAr;
        }

        if (request.IsVatRegistered.HasValue)
        {
            vendor.IsVatRegistered = request.IsVatRegistered.Value;
        }

        vendor.UpdatedAt = _dateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Validates the <see cref="UpdateVendorCommand"/>.
/// </summary>
public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    /// <summary>
    /// Initializes validation rules for vendor updates.
    /// </summary>
    public UpdateVendorCommandValidator()
    {
        RuleFor(v => v.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required.");

        RuleFor(v => v.CommissionRate)
            .InclusiveBetween(0m, 1m)
            .When(v => v.CommissionRate.HasValue)
            .WithMessage("Commission rate must be between 0 and 1 (0% to 100%).");

        RuleFor(v => v.BankName)
            .MaximumLength(100)
            .When(v => v.BankName is not null)
            .WithMessage("Bank name must not exceed 100 characters.");

        RuleFor(v => v.BankAccountNumber)
            .MaximumLength(50)
            .When(v => v.BankAccountNumber is not null)
            .WithMessage("Bank account number must not exceed 50 characters.");

        RuleFor(v => v.BusinessName)
            .MaximumLength(200)
            .When(v => v.BusinessName is not null)
            .WithMessage("Business name must not exceed 200 characters.");

        RuleFor(v => v.BusinessNameAr)
            .MaximumLength(200)
            .When(v => v.BusinessNameAr is not null)
            .WithMessage("Arabic business name must not exceed 200 characters.");
    }
}
