using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Finance.Queries;

/// <summary>
/// Query to retrieve a vendor's wallet with balance breakdown.
/// </summary>
public record GetVendorWalletQuery : IRequest<VendorWalletDto>
{
    /// <summary>Gets the vendor identifier.</summary>
    public Guid VendorId { get; init; }
}

/// <summary>
/// DTO representing a vendor's wallet with complete balance breakdown.
/// </summary>
public record VendorWalletDto
{
    /// <summary>Gets the wallet identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the parent vendor identifier.</summary>
    public Guid ParentVendorId { get; init; }

    /// <summary>Gets the vendor's business name.</summary>
    public string VendorBusinessName { get; init; } = default!;

    /// <summary>Gets the pending balance in EGP (funds in escrow).</summary>
    public decimal PendingBalance { get; init; }

    /// <summary>Gets the available balance in EGP (cleared for withdrawal).</summary>
    public decimal AvailableBalance { get; init; }

    /// <summary>Gets the total lifetime earnings in EGP.</summary>
    public decimal TotalEarnings { get; init; }

    /// <summary>Gets the total payouts withdrawn in EGP.</summary>
    public decimal TotalPayouts { get; init; }

    /// <summary>Gets the total balance (pending + available).</summary>
    public decimal TotalBalance => PendingBalance + AvailableBalance;

    /// <summary>Gets the wallet creation date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of a vendor's wallet with balance breakdown.
/// </summary>
public class GetVendorWalletQueryHandler : IRequestHandler<GetVendorWalletQuery, VendorWalletDto>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetVendorWalletQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetVendorWalletQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves the vendor wallet and maps it to a DTO with balance breakdown.
    /// </summary>
    /// <param name="request">The query containing the vendor ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A vendor wallet DTO with complete balance information.</returns>
    /// <exception cref="NotFoundException">Thrown when the vendor wallet is not found.</exception>
    public async Task<VendorWalletDto> Handle(
        GetVendorWalletQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _context.VendorWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ParentVendorId == request.VendorId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(VendorWallet), request.VendorId);
        }

        var vendor = await _context.ParentVendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        return new VendorWalletDto
        {
            Id = wallet.VendorId,
            ParentVendorId = wallet.VendorId,
            VendorBusinessName = vendor?.BusinessName ?? string.Empty,
            PendingBalance = wallet.PendingBalance,
            AvailableBalance = wallet.AvailableBalance,
            TotalEarnings = wallet.TotalEarnings,
            TotalPayouts = wallet.TotalPayouts,
            CreatedAt = wallet.CreatedAt
        };
    }
}
