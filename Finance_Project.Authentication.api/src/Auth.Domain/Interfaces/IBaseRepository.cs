using Auth.Domain.Entities.Interface;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace Auth.Domain.Interfaces
{
    /// <summary>
    /// IBaseRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IBaseRepository<TEntity> where TEntity : IBaseEntity
    {
        /// <summary>Gets the by identifier asynchronous.</summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<TEntity> GetByIdAsync(ObjectId id);

        /// <summary>Creates the asynchronous.</summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task<TEntity> CreateAsync(TEntity entity);

        /// <summary>Updates the asynchronous.</summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>Deletes the asynchronous.</summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task DeleteAsync(ObjectId id);

        /// <summary>Gets the one asynchronous.</summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        Task<TEntity> GetOneAsync(Expression<Func<TEntity, bool>> filter);

        /// <summary>Gets all asynchronous.</summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter);
    }
}