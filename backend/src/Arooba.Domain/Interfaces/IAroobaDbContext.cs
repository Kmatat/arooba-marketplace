using Arooba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Domain.Interfaces;

/// <summary>
/// Abstraction over the Entity Framework Core database context for the Arooba Marketplace.
/// Exposes <see cref="DbSet{TEntity}"/> properties for all domain entities.
/// </summary>
public interface IAroobaDbContext
{
    /// <summary>Gets the users table.</summary>
    DbSet<User> Users { get; }

    /// <summary>Gets the parent vendors table.</summary>
    DbSet<ParentVendor> ParentVendors { get; }

    /// <summary>Gets the sub-vendors table.</summary>
    DbSet<SubVendor> SubVendors { get; }

    /// <summary>Gets the cooperatives table.</summary>
    DbSet<Cooperative> Cooperatives { get; }

    /// <summary>Gets the products table.</summary>
    DbSet<Product> Products { get; }

    /// <summary>Gets the pickup locations table.</summary>
    DbSet<PickupLocation> PickupLocations { get; }

    /// <summary>Gets the orders table.</summary>
    DbSet<Order> Orders { get; }

    /// <summary>Gets the order items table.</summary>
    DbSet<OrderItem> OrderItems { get; }

    /// <summary>Gets the shipments table.</summary>
    DbSet<Shipment> Shipments { get; }

    /// <summary>Gets the customers table.</summary>
    DbSet<Customer> Customers { get; }

    /// <summary>Gets the customer addresses table.</summary>
    DbSet<CustomerAddress> CustomerAddresses { get; }

    /// <summary>Gets the subscriptions table.</summary>
    DbSet<Subscription> Subscriptions { get; }

    /// <summary>Gets the vendor wallets table.</summary>
    DbSet<VendorWallet> VendorWallets { get; }

    /// <summary>Gets the ledger entries table.</summary>
    DbSet<LedgerEntry> LedgerEntries { get; }

    /// <summary>Gets the transaction splits table.</summary>
    DbSet<TransactionSplit> TransactionSplits { get; }

    /// <summary>Gets the shipping zones table.</summary>
    DbSet<ShippingZone> ShippingZones { get; }

    /// <summary>Gets the rate cards table.</summary>
    DbSet<RateCard> RateCards { get; }

    /// <summary>Gets the product categories table.</summary>
    DbSet<ProductCategory> ProductCategories { get; }

    /// <summary>Gets the user activities table for analytics tracking.</summary>
    DbSet<UserActivity> UserActivities { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
