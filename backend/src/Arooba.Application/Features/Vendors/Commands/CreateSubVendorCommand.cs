using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Vendors.Commands.CreateSubVendor;

/// <summary>
/// Command to register a new sub-vendor under a parent vendor.
/// </summary>
public record CreateSubVendorCommand : IRequest<int>
{
    public string BusinessName { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public VendorType VendorType { get; init; }
    public int ParentVendorId { get; init; }
}

/// <summary>
/// Handles the creation of a new sub-vendor under a parent vendor.
/// </summary>
public class CreateSubVendorCommandHandler : IRequestHandler<CreateSubVendorCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    public CreateSubVendorCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<int> Handle(CreateSubVendorCommand request, CancellationToken cancellationToken)
    {
        var parentVendor = await _context.ParentVendors
            .FirstOrDefaultAsync(v => v.Id == request.ParentVendorId, cancellationToken);

        if (parentVendor is null)
        {
            throw new NotFoundException(nameof(ParentVendor), request.ParentVendorId);
        }

        var subVendor = new SubVendor
        {
            BusinessName = request.BusinessName,
            BusinessNameEn = request.BusinessNameEn,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            NationalId = request.NationalId,
            VendorType = request.VendorType,
            ParentVendorId = request.ParentVendorId,
            Status = VendorStatus.Pending
        };

        _context.SubVendors.Add(subVendor);
        await _context.SaveChangesAsync(cancellationToken);

        return subVendor.Id;
    }
}

/// <summary>
/// Validates the CreateSubVendorCommand.
/// </summary>
public class CreateSubVendorCommandValidator : AbstractValidator<CreateSubVendorCommand>
{
    public CreateSubVendorCommandValidator()
    {
        RuleFor(c => c.BusinessName)
            .NotEmpty().WithMessage("Arabic business name is required.")
            .MaximumLength(200);

        RuleFor(c => c.BusinessNameEn)
            .NotEmpty().WithMessage("English business name is required.")
            .MaximumLength(200);

        RuleFor(c => c.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+20[1][0-9]{9}$").WithMessage("Phone number must be a valid Egyptian mobile number.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(c => c.NationalId)
            .NotEmpty().WithMessage("National ID is required.")
            .Matches(@"^\d{14}$").WithMessage("National ID must be exactly 14 digits.");

        RuleFor(c => c.ParentVendorId)
            .NotEmpty().WithMessage("Parent vendor ID is required.");
    }
}
