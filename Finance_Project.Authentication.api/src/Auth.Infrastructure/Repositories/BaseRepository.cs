using Auth.Domain.Entities.Interface;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Auth.Infrastructure.Repositories
{
    /// <summary>
    /// BaseRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IBaseEntity
    {
        private readonly IMongoCollection<TEntity> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="settings">The settings.</param>
        public BaseRepository(string collection, MongoDbContext<TEntity> context)
        {
            _collection = context.GetCollection(collection);
        }

        /// <summary>Gets the by identifier asynchronous.</summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<TEntity> GetByIdAsync(ObjectId id)
        {
            return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>Creates the asynchronous.</summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        /// <summary>Updates the asynchronous.</summary>
        /// <param name="user">The user.</param>
        public async Task UpdateAsync(TEntity entity)
        {
            await _collection.ReplaceOneAsync(u => u.Id == entity.Id, entity);
        }

        /// <summary>Deletes the asynchronous.</summary>
        /// <param name="id">The identifier.</param>
        public async Task DeleteAsync(ObjectId id)
        {
            await _collection.DeleteOneAsync(u => u.Id == id);
        }

        /// <summary>Gets the one asynchronous.</summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public async Task<TEntity> GetOneAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>Gets all asynchronous.</summary>
        /// <returns></returns>
        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }
    }
}