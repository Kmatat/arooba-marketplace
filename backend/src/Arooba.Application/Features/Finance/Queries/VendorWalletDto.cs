namespace Arooba.Application.Features.Finance.Queries;

public record VendorWalletDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
