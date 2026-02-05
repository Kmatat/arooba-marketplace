using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.Customers.Commands;

/// <summary>
/// Command to register a new customer on the Arooba Marketplace.
/// Creates both a User account and a Customer profile.
/// </summary>
public record CreateCustomerCommand : IRequest<Guid>
{
    /// <summary>Gets the customer's full name.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Gets the customer's Egyptian mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the customer's email address.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the customer's preferred language (ar or en).</summary>
    public string PreferredLanguage { get; init; } = "ar";
}

/// <summary>
/// Handles customer registration by creating a User and Customer record.
/// </summary>
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateCustomerCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public CreateCustomerCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Creates a new User and Customer, persists them, and returns the customer identifier.
    /// </summary>
    /// <param name="request">The create customer command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the newly created customer.</returns>
    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var now = _dateTime.UtcNow;

        var user = new User
        {
            Id = userId,
            FullName = request.FullName,
            MobileNumber = request.MobileNumber,
            Email = request.Email,
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = now
        };

        var customer = new Customer
        {
            Id = customerId,
            UserId = userId,
            FullName = request.FullName,
            MobileNumber = request.MobileNumber,
            Email = request.Email,
            PreferredLanguage = request.PreferredLanguage,
            CreatedAt = now
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);

        await _context.SaveChangesAsync(cancellationToken);

        return customerId;
    }
}

/// <summary>
/// Validates the <see cref="CreateCustomerCommand"/>.
/// </summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    /// <summary>
    /// Initializes validation rules for customer registration.
    /// </summary>
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(c => c.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+20(10|11|12|15)\d{8}$")
            .WithMessage("Mobile number must be a valid Egyptian mobile number (e.g., +201XXXXXXXXX).");

        RuleFor(c => c.Email)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("A valid email address is required.");

        RuleFor(c => c.PreferredLanguage)
            .Must(lang => lang is "ar" or "en")
            .WithMessage("Preferred language must be 'ar' or 'en'.");
    }
}
