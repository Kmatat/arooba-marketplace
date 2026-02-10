using Arooba.Application.Common.Models;
using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.Approvals.Queries;

/// <summary>
/// Retrieves vendor action requests with filtering and pagination.
/// </summary>
public record GetActionRequestsQuery : IRequest<PaginatedList<ActionRequestDto>>
{
    public ApprovalStatus? Status { get; init; }
    public VendorActionType? ActionType { get; init; }
    public int? VendorId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record ActionRequestDto
{
    public int Id { get; init; }
    public int VendorId { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public VendorActionType ActionType { get; init; }
    public ApprovalStatus Status { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public int? EntityId { get; init; }
    public string? CurrentValues { get; init; }
    public string? ProposedValues { get; init; }
    public string? Justification { get; init; }
    public string? ReviewedBy { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ReviewNotes { get; init; }
    public int Priority { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}
