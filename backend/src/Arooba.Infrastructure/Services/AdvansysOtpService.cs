using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Arooba.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// OTP service implementation using ADVANSYS SMS gateway.
/// Generates OTP codes, stores them in the database, and sends them via ADVANSYS HTTP API.
/// </summary>
public class AdvansysOtpService : IOtpService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdvansysOtpService> _logger;
    private readonly IDateTimeService _dateTime;

    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 5;
    private const int MaxOtpAttempts = 5;

    public AdvansysOtpService(
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AdvansysOtpService> logger,
        IDateTimeService dateTime)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _dateTime = dateTime;
    }

    /// <inheritdoc />
    public async Task<OtpSendResult> SendOtpAsync(string mobileNumber)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == mobileNumber);

            if (user is null)
            {
                return OtpSendResult.Failed("Mobile number is not registered.");
            }

            // Rate-limit: prevent resending if previous OTP hasn't expired yet (cooldown: 60s)
            if (user.OtpExpiresAt.HasValue &&
                user.OtpExpiresAt.Value > _dateTime.UtcNow &&
                user.OtpExpiresAt.Value.AddMinutes(-OtpExpiryMinutes + 1) > _dateTime.UtcNow)
            {
                return OtpSendResult.Failed("Please wait before requesting a new OTP.");
            }

            // Generate a cryptographically secure OTP
            var otpCode = GenerateOtp();
            user.OtpCode = otpCode;
            user.OtpExpiresAt = _dateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            user.OtpAttempts = 0;

            await _context.SaveChangesAsync();

            // Send OTP via ADVANSYS SMS gateway
            var transactionId = await SendSmsViaAdvansysAsync(mobileNumber, otpCode);

            _logger.LogInformation(
                "OTP sent to {MobileNumber}, TransactionId: {TransactionId}",
                MaskMobileNumber(mobileNumber),
                transactionId);

            return OtpSendResult.Succeeded(transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {MobileNumber}", MaskMobileNumber(mobileNumber));
            return OtpSendResult.Failed("Failed to send OTP. Please try again later.");
        }
    }

    /// <inheritdoc />
    public async Task<OtpVerifyResult> VerifyOtpAsync(string mobileNumber, string otpCode)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == mobileNumber);

        if (user is null)
        {
            return OtpVerifyResult.Invalid("Mobile number is not registered.");
        }

        if (string.IsNullOrEmpty(user.OtpCode))
        {
            return OtpVerifyResult.Invalid("No OTP has been requested. Please request a new OTP.");
        }

        if (user.OtpAttempts >= MaxOtpAttempts)
        {
            // Clear the OTP to force re-request
            user.OtpCode = null;
            user.OtpExpiresAt = null;
            user.OtpAttempts = 0;
            await _context.SaveChangesAsync();
            return OtpVerifyResult.Invalid("Too many failed attempts. Please request a new OTP.");
        }

        if (user.OtpExpiresAt < _dateTime.UtcNow)
        {
            user.OtpCode = null;
            user.OtpExpiresAt = null;
            user.OtpAttempts = 0;
            await _context.SaveChangesAsync();
            return OtpVerifyResult.Invalid("OTP has expired. Please request a new one.");
        }

        if (!string.Equals(user.OtpCode, otpCode, StringComparison.Ordinal))
        {
            user.OtpAttempts++;
            await _context.SaveChangesAsync();
            return OtpVerifyResult.Invalid("Invalid OTP code.");
        }

        // OTP is valid - clear it and mark mobile as verified
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        user.OtpAttempts = 0;
        user.IsMobileVerified = true;
        user.IsVerified = true;
        await _context.SaveChangesAsync();

        return OtpVerifyResult.Valid();
    }

    private async Task<string> SendSmsViaAdvansysAsync(string mobileNumber, string otpCode)
    {
        var settings = _configuration.GetSection("AdvansysSettings");
        var apiUrl = settings["ApiUrl"] ?? "https://smsmisr.com/api/OTP/";
        var username = settings["Username"] ?? string.Empty;
        var password = settings["Password"] ?? string.Empty;
        var senderId = settings["SenderId"] ?? "Arooba";
        var environment = settings["Environment"] ?? "2"; // 1=production, 2=testing

        var client = _httpClientFactory.CreateClient("Advansys");

        // Strip leading '+' for ADVANSYS; they expect the number without '+'
        var formattedNumber = mobileNumber.TrimStart('+');

        var requestBody = new
        {
            environment,
            username,
            password,
            sender = senderId,
            mobile = formattedNumber,
            otp = otpCode,
            language = "2", // 1=Arabic, 2=English
            template = $"Your Arooba verification code is: {otpCode}. Valid for {OtpExpiryMinutes} minutes."
        };

        var response = await client.PostAsJsonAsync(apiUrl, requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "ADVANSYS SMS API returned {StatusCode}: {Response}",
                response.StatusCode,
                responseContent);
            throw new InvalidOperationException($"ADVANSYS SMS API error: {response.StatusCode}");
        }

        // Parse response to get transaction ID
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var code = doc.RootElement.TryGetProperty("code", out var codeEl)
                ? codeEl.GetString()
                : null;

            if (code != null && code != "1901" && code != "4901")
            {
                _logger.LogWarning("ADVANSYS returned error code {Code}: {Response}", code, responseContent);
                throw new InvalidOperationException($"ADVANSYS error code: {code}");
            }

            return doc.RootElement.TryGetProperty("SMSID", out var smsId)
                ? smsId.GetString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();
        }
        catch (JsonException)
        {
            return Guid.NewGuid().ToString();
        }
    }

    private static string GenerateOtp()
    {
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % (int)Math.Pow(10, OtpLength);
        return number.ToString().PadLeft(OtpLength, '0');
    }

    private static string MaskMobileNumber(string mobile)
    {
        if (mobile.Length <= 4) return "****";
        return string.Concat(mobile.AsSpan(0, mobile.Length - 4), "****");
    }
}
