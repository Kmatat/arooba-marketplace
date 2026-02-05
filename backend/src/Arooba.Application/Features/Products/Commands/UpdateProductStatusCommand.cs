using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Products.Commands;

/// <summary>
/// Command to change a product's status through its lifecycle
/// (approve, reject, pause, or reactivate).
/// </summary>
public record UpdateProductStatusCommand : IRequest<bool>
{
    /// <summary>Gets the product identifier.</summary>
    public Guid ProductId { get; init; }

    /// <summary>Gets the new status to apply.</summary>
    public ProductStatus NewStatus { get; init; }

    /// <summary>Gets an optional reason for the status change (e.g., rejection reason).</summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Handles product status transitions with business rule validation.
/// </summary>
public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateProductStatusCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public UpdateProductStatusCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Validates the status transition and applies the new status.
    /// </summary>
    /// <param name="request">The update status command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the status was updated successfully.</returns>
    /// <exception cref="NotFoundException">Thrown when the product is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when the status transition is not allowed.</exception>
    public async Task<bool> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        // Validate allowed status transitions
        var isValidTransition = (product.Status, request.NewStatus) switch
        {
            (ProductStatus.Draft, ProductStatus.PendingReview) => true,
            (ProductStatus.PendingReview, ProductStatus.Active) => true,      // Approve
            (ProductStatus.PendingReview, ProductStatus.Rejected) => true,    // Reject
            (ProductStatus.Active, ProductStatus.Paused) => true,             // Pause
            (ProductStatus.Paused, ProductStatus.Active) => true,             // Reactivate
            (ProductStatus.Rejected, ProductStatus.PendingReview) => true,    // Resubmit
            _ => false
        };

        if (!isValidTransition)
        {
            throw new BadRequestException(
                $"Cannot transition product from {product.Status} to {request.NewStatus}.");
        }

        product.Status = request.NewStatus;
        product.StatusReason = request.Reason;
        product.UpdatedAt = _dateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Validates the <see cref="UpdateProductStatusCommand"/>.
/// </summary>
public class UpdateProductStatusCommandValidator : AbstractValidator<UpdateProductStatusCommand>
{
    /// <summary>
    /// Initializes validation rules for product status updates.
    /// </summary>
    public UpdateProductStatusCommandValidator()
    {
        RuleFor(p => p.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(p => p.NewStatus)
            .IsInEnum().WithMessage("A valid product status is required.");

        RuleFor(p => p.Reason)
            .NotEmpty()
            .When(p => p.NewStatus == ProductStatus.Rejected)
            .WithMessage("A reason is required when rejecting a product.");
    }
}
