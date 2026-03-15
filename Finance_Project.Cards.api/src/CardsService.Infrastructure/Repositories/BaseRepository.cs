using CardsService.Domain.Entities.Interface;
using CardsService.Domain.Interfaces;
using CardsService.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace CardsService.Infrastructure.Repositories;

/// <summary>
/// BaseRepository
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <seealso cref="CardsService.Domain.Interfaces.IBaseRepository&lt;TEntity&gt;" />
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IBaseEntity
{
    protected readonly IMongoCollection<TEntity> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="context">The context.</param>
    public BaseRepository(string collection, MongoDbContext<TEntity> context)
    {
        _collection = context.GetCollection(collection);
    }

    /// <summary>Gets the by identifier asynchronous.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    /// <summary>Creates the asynchronous.</summary>
    /// <param name="entity">The entity.</param>
    /// <param name="ct">The ct.</param>
    public async Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    /// <summary>Updates the asynchronous.</summary>
    /// <param name="entity">The entity.</param>
    /// <param name="ct">The ct.</param>
    public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", ObjectId.Parse(entity.Id));
        await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
    }

    /// <summary>Deletes the asynchronous.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter, ct);
    }

    /// <summary>Gets the one asynchronous.</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<TEntity?> GetOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken ct = default)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    /// <summary>Gets all asynchronous.</summary>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        IFindFluent<TEntity, TEntity> query = _collection
            .Find(filter ?? FilterDefinition<TEntity>.Empty)
            .SortByDescending(e => e.CreatedAt);

        if (page > 0 && pageSize > 0)
            query = query.Skip((page - 1) * pageSize).Limit(pageSize);

        return (await query.ToListAsync(ct)).AsReadOnly();
    }

    /// <summary>Counts all asynchronous.</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<long> CountAllAsync(
        Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        return filter is null
            ? await _collection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty, cancellationToken: ct)
            : await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
    }
}