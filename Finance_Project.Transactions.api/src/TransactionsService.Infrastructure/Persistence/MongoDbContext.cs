using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TransactionsService.Domain.Entities.Interface;
using TransactionsService.Infrastructure.Configuration;

namespace TransactionsService.Infrastructure.Persistence;

/// <summary>
/// MongoDb context.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class MongoDbContext<TEntity> where TEntity : IBaseEntity
{
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes the context with the MongoDB settings.
    /// </summary>
    /// <param name="settings">Database settings.</param>
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    /// <summary>
    /// Returns the collection by the specified name.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    public IMongoCollection<TEntity> GetCollection(string collectionName)
    {
        return _database.GetCollection<TEntity>(collectionName);
    }

    /// <summary>
    /// Exposes the database for infrastructure operations (e.g., health check).
    /// </summary>
    public IMongoDatabase Database => _database;
}
