using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Auth.Commands;

/// <summary>
/// Command to register a new user account on the Arooba Marketplace.
/// Supports registration for both customers and vendors.
/// </summary>
public record RegisterCommand : IRequest<Guid>
{
    /// <summary>Gets the user's full name.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Gets the user's Egyptian mobile number (primary identifier).</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the user's email address.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the role to register as.</summary>
    public UserRole Role { get; init; }

    /// <summary>Gets the preferred language (ar or en). Defaults to Arabic.</summary>
    public string PreferredLanguage { get; init; } = "ar";
}

/// <summary>
/// Handles new user registration ensuring mobile number uniqueness
/// and creating the appropriate user record.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="identityService">The identity service for user creation.</param>
    /// <param name="dateTime">The date/time service.</param>
    public RegisterCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IDateTimeService dateTime)
    {
        _context = context;
        _identityService = identityService;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Registers a new user account by:
    /// 1. Checking mobile number uniqueness
    /// 2. Creating the user in the identity store
    /// 3. Creating the User entity in the database
    /// 4. Creating associated role-specific records (Customer, etc.)
    /// </summary>
    /// <param name="request">The register command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the newly created user.</returns>
    /// <exception cref="BadRequestException">Thrown when the mobile number is already registered.</exception>
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if mobile number already exists
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);

        if (existingUser is not null)
        {
            throw new BadRequestException(
                "A user with this mobile number is already registered.");
        }

        // Create user in identity store
        var identityResult = await _identityService.CreateUserAsync(
            request.FullName,
            request.MobileNumber);

        if (!identityResult.Succeeded)
        {
            throw new BadRequestException("Failed to create user account. Please try again.");
        }

        var userId = Guid.NewGuid();
        var now = _dateTime.UtcNow;

        var user = new User
        {
            Id = userId,
            FullName = request.FullName,
            MobileNumber = request.MobileNumber,
            Email = request.Email,
            Role = request.Role,
            IsActive = true,
            CreatedAt = now
        };

        _context.Users.Add(user);

        // Create role-specific records
        if (request.Role == UserRole.Customer)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = request.FullName,
                MobileNumber = request.MobileNumber,
                Email = request.Email,
                PreferredLanguage = request.PreferredLanguage,
                CreatedAt = now
            };

            _context.Customers.Add(customer);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return userId;
    }
}

/// <summary>
/// Validates the <see cref="RegisterCommand"/>.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Initializes validation rules for user registration.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(r => r.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(r => r.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+20(10|11|12|15)\d{8}$")
            .WithMessage("Mobile number must be a valid Egyptian mobile number (e.g., +201XXXXXXXXX).");

        RuleFor(r => r.Email)
            .EmailAddress()
            .When(r => !string.IsNullOrWhiteSpace(r.Email))
            .WithMessage("A valid email address is required.");

        RuleFor(r => r.Role)
            .IsInEnum().WithMessage("A valid user role is required.")
            .Must(role => role == UserRole.Customer || role == UserRole.ParentVendor)
            .WithMessage("Registration is only available for Customer and ParentVendor roles.");

        RuleFor(r => r.PreferredLanguage)
            .Must(lang => lang is "ar" or "en")
            .WithMessage("Preferred language must be 'ar' or 'en'.");
    }
}
