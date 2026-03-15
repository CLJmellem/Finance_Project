using CardsService.API.Requests;
using CardsService.Application.Commands.CreateCard;
using CardsService.Application.Commands.DeleteCard;
using CardsService.Application.Commands.UpdateCard;
using CardsService.Application.DTOs;
using CardsService.Application.Queries.GetCardById;
using CardsService.Application.Queries.GetUserCards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardsService.API.Controllers;

/// <summary>
/// CardsController
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
[ApiController]
[Route("api/cards")]
[Authorize]
public sealed class CardsController(ISender mediator) : ControllerBase
{
    /// <summary>Gets all.</summary>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="inactiveCards">if set to <c>true</c> [inactive cards].</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 0,
        [FromQuery] string? userId = null,
        [FromQuery] bool inactiveCards = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetUserCardsQuery(page, pageSize, userId, inactiveCards), ct);
        return Ok(result);
    }

    /// <summary>Gets the card by identifier.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCardByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Creates the specified card command.</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCardCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates the specified card by the identifier.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id, [FromBody] UpdateCardRequest request, CancellationToken ct)
    {
        var command = new UpdateCardCommand(id, request.Name, request.CreditLimit, request.DueDay, request.IsActive);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Deletes the specified card by identifier.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCardCommand(id), ct);
        return NoContent();
    }
}