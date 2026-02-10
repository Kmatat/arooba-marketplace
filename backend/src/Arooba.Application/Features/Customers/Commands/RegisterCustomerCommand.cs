using MediatR;

namespace Arooba.Application.Features.Customers.Commands;

/// <summary>Command to register a new customer.</summary>
public record RegisterCustomerCommand : IRequest<int>
{
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
