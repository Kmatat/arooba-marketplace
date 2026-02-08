namespace Arooba.Domain.Enums;

/// <summary>
/// Types of auditable actions in the system.
/// </summary>
public enum AuditAction
{
    Create,
    Update,
    Delete,
    StatusChange,
    Approve,
    Reject,
    ConfigChange,
    Login,
    Export,
    BulkOperation
}
