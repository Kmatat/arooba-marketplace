using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Common;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Stub implementation of <see cref="IIdentityService"/>.
/// This is a placeholder for the real identity provider integration (Firebase, Twilio OTP, etc.).
/// All methods return minimal valid responses to unblock development of dependent features.
/// </summary>
public class IdentityService : IIdentityService
{
    /// <inheritdoc />
    public Task<string?> GetUserNameAsync(string userId)
    {
        // Placeholder: in production, look up the user in the identity store
        return Task.FromResult<string?>($"User-{userId}");
    }

    /// <inheritdoc />
    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        // Placeholder: in production, check role claims from the identity store
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        // Placeholder: in production, evaluate the authorization policy
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<Result<string>> CreateUserAsync(string userName, string password)
    {
        // Placeholder: in production, create the user in the identity store
        // Return a string representation of a generated ID
        var newUserId = Guid.NewGuid().ToString();
        return Task.FromResult(Result.Success(newUserId));
    }

    /// <inheritdoc />
    public Task<Result> DeleteUserAsync(string userId)
    {
        // Placeholder: in production, delete the user from the identity store
        return Task.FromResult(Result.Success());
    }
}
