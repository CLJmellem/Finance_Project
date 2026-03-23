using MongoDB.Bson;
using MongoDB.Driver;
using TransactionsService.Application.Interfaces;
using TransactionsService.Domain.Entities;
using TransactionsService.Infrastructure.Persistence;

namespace TransactionsService.Infrastructure.Repositories;

/// <summary>
/// Repository for the usersTransactions collection.
/// Extends BaseRepository with transaction-specific operations.
/// </summary>
public sealed class TransactionRepository : BaseRepository<TransactionsDataEntity>, ITransactionRepository
{
    public TransactionRepository(MongoDbContext<TransactionsDataEntity> context)
        : base("usersTransactions", context)
    {
        ConfigureIndexes();
    }

    private void ConfigureIndexes()
    {
        var keys = Builders<TransactionsDataEntity>.IndexKeys;

        var indexes = new List<CreateIndexModel<TransactionsDataEntity>>
        {
            new(keys.Ascending(t => t.UserId).Ascending(t => t.CardId),
                new CreateIndexOptions { Name = "idx_userId_cardId" }),

            new(keys.Ascending(t => t.CardId),
                new CreateIndexOptions { Name = "idx_cardId" }),

            new(keys.Ascending(t => t.CardDueDate),
                new CreateIndexOptions { Name = "idx_cardDueDate" }),

            new(keys.Descending(t => t.CreatedAt),
                new CreateIndexOptions { Name = "idx_createdAt_desc" })
        };

        _collection.Indexes.CreateMany(indexes);
    }

    /// <summary>
    /// Calculates total used credit for a card.
    /// Remaining balance = ActualInstallments * MonthAmount (installment) or TotalAmount (non-installment).
    /// </summary>
    public async Task<double> GetUsedCreditAsync(string cardId, CancellationToken ct = default)
    {
        var cardObjectId = ObjectId.Parse(cardId);
        var transactions = await _collection
            .Find(t => t.CardId == cardObjectId)
            .ToListAsync(ct);

        return transactions.Sum(CalculateRemainingBalance);
    }

    /// <summary>
    /// Calculates used credit excluding a specific transaction (for update validation).
    /// </summary>
    public async Task<double> GetUsedCreditExcludingAsync(
        string cardId, string excludeTransactionId, CancellationToken ct = default)
    {
        var cardObjectId = ObjectId.Parse(cardId);
        var excludeObjectId = ObjectId.Parse(excludeTransactionId);
        var transactions = await _collection
            .Find(t => t.CardId == cardObjectId && t.Id != excludeObjectId)
            .ToListAsync(ct);

        return transactions.Sum(CalculateRemainingBalance);
    }

    /// <summary>Physically deletes all transactions belonging to the given card.</summary>
    public async Task DeleteByCardIdAsync(string cardId, CancellationToken ct = default)
    {
        var cardObjectId = ObjectId.Parse(cardId);
        await _collection.DeleteManyAsync(t => t.CardId == cardObjectId, ct);
    }

    /// <summary>Returns installment transactions whose billing cycle has expired.</summary>
    public async Task<IReadOnlyList<TransactionsDataEntity>> GetDueInstallmentTransactionsAsync(
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return (await _collection
            .Find(t => t.Installments != null && t.CardDueDate <= now)
            .ToListAsync(ct)).AsReadOnly();
    }

    /// <summary>Returns recurring non-installment transactions whose billing cycle has expired.</summary>
    public async Task<IReadOnlyList<TransactionsDataEntity>> GetDueRecurringTransactionsAsync(
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return (await _collection
            .Find(t => t.IsRecurring && t.Installments == null && t.CardDueDate <= now)
            .ToListAsync(ct)).AsReadOnly();
    }

    /// <summary>
    /// Remaining balance per transaction:
    /// - Installment: ActualInstallments * MonthAmount
    /// - Non-installment: TotalAmount
    /// </summary>
    private static double CalculateRemainingBalance(TransactionsDataEntity t)
    {
        if (t.Installments.HasValue && t.ActualInstallments.HasValue && t.MonthAmount.HasValue)
            return t.ActualInstallments.Value * t.MonthAmount.Value;

        return t.TotalAmount;
    }
}
