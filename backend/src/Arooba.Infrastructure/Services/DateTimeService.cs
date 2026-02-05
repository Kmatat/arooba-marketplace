using Arooba.Application.Common.Interfaces;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Default implementation of <see cref="IDateTimeService"/> that returns the system UTC clock.
/// Replace with a testable wrapper in unit tests to control time-dependent logic
/// (escrow release calculations, subscription scheduling, etc.).
/// </summary>
public class DateTimeService : IDateTimeService
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
