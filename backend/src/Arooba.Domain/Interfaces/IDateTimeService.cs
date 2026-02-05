namespace Arooba.Domain.Interfaces;

/// <summary>
/// Provides an abstraction over the system clock.
/// Enables deterministic testing by allowing the current time to be controlled.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime Now { get; }
}
