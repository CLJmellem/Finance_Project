using MediatR;
using TransactionsService.Application.DTOs;
using TransactionsService.Domain.Enums;

namespace TransactionsService.Application.Commands.AddTransaction;

/// <summary>
/// Command to add a new transaction to a card.
/// CardId comes from the route; UserId is resolved from JWT (or body for admin).
/// </summary>
public sealed record AddTransactionCommand(
    string CardId,
    double TotalAmount,
    double? MonthAmount,
    int? Installments,
    TransactionType Type,
    string? Description,
    bool IsRecurring,
    string? UserId) : IRequest<TransactionResponse>;
