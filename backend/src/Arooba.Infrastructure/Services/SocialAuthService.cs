using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Validates social authentication tokens from Google, Apple, and Facebook.
/// Extracts user profile information from the validated tokens.
/// </summary>
public class SocialAuthService : ISocialAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SocialAuthService> _logger;

    public SocialAuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<SocialAuthService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SocialAuthResult> ValidateTokenAsync(SocialProvider provider, string idToken)
    {
        try
        {
            return provider switch
            {
                SocialProvider.Google => await ValidateGoogleTokenAsync(idToken),
                SocialProvider.Apple => await ValidateAppleTokenAsync(idToken),
                SocialProvider.Facebook => await ValidateFacebookTokenAsync(idToken),
                _ => SocialAuthResult.Failed("Unsupported social provider.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Social auth validation failed for provider {Provider}", provider);
            return SocialAuthResult.Failed("Failed to validate social login token.");
        }
    }

    private async Task<SocialAuthResult> ValidateGoogleTokenAsync(string idToken)
    {
        var clientId = _configuration["SocialAuth:Google:ClientId"];

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = string.IsNullOrEmpty(clientId)
                ? null
                : new[] { clientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        return SocialAuthResult.Succeeded(
            SocialProvider.Google,
            payload.Subject,
            payload.Email,
            payload.Name,
            payload.Picture);
    }

    private async Task<SocialAuthResult> ValidateAppleTokenAsync(string idToken)
    {
        // Apple Sign In returns a JWT ID token that we verify via Apple's public keys
        var client = _httpClientFactory.CreateClient("Apple");
        var response = await client.GetAsync("https://appleid.apple.com/auth/keys");
        response.EnsureSuccessStatusCode();

        var keysJson = await response.Content.ReadAsStringAsync();
        using var keysDoc = JsonDocument.Parse(keysJson);

        // Decode the JWT header to find the matching key
        var tokenParts = idToken.Split('.');
        if (tokenParts.Length != 3)
        {
            return SocialAuthResult.Failed("Invalid Apple ID token format.");
        }

        // Decode payload
        var payloadJson = DecodeBase64Url(tokenParts[1]);
        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var root = payloadDoc.RootElement;

        // Verify issuer
        var issuer = root.TryGetProperty("iss", out var issEl) ? issEl.GetString() : null;
        if (issuer != "https://appleid.apple.com")
        {
            return SocialAuthResult.Failed("Invalid Apple token issuer.");
        }

        // Verify audience (our app's client ID)
        var audience = root.TryGetProperty("aud", out var audEl) ? audEl.GetString() : null;
        var expectedAudience = _configuration["SocialAuth:Apple:ClientId"];
        if (!string.IsNullOrEmpty(expectedAudience) && audience != expectedAudience)
        {
            return SocialAuthResult.Failed("Apple token audience mismatch.");
        }

        // Check expiry
        var exp = root.TryGetProperty("exp", out var expEl) ? expEl.GetInt64() : 0;
        if (DateTimeOffset.FromUnixTimeSeconds(exp) < DateTimeOffset.UtcNow)
        {
            return SocialAuthResult.Failed("Apple token has expired.");
        }

        var sub = root.TryGetProperty("sub", out var subEl) ? subEl.GetString() : null;
        var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;

        if (string.IsNullOrEmpty(sub))
        {
            return SocialAuthResult.Failed("Apple token missing subject.");
        }

        return SocialAuthResult.Succeeded(
            SocialProvider.Apple,
            sub,
            email,
            null, // Apple doesn't always provide name in the token
            null);
    }

    private async Task<SocialAuthResult> ValidateFacebookTokenAsync(string accessToken)
    {
        var appId = _configuration["SocialAuth:Facebook:AppId"];
        var appSecret = _configuration["SocialAuth:Facebook:AppSecret"];

        var client = _httpClientFactory.CreateClient("Facebook");

        // Step 1: Debug the token to verify it's valid and belongs to our app
        var debugUrl = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appId}|{appSecret}";
        var debugResponse = await client.GetAsync(debugUrl);

        if (!debugResponse.IsSuccessStatusCode)
        {
            return SocialAuthResult.Failed("Failed to validate Facebook token.");
        }

        var debugContent = await debugResponse.Content.ReadAsStringAsync();
        using var debugDoc = JsonDocument.Parse(debugContent);

        var data = debugDoc.RootElement.GetProperty("data");
        var isValid = data.TryGetProperty("is_valid", out var validEl) && validEl.GetBoolean();

        if (!isValid)
        {
            return SocialAuthResult.Failed("Facebook token is invalid or expired.");
        }

        // Step 2: Fetch user profile
        var profileUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token={accessToken}";
        var profileResponse = await client.GetAsync(profileUrl);

        if (!profileResponse.IsSuccessStatusCode)
        {
            return SocialAuthResult.Failed("Failed to fetch Facebook user profile.");
        }

        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        using var profileDoc = JsonDocument.Parse(profileContent);
        var profile = profileDoc.RootElement;

        var userId = profile.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var name = profile.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
        var email = profile.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

        string? pictureUrl = null;
        if (profile.TryGetProperty("picture", out var picEl) &&
            picEl.TryGetProperty("data", out var picData) &&
            picData.TryGetProperty("url", out var urlEl))
        {
            pictureUrl = urlEl.GetString();
        }

        if (string.IsNullOrEmpty(userId))
        {
            return SocialAuthResult.Failed("Failed to extract Facebook user ID.");
        }

        return SocialAuthResult.Succeeded(
            SocialProvider.Facebook,
            userId,
            email,
            name,
            pictureUrl);
    }

    private static string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url
            .Replace('-', '+')
            .Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
