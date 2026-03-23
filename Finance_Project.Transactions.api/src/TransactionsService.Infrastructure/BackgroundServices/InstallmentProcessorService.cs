using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransactionsService.Application.Interfaces;
using TransactionsService.Domain.Entities;

namespace TransactionsService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs daily to process installment and recurring transactions.
///
/// Installment logic:
///   - Finds all transactions where CardDueDate has passed and Installments != null
///   - Decrements ActualInstallments by 1
///   - Advances CardDueDate to the next month
///   - When ActualInstallments reaches 0, physically deletes the document
///
/// Recurring logic:
///   - Finds non-installment transactions marked IsRecurring = true whose CardDueDate has passed
///   - Clones the transaction with a new CardDueDate for the next month
///   - Deletes the old document
/// </summary>
public sealed class InstallmentProcessorService(
    IServiceScopeFactory scopeFactory,
    ILogger<InstallmentProcessorService> logger)
    : BackgroundService
{
    // Runs once every 24 hours
    private static readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("InstallmentProcessorService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during installment processing cycle.");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        logger.LogInformation("InstallmentProcessorService stopped.");
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        // Use a new scope per cycle since ITransactionRepository is scoped
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        await ProcessInstallmentsAsync(repository, ct);
        await ProcessRecurringAsync(repository, ct);
    }

    private async Task ProcessInstallmentsAsync(ITransactionRepository repository, CancellationToken ct)
    {
        var due = await repository.GetDueInstallmentTransactionsAsync(ct);

        foreach (var tx in due)
        {
            tx.ActualInstallments -= 1;

            if (tx.ActualInstallments <= 0)
            {
                // All installments have been paid — remove the document
                await repository.DeleteAsync(tx.Id.ToString(), ct);
                logger.LogInformation("Installment transaction {Id} fully paid and deleted.", tx.Id);
            }
            else
            {
                // Advance the due date to the next billing cycle
                tx.CardDueDate = AdvanceOneMonth(tx.CardDueDate);
                await repository.UpdateAsync(tx, ct);
                logger.LogInformation(
                    "Transaction {Id}: ActualInstallments decremented to {Remaining}. Next due: {DueDate}.",
                    tx.Id, tx.ActualInstallments, tx.CardDueDate);
            }
        }
    }

    private async Task ProcessRecurringAsync(ITransactionRepository repository, CancellationToken ct)
    {
        var due = await repository.GetDueRecurringTransactionsAsync(ct);

        foreach (var tx in due)
        {
            // Clone the transaction for the next billing cycle
            var renewed = new TransactionsDataEntity
            {
                UserId = tx.UserId,
                CardId = tx.CardId,
                TotalAmount = tx.TotalAmount,
                MonthAmount = tx.MonthAmount,
                Installments = tx.Installments,
                ActualInstallments = tx.ActualInstallments,
                Type = tx.Type,
                Description = tx.Description,
                IsRecurring = true,
                CardDueDate = AdvanceOneMonth(tx.CardDueDate),
                CreatedAt = DateTime.UtcNow
            };

            await repository.CreateAsync(renewed, ct);
            await repository.DeleteAsync(tx.Id.ToString(), ct);

            logger.LogInformation(
                "Recurring transaction {OldId} renewed as {NewId} for {DueDate}.",
                tx.Id, renewed.Id, renewed.CardDueDate);
        }
    }

    /// <summary>Advances a date by exactly one month, clamping to the last valid day when necessary.</summary>
    private static DateTime AdvanceOneMonth(DateTime date)
    {
        var next = date.AddMonths(1);
        var lastDay = DateTime.DaysInMonth(next.Year, next.Month);
        var day = Math.Min(date.Day, lastDay);
        return new DateTime(next.Year, next.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}
