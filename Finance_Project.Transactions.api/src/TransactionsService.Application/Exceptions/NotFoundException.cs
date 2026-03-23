namespace TransactionsService.Application.Exceptions;

/// <summary>
/// NotFoundException
/// </summary>
/// <seealso cref="System.Exception" />
public sealed class NotFoundException(string message) : Exception(message);