using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve a single customer by ID with addresses and order history.
/// </summary>
public record GetCustomerByIdQuery : IRequest<CustomerDetailDto>
{
    /// <summary>Gets the customer identifier.</summary>
    public int CustomerId { get; init; }
}

/// <summary>
/// DTO for a customer delivery address.
/// </summary>
public record CustomerAddressDto
{
    /// <summary>Gets the address identifier.</summary>
    public int Id { get; init; }

    /// <summary>Gets the address label.</summary>
    public string Label { get; init; } = default!;

    /// <summary>Gets the full address.</summary>
    public string FullAddress { get; init; } = default!;

    /// <summary>Gets the city.</summary>
    public string City { get; init; } = default!;

    /// <summary>Gets the shipping zone identifier.</summary>
    public string ZoneId { get; init; } = default!;

    /// <summary>Gets whether this is the default address.</summary>
    public bool IsDefault { get; init; }
}

/// <summary>
/// Summary of a recent customer order.
/// </summary>
public record CustomerOrderSummaryDto
{
    /// <summary>Gets the order identifier.</summary>
    public int Id { get; init; }

    /// <summary>Gets the order number.</summary>
    public string OrderNumber { get; init; } = default!;

    /// <summary>Gets the order status.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Gets the total amount in EGP.</summary>
    public decimal TotalAmount { get; init; }

    /// <summary>Gets the number of items.</summary>
    public int ItemCount { get; init; }

    /// <summary>Gets the order date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Detailed DTO for a customer with addresses and order history.
/// </summary>
public record CustomerDetailDto
{
    /// <summary>Gets the customer identifier.</summary>
    public int Id { get; init; }

    /// <summary>Gets the associated user identifier.</summary>
    public int UserId { get; init; }

    /// <summary>Gets the full name.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Gets the mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the email address.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the preferred language.</summary>
    public string PreferredLanguage { get; init; } = default!;

    /// <summary>Gets the list of delivery addresses.</summary>
    public List<CustomerAddressDto> Addresses { get; init; } = new();

    /// <summary>Gets the recent order history.</summary>
    public List<CustomerOrderSummaryDto> RecentOrders { get; init; } = new();

    /// <summary>Gets the total number of orders.</summary>
    public int TotalOrders { get; init; }

    /// <summary>Gets the total amount spent in EGP.</summary>
    public decimal TotalSpent { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the last update date.</summary>
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of a single customer with addresses and order history.
/// </summary>
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetCustomerByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves the customer, their addresses, and recent order history.
    /// </summary>
    /// <param name="request">The query containing the customer ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A detailed customer DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the customer is not found.</exception>
    public async Task<CustomerDetailDto> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        var addresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == request.CustomerId)
            .AsNoTracking()
            .Select(a => new CustomerAddressDto
            {       
                Id = a.Id,
                Label = a.Label,
                FullAddress = a.FullAddress,
                City = a.City,
                ZoneId = a.ZoneId,
                IsDefault = a.IsDefault
            })
            .ToListAsync(cancellationToken);

        var recentOrders = await _context.Orders
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .AsNoTracking()
            .Select(o => new CustomerOrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ItemCount = o.Items.Sum(oi => oi.Quantity),
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var orderAggregates = await _context.Orders
            .Where(o => o.CustomerId == request.CustomerId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new CustomerDetailDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.FullName,
            MobileNumber = customer.MobileNumber,
            Email = customer.Email,
            PreferredLanguage = customer.PreferredLanguage,
            Addresses = addresses,
            RecentOrders = recentOrders,
            TotalOrders = orderAggregates?.TotalOrders ?? 0,
            TotalSpent = orderAggregates?.TotalSpent ?? 0m,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
