using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Common;
using Arooba.Domain.Entities;
using Arooba.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the Arooba Marketplace database.
/// Implements both <see cref="IAroobaDbContext"/> (Domain) and <see cref="IApplicationDbContext"/>
/// (Application) to expose typed DbSet properties. Automatically manages audit timestamps
/// on tracked entities that derive from <see cref="BaseEntity"/> or <see cref="AuditableEntity"/>.
/// </summary>
public class AroobaDbContext : DbContext, IAroobaDbContext, IApplicationDbContext
{
    private readonly Domain.Interfaces.IDateTimeService _dateTime;
    private readonly Domain.Interfaces.ICurrentUserService _currentUser;

    /// <summary>
    /// Initializes a new instance of <see cref="AroobaDbContext"/> with the specified options
    /// and service dependencies for audit tracking.
    /// </summary>
    /// <param name="options">The EF Core context configuration options.</param>
    /// <param name="dateTime">The date-time service for audit timestamps.</param>
    /// <param name="currentUser">The current user service for audit identity tracking.</param>
    public AroobaDbContext(
        DbContextOptions<AroobaDbContext> options,
        Domain.Interfaces.IDateTimeService dateTime,
        Domain.Interfaces.ICurrentUserService currentUser)
        : base(options)
    {
        _dateTime = dateTime;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc />
    public DbSet<ParentVendor> ParentVendors => Set<ParentVendor>();

    /// <inheritdoc />
    public DbSet<SubVendor> SubVendors => Set<SubVendor>();

    /// <inheritdoc />
    public DbSet<Cooperative> Cooperatives => Set<Cooperative>();

    /// <inheritdoc />
    public DbSet<Product> Products => Set<Product>();

    /// <inheritdoc />
    public DbSet<PickupLocation> PickupLocations => Set<PickupLocation>();

    /// <inheritdoc />
    public DbSet<Order> Orders => Set<Order>();

    /// <inheritdoc />
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <inheritdoc />
    public DbSet<Shipment> Shipments => Set<Shipment>();

    /// <inheritdoc />
    public DbSet<Customer> Customers => Set<Customer>();

    /// <inheritdoc />
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    /// <inheritdoc />
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    /// <inheritdoc />
    public DbSet<VendorWallet> VendorWallets => Set<VendorWallet>();

    /// <inheritdoc />
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    /// <inheritdoc />
    public DbSet<TransactionSplit> TransactionSplits => Set<TransactionSplit>();

    /// <inheritdoc />
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();

    /// <inheritdoc />
    public DbSet<RateCard> RateCards => Set<RateCard>();

    /// <inheritdoc />
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    /// <inheritdoc />
    public DbSet<PlatformConfiguration> PlatformConfigurations => Set<PlatformConfiguration>();

    /// <inheritdoc />
    public DbSet<VendorActionRequest> VendorActionRequests => Set<VendorActionRequest>();

    /// <inheritdoc />
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>
    /// Applies all <see cref="IEntityTypeConfiguration{TEntity}"/> implementations
    /// found in the current assembly to the model builder.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the database model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AroobaDbContext).Assembly);
    }

    /// <summary>
    /// Overrides the default save behavior to automatically set <c>CreatedAt</c>,
    /// <c>UpdatedAt</c>, <c>CreatedBy</c>, and <c>LastModifiedBy</c> on tracked entities
    /// that derive from <see cref="BaseEntity"/> or <see cref="AuditableEntity"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTime.Now;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.LastModifiedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = userId;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
