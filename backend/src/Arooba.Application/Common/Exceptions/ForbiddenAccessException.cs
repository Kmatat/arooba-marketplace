namespace Arooba.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when the current user does not have permission to perform the requested operation.
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenAccessException"/> with a default message.
    /// </summary>
    public ForbiddenAccessException()
        : base("Access to this resource is forbidden.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenAccessException"/> with a custom message.
    /// </summary>
    /// <param name="message">The error message describing the forbidden access.</param>
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenAccessException"/> with a custom message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
