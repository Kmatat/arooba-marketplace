namespace Arooba.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a request contains invalid data or violates business rules
/// that do not fall under standard validation.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with a default message.
    /// </summary>
    public BadRequestException()
        : base("A bad request was made.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with a custom message.
    /// </summary>
    /// <param name="message">The error message describing what was wrong with the request.</param>
    public BadRequestException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with a custom message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
