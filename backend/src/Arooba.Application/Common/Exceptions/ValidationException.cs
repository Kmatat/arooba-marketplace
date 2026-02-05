using FluentValidation.Results;

namespace Arooba.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when one or more FluentValidation rules fail during request processing.
/// Wraps validation failures into a dictionary keyed by property name.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValidationException"/> with default message.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationException"/> from a collection
    /// of FluentValidation <see cref="ValidationFailure"/> objects.
    /// </summary>
    /// <param name="failures">The collection of validation failures.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Gets the dictionary of validation errors keyed by property name.
    /// Each key maps to an array of error messages for that property.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}
