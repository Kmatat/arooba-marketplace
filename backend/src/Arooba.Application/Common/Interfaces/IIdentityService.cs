using Arooba.Domain.Common;

namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Abstraction over identity and authentication operations.
/// Decouples the Application layer from specific identity providers (JWT, Firebase, etc.).
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Retrieves the display name of a user by their identifier.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>The user's full name, or <c>null</c> if not found.</returns>
    Task<string?> GetUserNameAsync(string userId);

    /// <summary>
    /// Checks whether a user is assigned to the specified role.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="role">The role name to check.</param>
    /// <returns><c>true</c> if the user has the specified role.</returns>
    Task<bool> IsInRoleAsync(string userId, string role);

    /// <summary>
    /// Checks whether a user is authorized by a specific policy.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="policyName">The authorization policy name.</param>
    /// <returns><c>true</c> if the user satisfies the policy.</returns>
    Task<bool> AuthorizeAsync(string userId, string policyName);

    /// <summary>
    /// Creates a new user identity with the specified credentials.
    /// </summary>
    /// <param name="userName">The user's display name.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A result containing the new user's identifier on success.</returns>
    Task<Result<string>> CreateUserAsync(string userName, string password);

    /// <summary>
    /// Deletes a user identity.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteUserAsync(string userId);
}
