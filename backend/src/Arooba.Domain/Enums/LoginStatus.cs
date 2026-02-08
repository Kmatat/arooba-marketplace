namespace Arooba.Domain.Enums;

/// <summary>
/// Status of a customer login attempt.
/// </summary>
public enum LoginStatus
{
    /// <summary>Login was successful.</summary>
    Success,

    /// <summary>Login failed due to invalid credentials or OTP.</summary>
    Failed,

    /// <summary>Login was blocked due to security rules.</summary>
    Blocked
}
