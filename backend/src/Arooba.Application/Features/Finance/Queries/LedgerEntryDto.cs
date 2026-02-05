namespace Arooba.Application.Features.Finance.Queries;

public record LedgerEntryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
