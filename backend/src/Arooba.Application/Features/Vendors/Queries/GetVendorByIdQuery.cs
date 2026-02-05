using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Vendors.Queries;

/// <summary>
/// Query to retrieve a single parent vendor by ID, including sub-vendors and summary statistics.
/// </summary>
public record GetVendorByIdQuery : IRequest<VendorDetailDto>
{
    /// <summary>Gets the vendor identifier.</summary>
    public Guid VendorId { get; init; }
}

/// <summary>
/// DTO for a sub-vendor within a parent vendor's detail view.
/// </summary>
public record SubVendorDto
{
    /// <summary>Gets the sub-vendor identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the business name.</summary>
    public string BusinessName { get; init; } = default!;

    /// <summary>Gets the Arabic business name.</summary>
    public string BusinessNameAr { get; init; } = default!;

    /// <summary>Gets the mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the email address.</summary>
    public string Email { get; init; } = default!;

    /// <summary>Gets the uplift type.</summary>
    public UpliftType UpliftType { get; init; }

    /// <summary>Gets the uplift value.</summary>
    public decimal UpliftValue { get; init; }

    /// <summary>Gets the custom uplift override, if any.</summary>
    public decimal? CustomUpliftOverride { get; init; }

    /// <summary>Gets whether the sub-vendor is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Detailed DTO for a parent vendor including sub-vendors and aggregate stats.
/// </summary>
public record VendorDetailDto
{
    /// <summary>Gets the vendor identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the associated user identifier.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the business name.</summary>
    public string BusinessName { get; init; } = default!;

    /// <summary>Gets the Arabic business name.</summary>
    public string BusinessNameAr { get; init; } = default!;

    /// <summary>Gets the vendor type.</summary>
    public VendorType VendorType { get; init; }

    /// <summary>Gets the current vendor status.</summary>
    public VendorStatus Status { get; init; }

    /// <summary>Gets the mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the email address.</summary>
    public string Email { get; init; } = default!;

    /// <summary>Gets the commercial registration number.</summary>
    public string? CommercialRegNumber { get; init; }

    /// <summary>Gets the tax identification number.</summary>
    public string? TaxId { get; init; }

    /// <summary>Gets whether the vendor is VAT registered.</summary>
    public bool IsVatRegistered { get; init; }

    /// <summary>Gets the commission rate.</summary>
    public decimal CommissionRate { get; init; }

    /// <summary>Gets the bank name.</summary>
    public string BankName { get; init; } = default!;

    /// <summary>Gets the bank account number.</summary>
    public string BankAccountNumber { get; init; } = default!;

    /// <summary>Gets the list of sub-vendors.</summary>
    public List<SubVendorDto> SubVendors { get; init; } = new();

    /// <summary>Gets the total number of products listed by this vendor.</summary>
    public int TotalProducts { get; init; }

    /// <summary>Gets the total number of orders received.</summary>
    public int TotalOrders { get; init; }

    /// <summary>Gets the total gross merchandise value in EGP.</summary>
    public decimal TotalGmv { get; init; }

    /// <summary>Gets the vendor's available wallet balance in EGP.</summary>
    public decimal AvailableBalance { get; init; }

    /// <summary>Gets the creation date.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the last update date.</summary>
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Handles retrieval of a single vendor with full detail including sub-vendors and statistics.
/// </summary>
public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetVendorByIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetVendorByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves the vendor, its sub-vendors, and computes aggregate statistics
    /// from products, orders, and wallet data.
    /// </summary>
    /// <param name="request">The query containing the vendor ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A detailed vendor DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the vendor is not found.</exception>
    public async Task<VendorDetailDto> Handle(
        GetVendorByIdQuery request,
        CancellationToken cancellationToken)
    {
        var vendor = await _context.ParentVendors
            .Include(v => v.SubVendors)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        if (vendor is null)
        {
            throw new NotFoundException(nameof(ParentVendor), request.VendorId);
        }

        var totalProducts = await _context.Products
            .CountAsync(p => p.ParentVendorId == request.VendorId, cancellationToken);

        var orderStats = await _context.OrderItems
            .Where(oi => oi.Product != null && oi.Product.ParentVendorId == request.VendorId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                TotalGmv = g.Sum(oi => oi.TotalPrice)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var wallet = await _context.VendorWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ParentVendorId == request.VendorId, cancellationToken);

        var subVendorDtos = vendor.SubVendors?
            .Select(sv => _mapper.Map<SubVendorDto>(sv))
            .ToList() ?? new List<SubVendorDto>();

        return new VendorDetailDto
        {
            Id = vendor.Id,
            UserId = vendor.UserId,
            BusinessName = vendor.BusinessName,
            BusinessNameAr = vendor.BusinessNameAr,
            VendorType = vendor.VendorType,
            Status = vendor.Status,
            MobileNumber = vendor.MobileNumber,
            Email = vendor.Email,
            CommercialRegNumber = vendor.CommercialRegNumber,
            TaxId = vendor.TaxId,
            IsVatRegistered = vendor.IsVatRegistered,
            CommissionRate = vendor.CommissionRate,
            BankName = vendor.BankName,
            BankAccountNumber = vendor.BankAccountNumber,
            SubVendors = subVendorDtos,
            TotalProducts = totalProducts,
            TotalOrders = orderStats?.TotalOrders ?? 0,
            TotalGmv = orderStats?.TotalGmv ?? 0m,
            AvailableBalance = wallet?.AvailableBalance ?? 0m,
            CreatedAt = vendor.CreatedAt,
            UpdatedAt = vendor.UpdatedAt
        };
    }
}
