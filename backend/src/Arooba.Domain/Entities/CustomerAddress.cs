using Arooba.Domain.Common;

namespace Arooba.Domain.Entities;

/// <summary>Represents a customer address in the Arooba Marketplace domain.</summary>
public class CustomerAddress : AuditableEntity
{
    public int CustomerId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    /// <summary>Navigation property to the parent customer.</summary>
    public Customer? Customer { get; set; }
}
