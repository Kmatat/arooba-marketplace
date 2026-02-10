namespace Arooba.Application.Features.Vendors.Queries;

public record SubVendorDto_v2
{
    public int Id { get; init; }
    public string BusinessName { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public int ParentVendorId { get; init; }
}
