namespace Arooba.Domain.Enums;

/// <summary>
/// Represents the social authentication provider used for user registration or login.
/// </summary>
public enum SocialProvider
{
    /// <summary>No social provider; user registered with mobile number + OTP.</summary>
    None = 0,

    /// <summary>Google OAuth 2.0 sign-in.</summary>
    Google = 1,

    /// <summary>Apple Sign In.</summary>
    Apple = 2,

    /// <summary>Facebook Login.</summary>
    Facebook = 3
}
