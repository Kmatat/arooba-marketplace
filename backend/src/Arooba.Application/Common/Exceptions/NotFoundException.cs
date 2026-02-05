namespace Arooba.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity cannot be found in the data store.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with default message.
    /// </summary>
    public NotFoundException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with the entity name and key
    /// that could not be found.
    /// </summary>
    /// <param name="name">The name of the entity type.</param>
    /// <param name="key">The key value that was searched for.</param>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
