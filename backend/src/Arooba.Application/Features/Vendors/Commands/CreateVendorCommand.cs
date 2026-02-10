using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.Vendors.Commands;

/// <summary>
/// Command to register a new parent vendor on the marketplace.
/// </summary>
public record CreateVendorCommand : IRequest<int>
{
    public string BusinessName { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public VendorType VendorType { get; init; }
    public string City { get; init; } = string.Empty;
    public string GovernorateId { get; init; } = string.Empty;
}
