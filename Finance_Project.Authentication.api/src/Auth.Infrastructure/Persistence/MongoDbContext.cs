using Auth.Domain.Entities.Interface;
using Auth.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Auth.Infrastructure.Persistence;

/// <summary>
/// MongoDbContext
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class MongoDbContext<TEntity> where TEntity : IBaseEntity
{
    /// <summary>The database</summary>
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbContext{TEntity}"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    /// <summary>
    /// Gets the collection by name.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <returns></returns>
    public IMongoCollection<TEntity> GetCollection(string collectionName)
    {
        return _database.GetCollection<TEntity>(collectionName);
    }
}