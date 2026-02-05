using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Commands;

/// <summary>
/// Command to update an existing customer's profile information.
/// </summary>
public record UpdateCustomerCommand : IRequest<bool>
{
    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the updated full name.</summary>
    public string? FullName { get; init; }

    /// <summary>Gets the updated email address.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the updated preferred language.</summary>
    public string? PreferredLanguage { get; init; }
}

/// <summary>
/// Handles updating customer profile details.
/// </summary>
public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateCustomerCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public UpdateCustomerCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Applies the requested updates to the customer profile and persists changes.
    /// </summary>
    /// <param name="request">The update customer command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the update was successful.</returns>
    /// <exception cref="NotFoundException">Thrown when the customer is not found.</exception>
    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        if (request.FullName is not null)
        {
            customer.FullName = request.FullName;
        }

        if (request.Email is not null)
        {
            customer.Email = request.Email;
        }

        if (request.PreferredLanguage is not null)
        {
            customer.PreferredLanguage = request.PreferredLanguage;
        }

        customer.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Validates the <see cref="UpdateCustomerCommand"/>.
/// </summary>
public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    /// <summary>
    /// Initializes validation rules for customer updates.
    /// </summary>
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(c => c.FullName)
            .MaximumLength(200)
            .When(c => c.FullName is not null)
            .WithMessage("Full name must not exceed 200 characters.");

        RuleFor(c => c.Email)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("A valid email address is required.");

        RuleFor(c => c.PreferredLanguage)
            .Must(lang => lang is "ar" or "en")
            .When(c => c.PreferredLanguage is not null)
            .WithMessage("Preferred language must be 'ar' or 'en'.");
    }
}
