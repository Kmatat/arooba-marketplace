using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Finance.Commands;

/// <summary>
/// Command to process a vendor payout from their available wallet balance.
/// Enforces the minimum payout threshold of 500 EGP as per Arooba's financial policy.
/// </summary>
public record ProcessPayoutCommand : IRequest<Guid>
{
    /// <summary>Gets the vendor identifier to process the payout for.</summary>
    public Guid VendorId { get; init; }

    /// <summary>Gets the payout amount in EGP.</summary>
    public decimal Amount { get; init; }

    /// <summary>Gets an optional note for the payout.</summary>
    public string? Note { get; init; }
}

/// <summary>
/// Handles vendor payout processing with minimum threshold and balance validation.
/// </summary>
public class ProcessPayoutCommandHandler : IRequestHandler<ProcessPayoutCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// The minimum payout amount in EGP.
    /// </summary>
    private const decimal MinimumPayoutThreshold = 500m;

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessPayoutCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public ProcessPayoutCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Validates the payout amount against the minimum threshold and available balance,
    /// deducts from the wallet, records a ledger entry, and returns the ledger entry ID.
    /// </summary>
    /// <param name="request">The process payout command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the created payout ledger entry.</returns>
    /// <exception cref="NotFoundException">Thrown when the vendor wallet is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when payout amount is below threshold or exceeds available balance.</exception>
    public async Task<Guid> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _context.VendorWallets
            .FirstOrDefaultAsync(w => w.ParentVendorId == request.VendorId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(VendorWallet), request.VendorId);
        }

        // Validate minimum payout threshold
        if (request.Amount < MinimumPayoutThreshold)
        {
            throw new BadRequestException(
                $"Payout amount must be at least {MinimumPayoutThreshold:N2} EGP. " +
                $"Requested: {request.Amount:N2} EGP.");
        }

        // Validate available balance
        if (wallet.AvailableBalance < request.Amount)
        {
            throw new BadRequestException(
                $"Insufficient available balance. " +
                $"Available: {wallet.AvailableBalance:N2} EGP, Requested: {request.Amount:N2} EGP.");
        }

        var now = _dateTime.UtcNow;
        var ledgerEntryId = Guid.NewGuid();

        // Deduct from wallet
        wallet.AvailableBalance -= request.Amount;
        wallet.TotalPayouts += request.Amount;

        // Record payout ledger entry
        var ledgerEntry = new LedgerEntry
        {
            Id = ledgerEntryId,
            ParentVendorId = request.VendorId,
            TransactionType = TransactionType.Payout,
            Amount = -request.Amount,
            VendorAmount = -request.Amount,
            CommissionAmount = 0m,
            VatAmount = 0m,
            Description = request.Note ?? $"Vendor payout of {request.Amount:N2} EGP",
            BalanceStatus = BalanceStatus.Withdrawn,
            CreatedAt = now
        };

        _context.LedgerEntries.Add(ledgerEntry);

        await _context.SaveChangesAsync(cancellationToken);

        return ledgerEntryId;
    }
}

/// <summary>
/// Validates the <see cref="ProcessPayoutCommand"/>.
/// </summary>
public class ProcessPayoutCommandValidator : AbstractValidator<ProcessPayoutCommand>
{
    /// <summary>
    /// Initializes validation rules for payout processing.
    /// </summary>
    public ProcessPayoutCommandValidator()
    {
        RuleFor(p => p.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required.");

        RuleFor(p => p.Amount)
            .GreaterThan(0).WithMessage("Payout amount must be greater than zero.");

        RuleFor(p => p.Note)
            .MaximumLength(500)
            .When(p => p.Note is not null)
            .WithMessage("Payout note must not exceed 500 characters.");
    }
}
