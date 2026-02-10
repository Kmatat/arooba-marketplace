using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.AdminConfig.Queries;

/// <summary>
/// Retrieves platform configurations, optionally filtered by category.
/// </summary>
public record GetConfigsQuery : IRequest<IReadOnlyList<ConfigDto>>
{
    public ConfigCategory? Category { get; init; }
}

public record ConfigDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public ConfigCategory Category { get; init; }
    public string Label { get; init; } = string.Empty;
    public string LabelAr { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? DescriptionAr { get; init; }
    public string ValueType { get; init; } = "string";
    public string? DefaultValue { get; init; }
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public bool IsActive { get; init; }
    public bool RequiresApproval { get; init; }
    public int SortOrder { get; init; }
    public string? LastModifiedBy { get; init; }
    public DateTime UpdatedAt { get; init; }
}
