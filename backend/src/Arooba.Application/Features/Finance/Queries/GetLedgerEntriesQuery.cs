using Arooba.Application.Common.Interfaces;
using Arooba.Application.Common.Models;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Finance.Queries;

/// <summary>
/// Query to retrieve paginated ledger entries for a specific vendor.
/// </summary>
public record GetLedgerEntriesQuery : IRequest<PaginatedList<LedgerEntryDto>>
{
    /// <summary>Gets the vendor identifier.</summary>
    public Guid VendorId { get; init; }

    /// <summary>Gets the page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the page size. Defaults to 20.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Gets an optional transaction type filter.</summary>
    public TransactionType? TransactionType { get; init; }

    /// <summary>Gets an optional balance status filter.</summary>
    public BalanceStatus? BalanceStatus { get; init; }

    /// <summary>Gets an optional start date filter.</summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>Gets an optional end date filter.</summary>
    public DateTime? DateTo { get; init; }
}

/// <summary>
/// DTO representing a financial ledger entry.
/// </summary>
public record LedgerEntryDto
{
    /// <summary>Gets the ledger entry identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the vendor identifier.</summary>
    public Guid ParentVendorId { get; init; }

    /// <summary>Gets the related order identifier, if applicable.</summary>
    public Guid? OrderId { get; init; }

    /// <summary>Gets the transaction type.</summary>
    public TransactionType TransactionType { get; init; }

    /// <summary>Gets the total amount in EGP.</summary>
    public decimal Amount { get; init; }

    /// <summary>Gets the vendor's portion in EGP.</summary>
    public decimal VendorAmount { get; init; }

    /// <summary>Gets the commission portion in EGP.</summary>
    public decimal CommissionAmount { get; init; }

    /// <summary>Gets the VAT portion in EGP.</summary>
    public decimal VatAmount { get; init; }

    /// <summary>Gets the transaction description.</summary>
    public string Description { get; init; } = default!;

    /// <summary>Gets the balance status.</summary>
    public BalanceStatus BalanceStatus { get; init; }

    /// <summary>Gets the entry creation date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handles the paginated ledger entries query with filtering.
/// </summary>
public class GetLedgerEntriesQueryHandler : IRequestHandler<GetLedgerEntriesQuery, PaginatedList<LedgerEntryDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetLedgerEntriesQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetLedgerEntriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves paginated ledger entries for the specified vendor with optional filtering.
    /// </summary>
    /// <param name="request">The query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A paginated list of ledger entry DTOs.</returns>
    public async Task<PaginatedList<LedgerEntryDto>> Handle(
        GetLedgerEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.LedgerEntries
            .Where(le => le.ParentVendorId == request.VendorId)
            .AsNoTracking()
            .AsQueryable();

        if (request.TransactionType.HasValue)
        {
            query = query.Where(le => le.TransactionType == request.TransactionType.Value);
        }

        if (request.BalanceStatus.HasValue)
        {
            query = query.Where(le => le.BalanceStatus == request.BalanceStatus.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(le => le.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(le => le.CreatedAt <= request.DateTo.Value);
        }

        var projectedQuery = query
            .OrderByDescending(le => le.CreatedAt)
            .Select(le => new LedgerEntryDto
            {
                Id = le.Id,
                ParentVendorId = le.ParentVendorId,
                OrderId = le.OrderId,
                TransactionType = le.TransactionType,
                Amount = le.Amount,
                VendorAmount = le.VendorAmount,
                CommissionAmount = le.CommissionAmount,
                VatAmount = le.VatAmount,
                Description = le.Description,
                BalanceStatus = le.BalanceStatus,
                CreatedAt = le.CreatedAt
            });

        return await PaginatedList<LedgerEntryDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
