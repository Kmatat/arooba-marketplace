namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's identity information.
/// Used by audit interceptors and authorization checks throughout the application.
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
    string? UserRole { get; }

    /// <summary>
    /// Gets a value indicating whether a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
