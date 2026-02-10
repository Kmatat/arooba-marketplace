using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve all reviews submitted by a specific customer.
/// </summary>
public record GetCustomerReviewsQuery : IRequest<List<CustomerReviewDto>>
{
    /// <summary>Gets the customer identifier.</summary>
    public int CustomerId { get; init; }
}

/// <summary>
/// DTO representing a customer review with product and order context.
/// </summary>
public record CustomerReviewDto
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = default!;
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = default!;
    public string VendorName { get; init; } = default!;
    public int Rating { get; init; }
    public string? ReviewText { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public int HelpfulCount { get; init; }
    public string Status { get; init; } = default!;
    public string? AdminReply { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of customer reviews with product and order details.
/// </summary>
public class GetCustomerReviewsQueryHandler : IRequestHandler<GetCustomerReviewsQuery, List<CustomerReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerReviewDto>> Handle(
        GetCustomerReviewsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.CustomerReviews
            .AsNoTracking()
            .Where(r => r.CustomerId == request.CustomerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CustomerReviewDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                OrderId = r.OrderId,
                OrderNumber = r.Order != null ? r.Order.OrderNumber : "",
                ProductId = r.ProductId,
                ProductTitle = r.Product != null ? r.Product.Title : "",
                VendorName = r.Product != null && r.Product.ParentVendor != null
                    ? r.Product.ParentVendor.BusinessName
                    : "",
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                HelpfulCount = r.HelpfulCount,
                Status = r.Status.ToString(),
                AdminReply = r.AdminReply,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
