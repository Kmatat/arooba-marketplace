using Arooba.Application.Common.Models;
using Arooba.Domain.Enums;
using MediatR;

namespace Arooba.Application.Features.AuditLogs.Queries;

/// <summary>
/// Retrieves audit log entries with filtering and pagination.
/// </summary>
public record GetAuditLogsQuery : IRequest<PaginatedList<AuditLogDto>>
{
    public string? UserId { get; init; }
    public AuditAction? Action { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record AuditLogDto
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserRole { get; init; } = string.Empty;
    public AuditAction Action { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? IpAddress { get; init; }
    public int? VendorActionRequestId { get; init; }
    public DateTime CreatedAt { get; init; }
}
