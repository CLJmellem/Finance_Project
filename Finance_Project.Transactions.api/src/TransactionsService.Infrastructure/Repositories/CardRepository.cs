using TransactionsService.Application.Interfaces;
using TransactionsService.Domain.Entities;
using TransactionsService.Infrastructure.Persistence;

namespace TransactionsService.Infrastructure.Repositories;

/// <summary>
/// Repository that accesses the usersCards collection from the shared MongoDB database.
/// Uses BaseRepository methods (GetOneAsync) for all card lookups.
/// </summary>
public sealed class CardRepository : BaseRepository<CardDataEntity>, ICardRepository
{
    public CardRepository(MongoDbContext<CardDataEntity> context)
        : base("usersCards", context)
    {
    }
}
