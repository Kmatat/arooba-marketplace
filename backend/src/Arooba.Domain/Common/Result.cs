namespace Arooba.Domain.Common;

/// <summary>
/// Represents the outcome of a domain operation, encapsulating success or failure
/// along with an optional error message. Use <see cref="Result{T}"/> when a return value is needed.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message when the operation failed; otherwise <c>null</c>.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error message if the operation failed.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="isSuccess"/> is <c>true</c> but an error is provided,
    /// or when <paramref name="isSuccess"/> is <c>false</c> but no error is provided.
    /// </exception>
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("A successful result cannot have an error message.", nameof(error));

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("A failed result must have an error message.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with no return value.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">A description of what went wrong.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a successful result carrying a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to return.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result for a value-carrying result type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">A description of what went wrong.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Represents the outcome of a domain operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value returned by a successful operation; otherwise the default value of <typeparamref name="T"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The value returned on success.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error message if the operation failed.</param>
    internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
