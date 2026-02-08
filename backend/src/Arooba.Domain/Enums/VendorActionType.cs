namespace Arooba.Domain.Enums;

/// <summary>
/// Types of vendor actions that require admin approval.
/// </summary>
public enum VendorActionType
{
    ProductListing,
    PriceChange,
    ProfileUpdate,
    SubVendorAddition,
    BankDetailsChange,
    CategoryChange,
    BulkStockUpdate,
    PromotionRequest,
    RefundRequest,
    AccountDeactivation
}
