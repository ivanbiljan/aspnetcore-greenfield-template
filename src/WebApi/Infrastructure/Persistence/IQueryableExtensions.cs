using System.Linq.Expressions;

namespace WebApi.Infrastructure.Persistence;

/// <summary>
///     Provides extension methods for the <see cref="IQueryable" /> type.
/// </summary>
// ReSharper disable once InconsistentNaming
internal static class IQueryableExtensions
{
    /// <summary>
    ///     Conditionally filters a sequence based on the provided predicate.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}" />.</param>
    /// <param name="predicate">The predicate used to filter the sequence.</param>
    /// <param name="condition">The flag that determines whether the sequence should be filtered.</param>
    /// <typeparam name="T">The type of element in the collection.</typeparam>
    /// <returns>An <see cref="IQueryable{T}" /> that represents the filtered sequence.</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> queryable,
        Expression<Func<T, bool>> predicate,
        bool condition
    )
    {
        return !condition ? queryable : queryable.Where(predicate);
    }
}