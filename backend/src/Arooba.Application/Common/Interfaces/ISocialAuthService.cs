using Arooba.Domain.Enums;

namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Abstraction over social authentication token validation.
/// Validates ID tokens from Google, Apple, and Facebook and extracts user profile information.
/// </summary>
public interface ISocialAuthService
{
    /// <summary>
    /// Validates a social login token and extracts the user's profile information.
    /// </summary>
    /// <param name="provider">The social authentication provider (Google, Apple, Facebook).</param>
    /// <param name="idToken">The ID token or access token received from the social provider's SDK.</param>
    /// <returns>The validated social user profile, or an error result.</returns>
    Task<SocialAuthResult> ValidateTokenAsync(SocialProvider provider, string idToken);
}

/// <summary>
/// Represents the result of validating a social authentication token.
/// </summary>
public record SocialAuthResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    /// <summary>The unique identifier from the social provider.</summary>
    public string? ProviderId { get; init; }

    /// <summary>The user's email address from the social provider.</summary>
    public string? Email { get; init; }

    /// <summary>The user's full name from the social provider.</summary>
    public string? FullName { get; init; }

    /// <summary>The user's profile picture URL from the social provider.</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>The social provider that validated the token.</summary>
    public SocialProvider Provider { get; init; }

    public static SocialAuthResult Succeeded(
        SocialProvider provider,
        string providerId,
        string? email,
        string? fullName,
        string? avatarUrl) =>
        new()
        {
            Success = true,
            Provider = provider,
            ProviderId = providerId,
            Email = email,
            FullName = fullName,
            AvatarUrl = avatarUrl
        };

    public static SocialAuthResult Failed(string error) =>
        new() { Success = false, ErrorMessage = error };
}
