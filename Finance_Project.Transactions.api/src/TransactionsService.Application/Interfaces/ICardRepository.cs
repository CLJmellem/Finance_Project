using TransactionsService.Domain.Entities;
using TransactionsService.Domain.Interfaces;

namespace TransactionsService.Application.Interfaces;

/// <summary>
/// ICardRepository — read access to the shared usersCards collection.
/// Uses BaseRepository methods (GetOneAsync) for all card lookups.
/// </summary>
public interface ICardRepository : IBaseRepository<CardDataEntity>
{
}
