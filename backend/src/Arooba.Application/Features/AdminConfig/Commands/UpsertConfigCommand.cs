using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.AdminConfig.Commands;

/// <summary>
/// Creates or updates a platform configuration entry.
/// </summary>
public record UpsertConfigCommand : IRequest<int>
{
    public int? Id { get; init; }
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
    public bool IsActive { get; init; } = true;
    public bool RequiresApproval { get; init; }
    public int SortOrder { get; init; }
}

public class UpsertConfigCommandValidator : AbstractValidator<UpsertConfigCommand>
{
    public UpsertConfigCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LabelAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValueType).NotEmpty()
            .Must(v => new[] { "number", "percentage", "boolean", "json", "string" }.Contains(v))
            .WithMessage("ValueType must be one of: number, percentage, boolean, json, string");
    }
}
