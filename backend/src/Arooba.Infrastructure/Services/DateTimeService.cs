using DomainInterfaces = Arooba.Domain.Interfaces;
using AppInterfaces = Arooba.Application.Common.Interfaces;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Default implementation of the date-time service that returns the system UTC clock.
/// Implements both the Domain and Application layer contracts.
/// Replace with a testable wrapper in unit tests to control time-dependent logic
/// (escrow release calculations, subscription scheduling, etc.).
/// </summary>
public class DateTimeService : DomainInterfaces.IDateTimeService, AppInterfaces.IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time. Implements <see cref="DomainInterfaces.IDateTimeService.Now"/>.
    /// </summary>
    public DateTime Now => DateTime.UtcNow;

    /// <summary>
    /// Gets the current UTC date and time. Implements <see cref="AppInterfaces.IDateTimeService.UtcNow"/>.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
