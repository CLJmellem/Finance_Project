using CardsService.Application.Interfaces;
using CardsService.Domain.Entities;
using CardsService.Infrastructure.Persistence;
using MongoDB.Driver;

namespace CardsService.Infrastructure.Repositories;

/// <summary>
/// CardRepository
/// </summary>
/// <seealso cref="CardsService.Infrastructure.Repositories.BaseRepository&lt;CardsService.Domain.Entities.CardDataEntity&gt;" />
/// <seealso cref="CardsService.Application.Interfaces.ICardRepository" />
public sealed class CardRepository : BaseRepository<CardDataEntity>, ICardRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CardRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public CardRepository(MongoDbContext<CardDataEntity> context) : base("usersCards", context)
    {
        ConfigureIndexes();
    }

    /// <summary>Configures the indexes.</summary>
    private void ConfigureIndexes()
    {
        var indexKeys = Builders<CardDataEntity>.IndexKeys;

        var indexes = new List<CreateIndexModel<CardDataEntity>>
        {
            new(indexKeys.Ascending(c => c.UserId).Ascending(c => c.IsActive),
                new CreateIndexOptions { Name = "idx_userId_isActive" }),

            new(indexKeys.Descending(c => c.CreatedAt),
                new CreateIndexOptions { Name = "idx_createdAt_desc" })
        };

        _collection.Indexes.CreateMany(indexes);
    }
}
