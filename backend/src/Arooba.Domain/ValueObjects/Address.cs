namespace Arooba.Domain.ValueObjects;

/// <summary>
/// Represents a physical delivery or pickup address within Egypt.
/// Immutable value object.
/// </summary>
public sealed record Address
{
    /// <summary>
    /// Gets the full street-level address.
    /// </summary>
    public string FullAddress { get; }

    /// <summary>
    /// Gets the city name.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the shipping zone identifier used for logistics rate calculation.
    /// </summary>
    public string ZoneId { get; }

    /// <summary>
    /// Initializes a new <see cref="Address"/> instance.
    /// </summary>
    /// <param name="fullAddress">The full street-level address.</param>
    /// <param name="city">The city name.</param>
    /// <param name="zoneId">The shipping zone identifier.</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or whitespace.</exception>
    public Address(string fullAddress, string city, string zoneId)
    {
        if (string.IsNullOrWhiteSpace(fullAddress))
            throw new ArgumentException("Full address must not be empty.", nameof(fullAddress));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City must not be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(zoneId))
            throw new ArgumentException("Zone ID must not be empty.", nameof(zoneId));

        FullAddress = fullAddress;
        City = city;
        ZoneId = zoneId;
    }

    /// <inheritdoc />
    public override string ToString() => $"{FullAddress}, {City} (Zone: {ZoneId})";
}
