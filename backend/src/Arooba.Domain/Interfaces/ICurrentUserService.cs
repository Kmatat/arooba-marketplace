using Arooba.Domain.Enums;

namespace Arooba.Domain.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's identity and role.
/// Implemented in the Infrastructure or API layer using HTTP context or JWT claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user, or <c>null</c> if unauthenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the role of the currently authenticated user, or <c>null</c> if unauthenticated.
    /// </summary>
    UserRole? UserRole { get; }
}
