using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionsService.Application.Commands.AddTransaction;
using TransactionsService.Application.Commands.RemoveTransaction;
using TransactionsService.Application.Commands.UpdateTransaction;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Queries.FindAllTransactions;
using TransactionsService.Application.Queries.FindTransactionById;

namespace TransactionsService.API.Controllers;

/// <summary>
/// TransactionsController — CRUD endpoints for credit card transactions.
/// </summary>
[ApiController]
[Route("api/transactions")]
[Authorize]
public sealed class TransactionsController(ISender mediator) : ControllerBase
{
    /// <summary>Returns all transactions with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? userId,
        [FromQuery] string? cardId,
        [FromQuery] string? transactionId,
        [FromQuery] string? cardName,
        [FromQuery] string? lastFourDigits,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 0,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new FindAllTransactionsQuery(userId, cardId, transactionId, cardName, lastFourDigits, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Returns a single transaction by its ID.</summary>
    [HttpGet("{cardId}/{transactionId}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string cardId, string transactionId,
        [FromQuery] string? userId,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new FindTransactionByIdQuery(cardId, transactionId, userId), ct);
        return Ok(result);
    }

    /// <summary>Creates a new transaction for the given card.</summary>
    [HttpPost("{cardId}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add(
        string cardId,
        [FromBody] AddTransactionCommand command,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(command with { CardId = cardId }, ct);
        return CreatedAtAction(nameof(GetById),
            new { cardId = result.CardId, transactionId = result.Id }, result);
    }

    /// <summary>Updates an existing transaction.</summary>
    [HttpPut("{cardId}/{transactionId}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        string cardId, string transactionId,
        [FromBody] UpdateTransactionCommand command,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(command with { CardId = cardId, TransactionId = transactionId }, ct);
        return Ok(result);
    }

    /// <summary>Physically deletes a transaction.</summary>
    [HttpDelete("{cardId}/{transactionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        string cardId, string transactionId,
        [FromQuery] string? userId,
        CancellationToken ct = default)
    {
        await mediator.Send(new RemoveTransactionCommand(cardId, transactionId, userId), ct);
        return NoContent();
    }
}
