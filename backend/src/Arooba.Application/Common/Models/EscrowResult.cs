namespace Arooba.Application.Common.Models;

/// <summary>
/// Result of an escrow release calculation for vendor payouts.
/// </summary>
/// <param name="DeliveryDate">The date the order was delivered.</param>
/// <param name="ReleaseDate">The date funds will be released from escrow.</param>
/// <param name="HoldDays">The number of days funds are held in escrow.</param>
/// <param name="IsReleased">Whether the escrow hold period has passed.</param>
public record EscrowResult(
    DateTime DeliveryDate,
    DateTime ReleaseDate,
    int HoldDays,
    bool IsReleased);
