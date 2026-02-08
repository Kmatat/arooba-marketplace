using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>
/// Records each customer login attempt with device, IP, and session information
/// for security monitoring and activity tracking.
/// </summary>
public class CustomerLoginHistory : BaseEntity
{
    /// <summary>The customer who attempted to log in.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Whether the login was successful, failed, or blocked.</summary>
    public LoginStatus Status { get; set; }

    /// <summary>IP address at time of login.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>Device type: mobile, desktop, or tablet.</summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>User agent or device description.</summary>
    public string DeviceInfo { get; set; } = string.Empty;

    /// <summary>Geo-resolved location from IP address.</summary>
    public string? Location { get; set; }

    /// <summary>Duration of the session in minutes (null if login failed).</summary>
    public int? SessionDurationMinutes { get; set; }

    /// <summary>Session identifier for correlation.</summary>
    public string? SessionId { get; set; }

    // Navigation
    /// <summary>Navigation property to the customer.</summary>
    public Customer? Customer { get; set; }
}
