using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Finance.Queries;

/// <summary>
/// Query to retrieve the 5-bucket transaction split for a specific order.
/// </summary>
public record GetTransactionSplitQuery : IRequest<List<TransactionSplitDto>>
{
    /// <summary>Gets the order identifier.</summary>
    public int OrderId { get; init; }
}

/// <summary>
/// DTO representing a single transaction split (5-bucket breakdown) for an order item.
/// </summary>
public record TransactionSplitDto
{
    /// <summary>Gets the split identifier.</summary>
    public int Id { get; init; }

    /// <summary>Gets the order identifier.</summary>
    public int OrderId { get; init; }

    /// <summary>Gets the order item identifier.</summary>
    public int OrderItemId { get; init; }

    /// <summary>Gets the product identifier.</summary>
    public int ProductId { get; init; }

    /// <summary>Gets the parent vendor identifier.</summary>
    public int ParentVendorId { get; init; }

    /// <summary>Gets the sub-vendor identifier, if applicable.</summary>
    public int? SubVendorId { get; init; }

    /// <summary>Gets the gross transaction amount in EGP.</summary>
    public decimal GrossAmount { get; init; }

    /// <summary>Gets the vendor payout bucket in EGP.</summary>
    public decimal VendorPayoutBucket { get; init; }

    /// <summary>Gets the Arooba commission bucket in EGP.</summary>
    public decimal AroobaBucket { get; init; }

    /// <summary>Gets the VAT bucket in EGP.</summary>
    public decimal VatBucket { get; init; }

    /// <summary>Gets the parent uplift bucket in EGP.</summary>
    public decimal ParentUpliftBucket { get; init; }

    /// <summary>Gets the withholding tax bucket in EGP.</summary>
    public decimal WithholdingTaxBucket { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of all transaction splits for an order.
/// </summary>
public class GetTransactionSplitQueryHandler : IRequestHandler<GetTransactionSplitQuery, List<TransactionSplitDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetTransactionSplitQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetTransactionSplitQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all 5-bucket transaction splits for the specified order.
    /// </summary>
    /// <param name="request">The query containing the order ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of transaction split DTOs for the order.</returns>
    /// <exception cref="NotFoundException">Thrown when the order has no transaction splits.</exception>
    public async Task<List<TransactionSplitDto>> Handle(
        GetTransactionSplitQuery request,
        CancellationToken cancellationToken)
    {
        var splits = await _context.TransactionSplits
            .Where(ts => ts.OrderId == request.OrderId)
            .AsNoTracking()
            .OrderBy(ts => ts.CreatedAt)
            .Select(ts => new TransactionSplitDto
            {
                Id = ts.Id,
                OrderId = ts.OrderId,
                OrderItemId = ts.OrderItemId,
                ProductId = ts.ProductId,
                ParentVendorId = ts.ParentVendorId,
                SubVendorId = ts.SubVendorId,
                GrossAmount = ts.GrossAmount,
                VendorPayoutBucket = ts.VendorPayoutBucket,
                AroobaBucket = ts.AroobaBucket,
                VatBucket = ts.VatBucket,
                ParentUpliftBucket = ts.ParentUpliftBucket,
                WithholdingTaxBucket = ts.WithholdingTaxBucket,
                CreatedAt = ts.CreatedAt
            })
            .ToListAsync(cancellationToken);

        if (splits.Count == 0)
        {
            throw new NotFoundException("TransactionSplits for Order", request.OrderId);
        }

        return splits;
    }
}
