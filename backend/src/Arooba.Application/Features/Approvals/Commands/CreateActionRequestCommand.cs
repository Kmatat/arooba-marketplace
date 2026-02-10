using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.Approvals.Commands;

/// <summary>
/// Creates a new vendor action request requiring admin approval.
/// </summary>
public record CreateActionRequestCommand : IRequest<int>
{
    public int VendorId { get; init; }
    public VendorActionType ActionType { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public int? EntityId { get; init; }
    public string? CurrentValues { get; init; }
    public string? ProposedValues { get; init; }
    public string? Justification { get; init; }
    public int Priority { get; init; } = 2;
}

public class CreateActionRequestValidator : AbstractValidator<CreateActionRequestCommand>
{
    public CreateActionRequestValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Priority).InclusiveBetween(1, 4);
    }
}
