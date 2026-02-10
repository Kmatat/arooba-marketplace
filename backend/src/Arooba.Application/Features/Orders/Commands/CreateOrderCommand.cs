using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Orders.Commands;

/// <summary>
/// Represents a single line item in an order creation request.
/// </summary>
public record OrderItemRequest
{
    /// <summary>Gets the product identifier to order.</summary>
    public int ProductId { get; init; }

    /// <summary>Gets the quantity to order.</summary>
    public int Quantity { get; init; }
}

/// <summary>
/// Command to create a new customer order on the Arooba Marketplace.
/// This is the most complex handler: it validates stock availability,
/// calculates the 5-bucket financial split for each line item,
/// groups items into shipments by pickup location, and persists the complete order.
/// </summary>
public record CreateOrderCommand : IRequest<int>
{
    /// <summary>Gets the customer placing the order.</summary>
    public int CustomerId { get; init; }

    /// <summary>Gets the list of products and quantities to order.</summary>
    public List<OrderItemRequest> Items { get; init; } = new();

    /// <summary>Gets the payment method for this order.</summary>
    public PaymentMethod PaymentMethod { get; init; }

    /// <summary>Gets the delivery street address.</summary>
    public string DeliveryAddress { get; init; } = default!;

    /// <summary>Gets the delivery city.</summary>
    public string DeliveryCity { get; init; } = default!;

    /// <summary>Gets the delivery shipping zone identifier (e.g., "cairo", "alexandria").</summary>
    public string DeliveryZoneId { get; init; } = string.Empty;
}

