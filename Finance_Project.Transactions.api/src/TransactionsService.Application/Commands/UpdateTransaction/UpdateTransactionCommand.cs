using MediatR;
using TransactionsService.Application.DTOs;
using TransactionsService.Domain.Enums;

namespace TransactionsService.Application.Commands.UpdateTransaction;

/// <summary>
/// Command to update an existing transaction.
/// CardId and TransactionId come from the route.
/// </summary>
public sealed record UpdateTransactionCommand(
    string CardId,
    string TransactionId,
    double? TotalAmount,
    double? MonthAmount,
    int? Installments,
    TransactionType? Type,
    string? Description,
    string? UserId) : IRequest<TransactionResponse>;
