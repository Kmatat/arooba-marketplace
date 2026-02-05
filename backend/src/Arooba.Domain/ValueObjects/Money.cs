namespace Arooba.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with its associated currency.
/// Defaults to Egyptian Pound (EGP). Immutable value object with arithmetic operator support.
/// </summary>
public sealed record Money
{
    /// <summary>
    /// Gets the numeric amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the ISO 4217 currency code. Defaults to <c>"EGP"</c>.
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Initializes a new <see cref="Money"/> instance.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The ISO 4217 currency code. Defaults to <c>"EGP"</c>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="currency"/> is null or whitespace.</exception>
    public Money(decimal amount, string currency = "EGP")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code must not be empty.", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a <see cref="Money"/> instance representing zero in the specified currency.
    /// </summary>
    /// <param name="currency">The currency code. Defaults to <c>"EGP"</c>.</param>
    /// <returns>A zero-amount <see cref="Money"/> value.</returns>
    public static Money Zero(string currency = "EGP") => new(0m, currency);

    /// <summary>
    /// Adds two <see cref="Money"/> values with the same currency.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when currencies do not match.</exception>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>
    /// Subtracts one <see cref="Money"/> value from another with the same currency.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when currencies do not match.</exception>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>
    /// Multiplies a <see cref="Money"/> value by a scalar factor.
    /// </summary>
    /// <param name="money">The monetary value.</param>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>A new <see cref="Money"/> instance with the multiplied amount.</returns>
    public static Money operator *(Money money, decimal factor)
    {
        return new Money(money.Amount * factor, money.Currency);
    }

    /// <summary>
    /// Multiplies a scalar factor by a <see cref="Money"/> value.
    /// </summary>
    /// <param name="factor">The multiplication factor.</param>
    /// <param name="money">The monetary value.</param>
    /// <returns>A new <see cref="Money"/> instance with the multiplied amount.</returns>
    public static Money operator *(decimal factor, Money money)
    {
        return new Money(money.Amount * factor, money.Currency);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Amount:N2} {Currency}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Cannot perform arithmetic on different currencies: {left.Currency} and {right.Currency}.");
    }
}
