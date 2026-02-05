using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a VendorWallet in the Arooba Marketplace domain.</summary>
public class VendorWallet : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
