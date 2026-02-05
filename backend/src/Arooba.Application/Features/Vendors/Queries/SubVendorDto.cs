namespace Arooba.Application.Features.Vendors.Queries;

public record SubVendorDto
{
    public Guid Id { get; init; }
    public string BusinessNameAr { get; init; } = string.Empty;
    public string BusinessNameEn { get; init; } = string.Empty;
    public Guid ParentVendorId { get; init; }
}
