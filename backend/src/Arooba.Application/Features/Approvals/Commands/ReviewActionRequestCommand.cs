using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.Approvals.Commands;

/// <summary>
/// Reviews (approves or rejects) a pending vendor action request.
/// </summary>
public record ReviewActionRequestCommand : IRequest<Unit>
{
    public int RequestId { get; init; }
    public ApprovalStatus Decision { get; init; }
    public string? ReviewNotes { get; init; }
}

public class ReviewActionRequestValidator : AbstractValidator<ReviewActionRequestCommand>
{
    public ReviewActionRequestValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Decision)
            .Must(d => d == ApprovalStatus.Approved || d == ApprovalStatus.Rejected)
            .WithMessage("Decision must be either Approved or Rejected.");
        RuleFor(x => x.ReviewNotes)
            .NotEmpty()
            .When(x => x.Decision == ApprovalStatus.Rejected)
            .WithMessage("Review notes are required when rejecting a request.");
    }
}
