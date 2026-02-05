namespace Arooba.Domain.Interfaces;

/// <summary>
/// Defines a unit of work that coordinates the writing of changes across multiple repositories
/// and ensures transactional consistency.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Commits all pending changes to the data store within a single transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
