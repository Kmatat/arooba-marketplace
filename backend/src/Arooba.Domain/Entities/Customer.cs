using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a Customer in the Arooba Marketplace domain.</summary>
public class Customer : AuditableEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public decimal TotalSpent { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public string? ReferredBy { get; set; }

    /// <summary>Navigation property to the associated user.</summary>
    public User? User { get; set; }

    /// <summary>Navigation property to the customer's orders.</summary>
    public List<Order>? Orders { get; set; }

    /// <summary>Navigation property to the customer's addresses.</summary>
    public List<CustomerAddress>? Addresses { get; set; }
}
