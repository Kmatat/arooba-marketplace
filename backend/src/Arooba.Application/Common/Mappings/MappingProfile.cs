using Arooba.Application.Features.Customers.Queries;
using Arooba.Application.Features.Finance.Queries;
using Arooba.Application.Features.Orders.Queries;
using Arooba.Application.Features.Products.Commands;
using Arooba.Application.Features.Products.Queries;
using Arooba.Application.Features.Shipping.Queries;
using Arooba.Application.Features.Vendors.Commands;
using Arooba.Application.Features.Vendors.Queries;
using Arooba.Domain.Entities;
using AutoMapper;

namespace Arooba.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that defines all entity-to-DTO and command-to-entity mappings
/// used throughout the Arooba Marketplace application layer.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of <see cref="MappingProfile"/> and configures
    /// all mapping definitions.
    /// </summary>
    public MappingProfile()
    {
        // Vendor mappings
        CreateMap<ParentVendor, VendorDto>()
            .ForMember(d => d.SubVendorCount, opt => opt.MapFrom(s => s.SubVendors != null ? s.SubVendors.Count : 0));
        CreateMap<ParentVendor, VendorDetailDto>();
        CreateMap<SubVendor, SubVendorDto>();
        CreateMap<CreateVendorCommand, ParentVendor>();
        CreateMap<CreateSubVendorCommand, SubVendor>();

        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductDetailDto>();
        CreateMap<CreateProductCommand, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Sku, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.FinalPrice, opt => opt.Ignore());

        // Order mappings
        CreateMap<Order, OrderDto>();
        CreateMap<Order, OrderDetailDto>();
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<Shipment, ShipmentDto>();

        // Customer mappings
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerDetailDto>();
        CreateMap<CustomerAddress, CustomerAddressDto>();

        // Finance mappings
        CreateMap<VendorWallet, VendorWalletDto>();
        CreateMap<LedgerEntry, LedgerEntryDto>();
        CreateMap<TransactionSplit, TransactionSplitDto>();

        // Shipping mappings
        CreateMap<ShippingZone, ShippingZoneDto>();

        // Category mapping
        CreateMap<ProductCategory, ProductCategoryDto>();
    }
}
