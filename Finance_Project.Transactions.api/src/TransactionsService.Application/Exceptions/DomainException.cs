namespace TransactionsService.Application.Exceptions;

/// <summary>
/// DomainException
/// </summary>
/// <seealso cref="System.Exception" />
public sealed class DomainException(string message) : Exception(message);