using Arooba.Domain.Common;

namespace Arooba.Domain.Interfaces;

/// <summary>
/// Generic repository interface providing standard CRUD operations for domain entities.
/// Implementations reside in the Infrastructure layer.
/// </summary>
/// <typeparam name="T">The entity type, which must extend <see cref="BaseEntity"/>.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of this type.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of all entities.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The added entity.</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Marks an entity for deletion.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(T entity);

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
