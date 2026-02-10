using Arooba.Domain.Common;
using Arooba.Domain.Enums;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Customer in the Arooba Marketplace domain.</summary>
public class Customer : AuditableEntity
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;

    /// <summary>Customer's mobile number (+20 format).</summary>
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>Customer's email address (optional).</summary>
    public string? Email { get; set; }

    /// <summary>Preferred language: ar or en.</summary>
    public string PreferredLanguage { get; set; } = "ar";

    /// <summary>Customer account status.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether the customer account is blocked.</summary>
    public bool IsBlocked { get; set; }

    /// <summary>Loyalty tier based on lifetime points.</summary>
    public CustomerTier Tier { get; set; } = CustomerTier.Bronze;

    /// <summary>Current loyalty points balance.</summary>
    public int LoyaltyPoints { get; set; }

    /// <summary>Total loyalty points ever earned.</summary>
    public int LifetimeLoyaltyPoints { get; set; }

    /// <summary>Wallet balance in EGP.</summary>
    public decimal WalletBalance { get; set; }

    /// <summary>Total amount spent across all orders.</summary>
    public decimal TotalSpent { get; set; }

    /// <summary>Unique referral code for this customer.</summary>
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>Referral code used by this customer when registering.</summary>
    public string? ReferredBy { get; set; }

    /// <summary>Number of successful referrals made by this customer.</summary>
    public int ReferralCount { get; set; }

    /// <summary>Total number of orders placed.</summary>
    public int TotalOrders { get; set; }

    /// <summary>Total number of reviews submitted.</summary>
    public int TotalReviews { get; set; }

    /// <summary>Average rating given across all reviews.</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Last login timestamp.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Total number of sessions.</summary>
    public int TotalSessions { get; set; }

    /// <summary>Navigation property to the associated user.</summary>
    public User? User { get; set; }

    /// <summary>Navigation property to the customer's orders.</summary>
    public List<Order>? Orders { get; set; }

    /// <summary>Navigation property to the customer's addresses.</summary>
    public List<CustomerAddress>? Addresses { get; set; }

    /// <summary>Navigation property to the customer's reviews.</summary>
    public List<CustomerReview>? Reviews { get; set; }

    /// <summary>Navigation property to the customer's login history.</summary>
    public List<CustomerLoginHistory>? LoginHistory { get; set; }

    public virtual ICollection<Subscription>? Subscriptions { get; set; }
}
