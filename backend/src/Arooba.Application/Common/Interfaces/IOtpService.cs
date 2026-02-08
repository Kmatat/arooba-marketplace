namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Abstraction over OTP (One-Time Password) operations.
/// The implementation uses ADVANSYS as the SMS gateway provider.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Sends a one-time password to the specified Egyptian mobile number via SMS.
    /// </summary>
    /// <param name="mobileNumber">The recipient's mobile number in E.164 format (e.g., +201XXXXXXXXX).</param>
    /// <returns>A result indicating success or failure of the send operation.</returns>
    Task<OtpSendResult> SendOtpAsync(string mobileNumber);

    /// <summary>
    /// Verifies the OTP code entered by the user against the stored code.
    /// </summary>
    /// <param name="mobileNumber">The user's mobile number.</param>
    /// <param name="otpCode">The OTP code entered by the user.</param>
    /// <returns>A result indicating whether the OTP is valid.</returns>
    Task<OtpVerifyResult> VerifyOtpAsync(string mobileNumber, string otpCode);
}

/// <summary>
/// Result of an OTP send operation.
/// </summary>
public record OtpSendResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? TransactionId { get; init; }

    public static OtpSendResult Succeeded(string transactionId) =>
        new() { Success = true, TransactionId = transactionId };

    public static OtpSendResult Failed(string error) =>
        new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Result of an OTP verification operation.
/// </summary>
public record OtpVerifyResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static OtpVerifyResult Valid() => new() { IsValid = true };

    public static OtpVerifyResult Invalid(string error) =>
        new() { IsValid = false, ErrorMessage = error };
}
