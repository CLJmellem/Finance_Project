using System.Linq.Expressions;
using TransactionsService.Domain.Entities.Interface;

namespace TransactionsService.Domain.Interfaces;

/// <summary>
/// IBaseRepository
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IBaseRepository<TEntity> where TEntity : IBaseEntity
{
    /// <summary>Gets the by identifier asynchronous.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Creates the asynchronous.</summary>
    /// <param name="entity">The entity.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task CreateAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Updates the asynchronous.</summary>
    /// <param name="entity">The entity.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Deletes the asynchronous.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>Gets the one asynchronous.</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<TEntity?> GetOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken ct = default);

    /// <summary>Gets all asynchronous.</summary>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);

    /// <summary>Counts all asynchronous.</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<long> CountAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
}