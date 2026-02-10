namespace Arooba.Application.Features.Vendors.Queries;

public record VendorDto_v2
{
    public int Id { get; init; }
    public string BusinessName { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int SubVendorCount { get; init; }
}
