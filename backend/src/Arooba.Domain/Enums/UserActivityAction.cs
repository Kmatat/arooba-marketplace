namespace Arooba.Domain.Enums;

/// <summary>
/// Types of user activities tracked for analytics and behavior analysis.
/// </summary>
public enum UserActivityAction
{
    /// <summary>User viewed a product detail page.</summary>
    ProductViewed,

    /// <summary>User added an item to their cart.</summary>
    AddedToCart,

    /// <summary>User removed an item from their cart.</summary>
    RemovedFromCart,

    /// <summary>User updated quantity in cart.</summary>
    CartQuantityChanged,

    /// <summary>User started the checkout process.</summary>
    CheckoutStarted,

    /// <summary>User completed a purchase.</summary>
    PurchaseCompleted,

    /// <summary>User abandoned checkout without completing.</summary>
    CheckoutAbandoned,

    /// <summary>User searched for products.</summary>
    ProductSearched,

    /// <summary>User applied a filter or sort option.</summary>
    FilterApplied,

    /// <summary>User viewed a product category.</summary>
    CategoryViewed,

    /// <summary>User added a product to their wishlist.</summary>
    WishlistAdded,

    /// <summary>User shared a product.</summary>
    ProductShared,

    /// <summary>User logged in to the platform.</summary>
    SessionStarted,

    /// <summary>User session ended.</summary>
    SessionEnded,

    /// <summary>User viewed their order history.</summary>
    OrderHistoryViewed,

    /// <summary>User initiated a product return.</summary>
    ReturnInitiated,

    /// <summary>User clicked a related/recommended product.</summary>
    RelatedProductClicked,

    /// <summary>User viewed a vendor storefront.</summary>
    VendorPageViewed
}
