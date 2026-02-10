using Arooba.Application.Common.Models;
using Arooba.Application.Features.Finance.Commands;
using Arooba.Application.Features.Finance.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Manages the financial subsystem of the Arooba Marketplace including vendor wallets,
/// ledger entries, transaction splits (5-bucket model), and payout processing.
/// All monetary values are in Egyptian Pounds (EGP).
/// </summary>
[Authorize]
public class FinanceController : ApiControllerBase
{
    /// <summary>
    /// Retrieves the wallet balance summary for a specific vendor.
    /// </summary>
    /// <remarks>
    /// The wallet shows:
    /// - Total balance (all funds associated with the vendor)
    /// - Pending balance (funds in 14-day escrow hold)
    /// - Available balance (withdrawable amount above minimum payout threshold of 500 EGP)
    /// - Lifetime earnings (cumulative total since account creation)
    /// </remarks>
    /// <param name="vendorId">The unique identifier of the vendor.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The vendor wallet with balance breakdown.</returns>
    /// <response code="200">Wallet retrieved successfully.</response>
    /// <response code="404">Vendor with the specified identifier was not found.</response>
    [HttpGet("wallets/{vendorId:int}")]
    [ProducesResponseType(typeof(VendorWalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVendorWallet(
        int vendorId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVendorWalletQuery() { VendorId = vendorId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves paginated ledger entries (financial transaction history) for a specific vendor.
    /// </summary>
    /// <remarks>
    /// Ledger entries track every financial movement: sales, commissions, VAT, shipping fees,
    /// refunds, and payouts. Each entry includes a running balance.
    /// </remarks>
    /// <param name="vendorId">The unique identifier of the vendor.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated list of ledger entries ordered by date descending.</returns>
    /// <response code="200">Ledger entries retrieved successfully.</response>
    /// <response code="404">Vendor with the specified identifier was not found.</response>
    [HttpGet("ledger/{vendorId:int}")]
    [ProducesResponseType(typeof(PaginatedList<LedgerEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLedgerEntries(
        int vendorId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLedgerEntriesQuery
        {
            VendorId = vendorId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the 5-bucket transaction splits for all items in a specific order.
    /// </summary>
    /// <remarks>
    /// Each order item is split into five financial buckets:
    /// <list type="bullet">
    ///   <item>Bucket A: Vendor revenue (cost price x quantity)</item>
    ///   <item>Bucket B: Vendor VAT (14% of Bucket A)</item>
    ///   <item>Bucket C: Arooba revenue (marketplace uplift + cooperative fee)</item>
    ///   <item>Bucket D: Arooba VAT (14% of Bucket C)</item>
    ///   <item>Bucket E: Logistics/delivery fee</item>
    /// </list>
    /// </remarks>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of transaction splits, one per order item.</returns>
    /// <response code="200">Transaction splits retrieved successfully.</response>
    /// <response code="404">Order with the specified identifier was not found.</response>
    [HttpGet("splits/{orderId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionSplitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionSplits(
        int orderId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTransactionSplitQuery() { OrderId = orderId}, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Processes a payout from a vendor's available balance to their registered bank account.
    /// </summary>
    /// <remarks>
    /// Payout rules:
    /// - Available balance must be at or above the minimum payout threshold (500 EGP).
    /// - Only funds that have cleared the 14-day escrow period are eligible.
    /// - A ledger entry is created for the payout transaction.
    /// - The vendor must have bank account details on file.
    /// </remarks>
    /// <param name="command">
    /// The payout request including vendor identifier and amount (or full available balance).
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The identifier of the payout transaction.</returns>
    /// <response code="200">Payout processed successfully.</response>
    /// <response code="400">Insufficient balance, below minimum threshold, or missing bank details.</response>
    /// <response code="404">Vendor with the specified identifier was not found.</response>
    [HttpPost("payouts")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessPayout(
        [FromBody] ProcessPayoutCommand command,
        CancellationToken cancellationToken)
    {
        var payoutId = await Sender.Send(command, cancellationToken);
        return Ok(payoutId);
    }
}
