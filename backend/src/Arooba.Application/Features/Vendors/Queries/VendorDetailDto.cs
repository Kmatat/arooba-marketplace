namespace Arooba.Application.Features.Vendors.Queries;

public record VendorDetailDto
{
    public Guid Id { get; init; }
    public string BusinessNameAr { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
}
