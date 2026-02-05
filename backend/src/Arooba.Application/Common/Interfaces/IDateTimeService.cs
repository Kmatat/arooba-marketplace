namespace Arooba.Application.Common.Interfaces;

/// <summary>
/// Provides an abstraction over system clock access, enabling deterministic testing
/// of time-dependent business logic (escrow release, subscription scheduling, etc.).
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
