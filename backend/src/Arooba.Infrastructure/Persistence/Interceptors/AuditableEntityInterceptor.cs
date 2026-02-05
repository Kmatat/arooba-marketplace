using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Arooba.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically populates audit fields on tracked entities.
/// Sets <c>CreatedAt</c>, <c>UpdatedAt</c>, <c>CreatedBy</c>, and <c>LastModifiedBy</c> based
/// on the current date-time service and authenticated user context.
/// </summary>
public class AuditableEntityInterceptor(
    IDateTimeService dateTimeService,
    ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    private readonly IDateTimeService _dateTimeService = dateTimeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    /// <summary>
    /// Called synchronously before <c>SaveChanges</c> is invoked on the context.
    /// Applies audit field updates to all added and modified entities.
    /// </summary>
    /// <param name="eventData">The event data containing the DbContext.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The interception result, possibly modified.</returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Called asynchronously before <c>SaveChangesAsync</c> is invoked on the context.
    /// Applies audit field updates to all added and modified entities.
    /// </summary>
    /// <param name="eventData">The event data containing the DbContext.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The interception result, possibly modified.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Iterates over all tracked <see cref="BaseEntity"/> and <see cref="AuditableEntity"/> entries
    /// and sets their audit properties based on the current state (Added or Modified).
    /// </summary>
    /// <param name="context">The database context whose change tracker will be inspected.</param>
    private void ApplyAuditFields(DbContext? context)
    {
        if (context is null) return;

        var utcNow = _dateTimeService.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
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

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
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
    }
}
