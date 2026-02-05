using System.Text.RegularExpressions;

namespace Arooba.Domain.ValueObjects;

/// <summary>
/// Represents a validated Egyptian phone number. Enforces the <c>+20</c> country code format.
/// Immutable value object.
/// </summary>
public sealed partial record PhoneNumber
{
    /// <summary>
    /// Regular expression pattern for validating Egyptian phone numbers.
    /// Accepts +20 followed by 10 digits (starting with 1).
    /// </summary>
    private const string EgyptianPhonePattern = @"^\+20[1][0-9]{9}$";

    /// <summary>
    /// Gets the validated phone number string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new <see cref="PhoneNumber"/> instance after validating the Egyptian format.
    /// </summary>
    /// <param name="value">The phone number string in <c>+20XXXXXXXXXX</c> format.</param>
    /// <exception cref="ArgumentException">Thrown when the phone number format is invalid.</exception>
    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number must not be empty.", nameof(value));

        if (!EgyptianPhoneRegex().IsMatch(value))
            throw new ArgumentException(
                $"'{value}' is not a valid Egyptian phone number. Expected format: +201XXXXXXXXX",
                nameof(value));

        Value = value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    [GeneratedRegex(EgyptianPhonePattern)]
    private static partial Regex EgyptianPhoneRegex();
}
