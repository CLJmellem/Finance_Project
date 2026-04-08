namespace Auth.Domain.Exceptions;

/// <summary>
/// InvalidUserDataException
/// </summary>
/// <seealso cref="System.Exception" />
public class InvalidUserDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidUserDataException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidUserDataException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidUserDataException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
    public InvalidUserDataException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}