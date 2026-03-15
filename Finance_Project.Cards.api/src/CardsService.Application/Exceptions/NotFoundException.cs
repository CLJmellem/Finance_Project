namespace CardsService.Application.Exceptions;

/// <summary>
/// NotFoundException
/// </summary>
/// <seealso cref="System.Exception" />
public class NotFoundException(string message) : Exception(message);