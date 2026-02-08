using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the Entity Framework Core DbContext for the Arooba Marketplace.
/// Allows the Application layer to query and persist entities without depending on Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Gets the Users table.</summary>
    DbSet<User> Users { get; }

    /// <summary>Gets the ParentVendors table.</summary>
    DbSet<ParentVendor> ParentVendors { get; }

    /// <summary>Gets the SubVendors table.</summary>
    DbSet<SubVendor> SubVendors { get; }

    /// <summary>Gets the Cooperatives table.</summary>
    DbSet<Cooperative> Cooperatives { get; }

    /// <summary>Gets the Products table.</summary>
    DbSet<Product> Products { get; }

    /// <summary>Gets the PickupLocations table.</summary>
    DbSet<PickupLocation> PickupLocations { get; }

    /// <summary>Gets the Orders table.</summary>
    DbSet<Order> Orders { get; }

    /// <summary>Gets the OrderItems table.</summary>
    DbSet<OrderItem> OrderItems { get; }

    /// <summary>Gets the Shipments table.</summary>
    DbSet<Shipment> Shipments { get; }

    /// <summary>Gets the Customers table.</summary>
    DbSet<Customer> Customers { get; }

    /// <summary>Gets the CustomerAddresses table.</summary>
    DbSet<CustomerAddress> CustomerAddresses { get; }

    /// <summary>Gets the Subscriptions table.</summary>
    DbSet<Subscription> Subscriptions { get; }

    /// <summary>Gets the VendorWallets table.</summary>
    DbSet<VendorWallet> VendorWallets { get; }

    /// <summary>Gets the LedgerEntries table.</summary>
    DbSet<LedgerEntry> LedgerEntries { get; }

    /// <summary>Gets the TransactionSplits table.</summary>
    DbSet<TransactionSplit> TransactionSplits { get; }

    /// <summary>Gets the ShippingZones table.</summary>
    DbSet<ShippingZone> ShippingZones { get; }

    /// <summary>Gets the RateCards table.</summary>
    DbSet<RateCard> RateCards { get; }

    /// <summary>Gets the ProductCategories table.</summary>
    DbSet<ProductCategory> ProductCategories { get; }

    /// <summary>Gets the UserActivities table for analytics tracking.</summary>
    DbSet<UserActivity> UserActivities { get; }

    /// <summary>
    /// Saves all pending changes to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
