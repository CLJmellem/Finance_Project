namespace CardsService.Application.Exceptions;

/// <summary>
/// DomainException
/// </summary>
/// <seealso cref="System.Exception" />
public class DomainException(string message) : Exception(message);
