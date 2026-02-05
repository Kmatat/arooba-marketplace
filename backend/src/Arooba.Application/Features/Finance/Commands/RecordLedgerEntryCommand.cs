using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Finance.Commands;

/// <summary>
/// Command to manually record a financial ledger entry for a vendor.
/// Used for adjustments, refunds, or manual corrections.
/// </summary>
public record RecordLedgerEntryCommand : IRequest<Guid>
{
    /// <summary>Gets the vendor identifier.</summary>
    public Guid VendorId { get; init; }

    /// <summary>Gets the optional related order identifier.</summary>
    public Guid? OrderId { get; init; }

    /// <summary>Gets the transaction type.</summary>
    public TransactionType TransactionType { get; init; }

    /// <summary>Gets the total transaction amount in EGP (positive for credits, negative for debits).</summary>
    public decimal Amount { get; init; }

    /// <summary>Gets the vendor's portion of the amount.</summary>
    public decimal VendorAmount { get; init; }

    /// <summary>Gets the commission portion.</summary>
    public decimal CommissionAmount { get; init; }

    /// <summary>Gets the VAT portion.</summary>
    public decimal VatAmount { get; init; }

    /// <summary>Gets a description of the transaction.</summary>
    public string Description { get; init; } = default!;

    /// <summary>Gets the balance status for this entry.</summary>
    public BalanceStatus BalanceStatus { get; init; }
}

/// <summary>
/// Handles recording a manual ledger entry and updating the vendor wallet accordingly.
/// </summary>
public class RecordLedgerEntryCommandHandler : IRequestHandler<RecordLedgerEntryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="RecordLedgerEntryCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public RecordLedgerEntryCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Creates a ledger entry and updates the vendor wallet balance based on
    /// the transaction type and balance status.
    /// </summary>
    /// <param name="request">The record ledger entry command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the created ledger entry.</returns>
    /// <exception cref="NotFoundException">Thrown when the vendor wallet is not found.</exception>
    public async Task<Guid> Handle(RecordLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _context.VendorWallets
            .FirstOrDefaultAsync(w => w.ParentVendorId == request.VendorId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(VendorWallet), request.VendorId);
        }

        var now = _dateTime.UtcNow;
        var ledgerEntryId = Guid.NewGuid();

        var ledgerEntry = new LedgerEntry
        {
            Id = ledgerEntryId,
            ParentVendorId = request.VendorId,
            OrderId = request.OrderId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            VendorAmount = request.VendorAmount,
            CommissionAmount = request.CommissionAmount,
            VatAmount = request.VatAmount,
            Description = request.Description,
            BalanceStatus = request.BalanceStatus,
            CreatedAt = now
        };

        // Update wallet based on balance status
        switch (request.BalanceStatus)
        {
            case BalanceStatus.Pending:
                wallet.PendingBalance += request.VendorAmount;
                if (request.VendorAmount > 0)
                {
                    wallet.TotalEarnings += request.VendorAmount;
                }
                break;

            case BalanceStatus.Available:
                wallet.AvailableBalance += request.VendorAmount;
                if (request.VendorAmount > 0)
                {
                    wallet.TotalEarnings += request.VendorAmount;
                }
                break;

            case BalanceStatus.Withdrawn:
                wallet.AvailableBalance -= Math.Abs(request.VendorAmount);
                wallet.TotalPayouts += Math.Abs(request.VendorAmount);
                break;
        }

        _context.LedgerEntries.Add(ledgerEntry);

        await _context.SaveChangesAsync(cancellationToken);

        return ledgerEntryId;
    }
}

/// <summary>
/// Validates the <see cref="RecordLedgerEntryCommand"/>.
/// </summary>
public class RecordLedgerEntryCommandValidator : AbstractValidator<RecordLedgerEntryCommand>
{
    /// <summary>
    /// Initializes validation rules for recording a ledger entry.
    /// </summary>
    public RecordLedgerEntryCommandValidator()
    {
        RuleFor(l => l.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required.");

        RuleFor(l => l.TransactionType)
            .IsInEnum().WithMessage("A valid transaction type is required.");

        RuleFor(l => l.Amount)
            .NotEqual(0).WithMessage("Amount cannot be zero.");

        RuleFor(l => l.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(l => l.BalanceStatus)
            .IsInEnum().WithMessage("A valid balance status is required.");
    }
}
