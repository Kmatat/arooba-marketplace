namespace Arooba.Domain.Common;

/// <summary>
/// Extends <see cref="BaseEntity"/> with user-level audit tracking.
/// Records which user created and last modified the entity.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}
