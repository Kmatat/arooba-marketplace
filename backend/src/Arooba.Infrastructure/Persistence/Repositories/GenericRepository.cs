using Arooba.Domain.Common;
using Arooba.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation using EF Core.
/// Provides standard CRUD operations for any entity extending <see cref="BaseEntity"/>.
/// </summary>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AroobaDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(AroobaDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }
}
