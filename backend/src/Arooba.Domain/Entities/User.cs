using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a platform user.</summary>
public class User : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MobileNumber { get => PhoneNumber; set => PhoneNumber = value; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LastLoginIp { get; set; }
    public string? LastLoginDeviceId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Social login fields
    public SocialProvider SocialProvider { get; set; } = SocialProvider.None;
    public string? SocialProviderId { get; set; }

    // OTP fields
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public int OtpAttempts { get; set; }
    public bool IsMobileVerified { get; set; }
}
