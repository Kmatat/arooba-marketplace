using System.Linq.Expressions;
using Arooba.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/> providing pagination, ordering,
/// and filtering helpers used throughout the Infrastructure layer.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies offset-based pagination to a queryable source.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The paginated query.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="pageNumber"/> is less than 1 or <paramref name="pageSize"/> is less than 1.
    /// </exception>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Applies ascending ordering by the specified property expression.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The property type used for ordering.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="keySelector">An expression selecting the property to order by.</param>
    /// <returns>The ordered query.</returns>
    public static IOrderedQueryable<T> OrderByAscending<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector)
    {
        return query.OrderBy(keySelector);
    }

    /// <summary>
    /// Applies descending ordering by the specified property expression.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The property type used for ordering.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="keySelector">An expression selecting the property to order by.</param>
    /// <returns>The ordered query.</returns>
    public static IOrderedQueryable<T> OrderByDescending<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector)
    {
        return query.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Conditionally applies a filter predicate to the query.
    /// If the condition is <c>false</c>, the query is returned unmodified.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="condition">Whether to apply the filter.</param>
    /// <param name="predicate">The filter predicate to apply when the condition is true.</param>
    /// <returns>The filtered query if the condition is met; otherwise the original query.</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Applies a search filter across a string property using a case-insensitive contains check.
    /// If the search term is null or whitespace, the query is returned unmodified.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="propertySelector">An expression selecting the string property to search.</param>
    /// <param name="searchTerm">The search term to match.</param>
    /// <returns>The filtered query if a search term is provided; otherwise the original query.</returns>
    public static IQueryable<T> Search<T>(
        this IQueryable<T> query,
        Expression<Func<T, string>> propertySelector,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var searchValue = Expression.Constant(searchTerm.Trim());
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        var containsCall = Expression.Call(property, containsMethod, searchValue);
        var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Projects the query results into a paginated result containing both the items and total count.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing the items and total count.</returns>
    public static async Task<(List<T> Items, int TotalCount)> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Paginate(pageNumber, pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    /// <summary>
    /// Orders entities that derive from <see cref="BaseEntity"/> by <c>CreatedAt</c> descending
    /// (most recent first), which is the default sort order for Arooba queries.
    /// </summary>
    /// <typeparam name="T">The entity type, which must inherit from <see cref="BaseEntity"/>.</typeparam>
    /// <param name="query">The source query.</param>
    /// <returns>The ordered query.</returns>
    public static IOrderedQueryable<T> OrderByMostRecent<T>(this IQueryable<T> query)
        where T : BaseEntity
    {
        return query.OrderByDescending(e => e.CreatedAt);
    }
}
