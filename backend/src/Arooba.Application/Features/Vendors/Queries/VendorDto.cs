namespace Arooba.Application.Features.Vendors.Queries;

public record VendorDto
{
    public Guid Id { get; init; }
    public string BusinessNameAr { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int SubVendorCount { get; init; }
}
