/**
 * ============================================================
 * AROOBA MARKETPLACE — User Analytics Type Definitions
 * ============================================================
 */

export type UserActivityAction =
  | 'product_viewed'
  | 'added_to_cart'
  | 'removed_from_cart'
  | 'cart_quantity_changed'
  | 'checkout_started'
  | 'purchase_completed'
  | 'checkout_abandoned'
  | 'product_searched'
  | 'filter_applied'
  | 'category_viewed'
  | 'wishlist_added'
  | 'product_shared'
  | 'session_started'
  | 'session_ended'
  | 'order_history_viewed'
  | 'return_initiated'
  | 'related_product_clicked'
  | 'vendor_page_viewed';

export interface UserActivityEvent {
  id: string;
  userId: string;
  userName: string;
  action: UserActivityAction;
  productId?: string;
  productTitle?: string;
  categoryId?: string;
  orderId?: string;
  searchQuery?: string;
  sessionId?: string;
  deviceType?: string;
  pageUrl?: string;
  cartValue?: number;
  cartItemCount?: number;
  createdAt: string;
}

export interface ConversionFunnel {
  productViews: number;
  addedToCart: number;
  checkoutsStarted: number;
  purchasesCompleted: number;
  viewToCartRate: number;
  cartToCheckoutRate: number;
  checkoutToCompletionRate: number;
  overallConversionRate: number;
}

export interface ProductAnalyticsItem {
  productId: string;
  productTitle: string;
  productTitleAr: string;
  categoryId: string;
  views: number;
  addedToCart: number;
  purchases: number;
  conversionRate: number;
  relatedProductClicks: number;
}

export interface CategoryAnalytics {
  categoryId: string;
  views: number;
  cartAdds: number;
  purchases: number;
}

export interface ActivityTrendPoint {
  date: string;
  views: number;
  cartAdds: number;
  purchases: number;
  sessions: number;
}

export interface DeviceBreakdown {
  deviceType: string;
  count: number;
  percentage: number;
}

export interface TopSearch {
  query: string;
  count: number;
}

export interface AnalyticsSummary {
  totalSessions: number;
  uniqueUsers: number;
  totalProductViews: number;
  totalCartAdds: number;
  totalPurchases: number;
  overallConversionRate: number;
  totalSearches: number;
  averageCartValue: number;
  dailyTrend: ActivityTrendPoint[];
  deviceBreakdown: DeviceBreakdown[];
  topSearches: TopSearch[];
}
