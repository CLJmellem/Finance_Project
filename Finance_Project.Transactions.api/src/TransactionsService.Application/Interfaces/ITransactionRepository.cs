using TransactionsService.Domain.Entities;
using TransactionsService.Domain.Interfaces;

namespace TransactionsService.Application.Interfaces;

/// <summary>
/// ITransactionRepository
/// </summary>
/// <seealso cref="TransactionsService.Domain.Interfaces.IBaseRepository&lt;TransactionsService.Domain.Entities.TransactionsDataEntity&gt;" />
public interface ITransactionRepository : IBaseRepository<TransactionsDataEntity>
{
    /// <summary>
    /// Returns the sum of remaining balances for all transactions of a given card.
    /// Remaining balance = ActualInstallments * MonthAmount (installment) or TotalAmount (non-installment).
    /// </summary>
    Task<double> GetUsedCreditAsync(string cardId, CancellationToken ct = default);

    /// <summary>
    /// Returns the used credit excluding a specific transaction (for update validation).
    /// </summary>
    Task<double> GetUsedCreditExcludingAsync(string cardId, string excludeTransactionId, CancellationToken ct = default);

    /// <summary>Deletes all transactions belonging to a card (used when a card is deleted).</summary>
    Task DeleteByCardIdAsync(string cardId, CancellationToken ct = default);

    /// <summary>Returns all installment transactions whose CardDueDate has passed (used by background service).</summary>
    Task<IReadOnlyList<TransactionsDataEntity>> GetDueInstallmentTransactionsAsync(CancellationToken ct = default);

    /// <summary>Returns all recurring non-installment transactions whose CardDueDate has passed.</summary>
    Task<IReadOnlyList<TransactionsDataEntity>> GetDueRecurringTransactionsAsync(CancellationToken ct = default);
}
