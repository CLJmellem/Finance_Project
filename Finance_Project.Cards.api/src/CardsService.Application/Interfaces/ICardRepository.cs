using CardsService.Domain.Entities;
using CardsService.Domain.Interfaces;

namespace CardsService.Application.Interfaces;

/// <summary>
/// ICardRepository
/// </summary>
/// <seealso cref="CardsService.Domain.Interfaces.IBaseRepository&lt;CardsService.Domain.Entities.CardDataEntity&gt;" />
public interface ICardRepository : IBaseRepository<CardDataEntity>
{
}
