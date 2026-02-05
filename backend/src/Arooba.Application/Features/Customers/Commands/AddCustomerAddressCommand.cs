using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Commands;

/// <summary>
/// Command to add a new delivery address to a customer's profile.
/// </summary>
public record AddCustomerAddressCommand : IRequest<Guid>
{
    /// <summary>Gets the customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Gets the address label (e.g., "Home", "Office").</summary>
    public string Label { get; init; } = default!;

    /// <summary>Gets the street address.</summary>
    public string StreetAddress { get; init; } = default!;

    /// <summary>Gets the apartment or building number.</summary>
    public string? ApartmentNumber { get; init; }

    /// <summary>Gets the floor number.</summary>
    public string? Floor { get; init; }

    /// <summary>Gets the district or neighbourhood.</summary>
    public string District { get; init; } = default!;

    /// <summary>Gets the city name.</summary>
    public string City { get; init; } = default!;

    /// <summary>Gets the Egyptian governorate.</summary>
    public string Governorate { get; init; } = default!;

    /// <summary>Gets the postal code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Gets the shipping zone identifier for this address.</summary>
    public Guid ShippingZoneId { get; init; }

    /// <summary>Gets whether this should be set as the default delivery address.</summary>
    public bool IsDefault { get; init; }

    /// <summary>Gets additional delivery notes.</summary>
    public string? DeliveryNotes { get; init; }
}

/// <summary>
/// Handles adding a new delivery address to a customer's profile.
/// </summary>
public class AddCustomerAddressCommandHandler : IRequestHandler<AddCustomerAddressCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="AddCustomerAddressCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public AddCustomerAddressCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Validates the customer exists, optionally clears other default flags,
    /// creates the address, and returns the new address identifier.
    /// </summary>
    /// <param name="request">The add address command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the newly created address.</returns>
    /// <exception cref="NotFoundException">Thrown when the customer is not found.</exception>
    public async Task<Guid> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        // If this address is set as default, clear the default flag on existing addresses
        if (request.IsDefault)
        {
            var existingAddresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == request.CustomerId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingAddresses)
            {
                addr.IsDefault = false;
            }
        }

        var addressId = Guid.NewGuid();

        var address = new CustomerAddress
        {
            Id = addressId,
            CustomerId = request.CustomerId,
            Label = request.Label,
            StreetAddress = request.StreetAddress,
            ApartmentNumber = request.ApartmentNumber,
            Floor = request.Floor,
            District = request.District,
            City = request.City,
            Governorate = request.Governorate,
            PostalCode = request.PostalCode,
            ShippingZoneId = request.ShippingZoneId,
            IsDefault = request.IsDefault,
            DeliveryNotes = request.DeliveryNotes,
            CreatedAt = _dateTime.UtcNow
        };

        _context.CustomerAddresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        return addressId;
    }
}

/// <summary>
/// Validates the <see cref="AddCustomerAddressCommand"/>.
/// </summary>
public class AddCustomerAddressCommandValidator : AbstractValidator<AddCustomerAddressCommand>
{
    /// <summary>
    /// Initializes validation rules for adding a customer address.
    /// </summary>
    public AddCustomerAddressCommandValidator()
    {
        RuleFor(a => a.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(a => a.Label)
            .NotEmpty().WithMessage("Address label is required.")
            .MaximumLength(50).WithMessage("Address label must not exceed 50 characters.");

        RuleFor(a => a.StreetAddress)
            .NotEmpty().WithMessage("Street address is required.")
            .MaximumLength(300).WithMessage("Street address must not exceed 300 characters.");

        RuleFor(a => a.District)
            .NotEmpty().WithMessage("District is required.")
            .MaximumLength(100).WithMessage("District must not exceed 100 characters.");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(a => a.Governorate)
            .NotEmpty().WithMessage("Governorate is required.")
            .MaximumLength(100).WithMessage("Governorate must not exceed 100 characters.");

        RuleFor(a => a.ShippingZoneId)
            .NotEmpty().WithMessage("Shipping zone ID is required.");
    }
}