/// <summary>
/// Handles the complete order creation workflow including stock validation,
/// 5-bucket pricing split, shipment grouping, and financial record creation.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateOrderCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="dateTime">The date/time service.</param>
    public CreateOrderCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Creates a complete order with the following steps:
    /// 1. Validates the customer exists
    /// 2. Validates the delivery zone exists
    /// 3. Loads and validates all products (stock, status, zone restrictions)
    /// 4. Creates order items with 5-bucket financial split
    /// 5. Groups items into shipments by pickup location
    /// 6. Records transaction splits and ledger entries
    /// 7. Updates vendor wallet pending balances
    /// 8. Decrements product stock
    /// </summary>
    /// <param name="request">The create order command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the newly created order.</returns>
    /// <exception cref="NotFoundException">Thrown when customer, product, or zone is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when stock is insufficient or product is unavailable.</exception>
    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTime.UtcNow;

        // Step 1: Validate customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        // Step 2: Validate delivery zone (string-based ID like "cairo")
        var deliveryZone = await _context.ShippingZones
            .FirstOrDefaultAsync(z => z.Id == request.DeliveryZoneId, cancellationToken);

        if (deliveryZone is null)
        {
            throw new NotFoundException(nameof(ShippingZone), request.DeliveryZoneId);
        }
        var zoneId = deliveryZone.Id; // string-based zone ID

        // Step 3: Load and validate all requested products
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Include(p => p.ParentVendor)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            var foundIds = products.Select(p => p.Id).ToHashSet();
            var missingIds = productIds.Where(id => !foundIds.Contains(id));
            throw new NotFoundException(nameof(Product), string.Join(", ", missingIds));
        }

        // Validate product availability and stock
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            if (product.Status != ProductStatus.Active)
            {
                throw new BadRequestException(
                    $"Product '{product.Title}' (SKU: {product.Sku}) is not available for purchase. Current status: {product.Status}.");
            }

            if (product.StockMode == StockMode.ReadyStock && product.QuantityAvailable < item.Quantity)
            {
                throw new BadRequestException(
                    $"Insufficient stock for product '{product.Title}' (SKU: {product.Sku}). " +
                    $"Requested: {item.Quantity}, Available: {product.QuantityAvailable}.");
            }

            if (product.IsLocalOnly)
            {
                // For local-only products, ensure delivery zone matches product zone
                var pickupLocation = await _context.PickupLocations
                    .FirstOrDefaultAsync(pl => pl.Id == product.PickupLocationId, cancellationToken);

                if (pickupLocation is not null && pickupLocation.ZoneId != request.DeliveryZoneId)
                {
                    throw new BadRequestException(
                        $"Product '{product.Title}' is only available for local delivery within its zone.");
                }
            }
        }

        // Step 4: Create the order
        var orderId = new int();
        var orderNumber = GenerateOrderNumber(now);

        var order = new Order
        {
            Id = orderId,
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending,
            PaymentMethod = request.PaymentMethod,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryCity = request.DeliveryCity,
            DeliveryZoneId = zoneId,
            Subtotal = 0m,
            TotalDeliveryFee = 0m,
            TotalAmount = 0m,
            CreatedAt = now
        };

        var orderItems = new List<OrderItem>();
        var transactionSplits = new List<TransactionSplit>();
        decimal orderSubTotal = 0m;

        // Step 5: Create order items with 5-bucket split
        foreach (var itemRequest in request.Items)
        {
            var product = products.First(p => p.Id == itemRequest.ProductId);
            var orderItemId = new int();

            var itemTotalPrice = product.FinalPrice * itemRequest.Quantity;
            var itemVendorPayout = product.VendorNetPayout * itemRequest.Quantity;
            var itemCommission = product.CommissionAmount * itemRequest.Quantity;
            var itemVat = product.VatAmount * itemRequest.Quantity;
            var itemParentUplift = product.ParentUpliftAmount * itemRequest.Quantity;
            var itemWithholdingTax = product.WithholdingTaxAmount * itemRequest.Quantity;

            var orderItem = new OrderItem
            {
                Id = orderItemId,
                OrderId = orderId,
                ProductId = product.Id,
                ProductTitle = product.Title,
                ProductSku = product.Sku,
                ProductImage = product.Images?.FirstOrDefault(),
                VendorName = product.ParentVendor?.BusinessName ?? string.Empty,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.FinalPrice,
                TotalPrice = itemTotalPrice,
                VendorNetPayout = itemVendorPayout,
                CommissionAmount = itemCommission,
                VatAmount = itemVat,
                ParentUpliftAmount = itemParentUplift,
                WithholdingTaxAmount = itemWithholdingTax,
                ParentVendorId = product.ParentVendorId,
                SubVendorId = product.SubVendorId,
                PickupLocationId = product.PickupLocationId,
                CreatedAt = now
            };

            orderItems.Add(orderItem);
            orderSubTotal += itemTotalPrice;

            // Create 5-bucket transaction split for this item
            var split = new TransactionSplit
            {
                Id = new int(),
                OrderId = orderId,
                OrderItemId = orderItemId,
                ProductId = product.Id,
                ParentVendorId = product.ParentVendorId,
                SubVendorId = product.SubVendorId,
                GrossAmount = itemTotalPrice,
                VendorPayoutBucket = itemVendorPayout,
                AroobaBucket = itemCommission,
                VatBucket = itemVat,
                ParentUpliftBucket = itemParentUplift,
                WithholdingTaxBucket = itemWithholdingTax,
                CreatedAt = now
            };

            transactionSplits.Add(split);

            // Decrement stock for ready-stock products
            if (product.StockMode == StockMode.ReadyStock)
            {
                product.QuantityAvailable -= itemRequest.Quantity;
            }
        }

        order.Subtotal = orderSubTotal;
        order.TotalAmount = orderSubTotal; // Shipping fee added separately

        // Step 6: Group order items into shipments by pickup location
        var shipmentGroups = orderItems
            .GroupBy(oi => oi.PickupLocationId)
            .ToList();

        var shipments = new List<Shipment>();

        foreach (var group in shipmentGroups)
        {
            var shipmentId = new int();
            var shipmentItems = group.ToList();

            var shipment = new Shipment
            {
                Id = shipmentId,
                OrderId = orderId,
                PickupLocationId = group.Key ?? 0,
                Status = ShipmentStatus.Pending,
                TrackingNumber = GenerateTrackingNumber(now, shipments.Count + 1),
                TotalWeight = shipmentItems.Sum(oi =>
                {
                    var product = products.First(p => p.Id == oi.ProductId);
                    return product.WeightKg * oi.Quantity;
                }),
                ItemCount = shipmentItems.Sum(oi => oi.Quantity),
                CreatedAt = now
            };

            // Link order items to this shipment
            foreach (var item in shipmentItems)
            {
                item.ShipmentId = shipmentId;
            }

            shipments.Add(shipment);
        }

        // Step 7: Update vendor wallet pending balances
        var vendorPayoutGroups = orderItems
            .GroupBy(oi => oi.ParentVendorId)
            .ToList();

        foreach (var vendorGroup in vendorPayoutGroups)
        {
            var wallet = await _context.VendorWallets
                .FirstOrDefaultAsync(w => w.ParentVendorId == vendorGroup.Key, cancellationToken);

            if (wallet is not null)
            {
                var vendorTotal = vendorGroup.Sum(oi => oi.VendorNetPayout);
                wallet.PendingBalance += vendorTotal;
                wallet.TotalEarnings += vendorTotal;
            }

            // Record ledger entries for each vendor
            var ledgerEntry = new LedgerEntry
            {
                Id = new int(),
                ParentVendorId = vendorGroup.Key,
                OrderId = orderId,
                TransactionType = TransactionType.Sale,
                Amount = vendorGroup.Sum(oi => oi.TotalPrice),
                VendorAmount = vendorGroup.Sum(oi => oi.VendorNetPayout),
                CommissionAmount = vendorGroup.Sum(oi => oi.CommissionAmount),
                VatAmount = vendorGroup.Sum(oi => oi.VatAmount),
                Description = $"Sale from order {orderNumber}",
                BalanceStatus = BalanceStatus.Pending,
                CreatedAt = now
            };

            _context.LedgerEntries.Add(ledgerEntry);
        }

        // Step 8: Persist everything
        _context.Orders.Add(order);

        foreach (var item in orderItems)
        {
            _context.OrderItems.Add(item);
        }

        foreach (var shipment in shipments)
        {
            _context.Shipments.Add(shipment);
        }

        foreach (var split in transactionSplits)
        {
            _context.TransactionSplits.Add(split);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return orderId;
    }

    /// <summary>
    /// Generates a human-readable order number with date prefix.
    /// Format: ARB-YYYYMMDD-XXXXXX
    /// </summary>
    private static string GenerateOrderNumber(DateTime date)
    {
        var datePart = date.ToString("yyyyMMdd");
        var randomPart = new int().ToString("N")[..6].ToUpperInvariant();
        return $"ARB-{datePart}-{randomPart}";
    }

    /// <summary>
    /// Generates a tracking number for a shipment.
    /// Format: TRK-YYYYMMDD-XXXX-NN
    /// </summary>
    private static string GenerateTrackingNumber(DateTime date, int sequence)
    {
        var datePart = date.ToString("yyyyMMdd");
        var randomPart = new int().ToString("N")[..4].ToUpperInvariant();
        return $"TRK-{datePart}-{randomPart}-{sequence:D2}";
    }
}

/// <summary>
/// Validates the <see cref="CreateOrderCommand"/> ensuring all required fields
/// are present and the order contains at least one item.
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// Initializes validation rules for order creation.
    /// </summary>
    public CreateOrderCommandValidator()
    {
        RuleFor(o => o.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(o => o.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(o => o.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required for each order item.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });

        RuleFor(o => o.PaymentMethod)
            .IsInEnum().WithMessage("A valid payment method is required.");

        RuleFor(o => o.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required.")
            .MaximumLength(500).WithMessage("Delivery address must not exceed 500 characters.");

        RuleFor(o => o.DeliveryCity)
            .NotEmpty().WithMessage("Delivery city is required.")
            .MaximumLength(100).WithMessage("Delivery city must not exceed 100 characters.");

        RuleFor(o => o.DeliveryZoneId)
            .NotEmpty().WithMessage("Delivery zone ID is required.");
    }
}
