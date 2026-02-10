using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Customers.Commands;

/// <summary>
/// Command to submit a product review from a customer.
/// </summary>
public record SubmitReviewCommand : IRequest<int>
{
    /// <summary>Gets the customer identifier.</summary>
    public int CustomerId { get; init; }

    /// <summary>Gets the order identifier.</summary>
    public int OrderId { get; init; }

    /// <summary>Gets the product identifier.</summary>
    public int ProductId { get; init; }

    /// <summary>Gets the star rating (1-5).</summary>
    public int Rating { get; init; }

    /// <summary>Gets the optional review text.</summary>
    public string? ReviewText { get; init; }
}

/// <summary>
/// Handles submission of a customer review.
/// </summary>
public class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    public SubmitReviewCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<int> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        // Verify order belongs to customer
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CustomerId == request.CustomerId, cancellationToken);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Check for duplicate review
        var existingReview = await _context.CustomerReviews
            .AnyAsync(r => r.CustomerId == request.CustomerId
                        && r.OrderId == request.OrderId
                        && r.ProductId == request.ProductId, cancellationToken);
        if (existingReview)
            throw new BadRequestException("A review for this product on this order already exists.");

        var reviewId = new int();
        var review = new CustomerReview
        {
            Id = reviewId,
            CustomerId = request.CustomerId,
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            ReviewText = request.ReviewText,
            IsVerifiedPurchase = true,
            Status = ReviewStatus.Published,
            CreatedAt = _dateTime.UtcNow,
        };

        _context.CustomerReviews.Add(review);

        // Update customer review stats
        var allRatings = await _context.CustomerReviews
            .Where(r => r.CustomerId == request.CustomerId)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);
        allRatings.Add(request.Rating);

        customer.TotalReviews = allRatings.Count;
        customer.AverageRating = Math.Round((decimal)allRatings.Average(), 1);

        await _context.SaveChangesAsync(cancellationToken);

        return reviewId;
    }
}

/// <summary>
/// Validates the <see cref="SubmitReviewCommand"/>.
/// </summary>
public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(r => r.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(r => r.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(r => r.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(r => r.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(r => r.ReviewText)
            .MaximumLength(2000)
            .When(r => r.ReviewText is not null)
            .WithMessage("Review text must not exceed 2000 characters.");
    }
}
