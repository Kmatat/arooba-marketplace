using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Commands;

/// <summary>
/// Command to update a customer's account status (activate, deactivate, block, unblock).
/// </summary>
public record UpdateCustomerStatusCommand : IRequest<bool>
{
    /// <summary>Gets the customer identifier.</summary>
    public int CustomerId { get; init; }

    /// <summary>Gets whether the customer account should be active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets whether the customer account should be blocked.</summary>
    public bool IsBlocked { get; init; }

    /// <summary>Gets the reason for the status change.</summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Handles customer status updates.
/// </summary>
public class UpdateCustomerStatusCommandHandler : IRequestHandler<UpdateCustomerStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    public UpdateCustomerStatusCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<bool> Handle(UpdateCustomerStatusCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        customer.IsActive = request.IsActive;
        customer.IsBlocked = request.IsBlocked;
        customer.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>
/// Validates the <see cref="UpdateCustomerStatusCommand"/>.
/// </summary>
public class UpdateCustomerStatusCommandValidator : AbstractValidator<UpdateCustomerStatusCommand>
{
    public UpdateCustomerStatusCommandValidator()
    {
        RuleFor(c => c.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(c => c.Reason)
            .MaximumLength(500)
            .When(c => c.Reason is not null)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
