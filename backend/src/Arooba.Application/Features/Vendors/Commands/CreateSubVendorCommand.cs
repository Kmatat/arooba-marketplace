using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.Vendors.Commands;

/// <summary>
/// Command to register a new sub-vendor under a parent vendor.
/// </summary>
public record CreateSubVendorCommand : IRequest<Guid>
{
    public string BusinessNameAr { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public VendorType VendorType { get; init; }
    public Guid ParentVendorId { get; init; }
}
