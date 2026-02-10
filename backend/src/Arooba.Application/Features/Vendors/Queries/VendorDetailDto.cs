namespace Arooba.Application.Features.Vendors.Queries;

public record VendorDetailDto_v2
{
    public int Id { get; init; }
    public string BusinessName { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
}
