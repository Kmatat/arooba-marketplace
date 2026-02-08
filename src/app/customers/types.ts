/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer CRM Type Definitions
 * ============================================================
 *
 * Extended types for the complete Customer CRM module including
 * ratings, performance metrics, login activity, and audit logs.
 * ============================================================
 */

// ──────────────────────────────────────────────
// CUSTOMER CRM — Extended Profile
// ──────────────────────────────────────────────

export type CustomerStatus = 'active' | 'inactive' | 'blocked' | 'churned';
export type CustomerTier = 'bronze' | 'silver' | 'gold' | 'platinum';

export interface CustomerCRM {
  id: string;
  userId: string;
  fullName: string;
  fullNameAr: string;
  mobileNumber: string;
  email?: string;
  avatarUrl?: string;
  preferredLanguage: 'ar' | 'en';
  status: CustomerStatus;
  tier: CustomerTier;

  // Loyalty & Wallet
  loyaltyPoints: number;
  lifetimeLoyaltyPoints: number;
  walletBalance: number;
  referralCode: string;
  referredBy?: string;
  referralCount: number;

  // Order Stats
  totalOrders: number;
  totalSpent: number;
  averageOrderValue: number;
  lastOrderDate?: string;

  // Engagement
  totalReviews: number;
  averageRating: number;
  lastLoginAt?: string;
  totalSessions: number;
  registeredAt: string;

  addresses: CustomerAddressCRM[];
}

export interface CustomerAddressCRM {
  id: string;
  label: string;
  fullAddress: string;
  city: string;
  governorate: string;
  zoneId: string;
  isDefault: boolean;
}

// ──────────────────────────────────────────────
// CUSTOMER ORDERS
// ──────────────────────────────────────────────

export type OrderStatusCRM = 'pending' | 'accepted' | 'ready_to_ship' | 'in_transit' | 'delivered' | 'returned' | 'cancelled';
export type PaymentMethodCRM = 'cod' | 'fawry' | 'card' | 'wallet';

export interface CustomerOrderCRM {
  id: string;
  orderNumber: string;
  status: OrderStatusCRM;
  totalAmount: number;
  itemCount: number;
  paymentMethod: PaymentMethodCRM;
  deliveryAddress: string;
  deliveryCity: string;
  createdAt: string;
  deliveredAt?: string;
  items: CustomerOrderItemCRM[];
}

export interface CustomerOrderItemCRM {
  id: string;
  productTitle: string;
  productImage: string;
  vendorName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

// ──────────────────────────────────────────────
// CUSTOMER RATINGS & REVIEWS
// ──────────────────────────────────────────────

export interface CustomerReview {
  id: string;
  customerId: string;
  orderId: string;
  orderNumber: string;
  productId: string;
  productTitle: string;
  productImage: string;
  vendorName: string;
  rating: number;           // 1-5 stars
  reviewText?: string;
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  status: 'published' | 'pending' | 'flagged' | 'removed';
  adminReply?: string;
  createdAt: string;
  updatedAt?: string;
}

// ──────────────────────────────────────────────
// CUSTOMER PERFORMANCE METRICS
// ──────────────────────────────────────────────

export interface CustomerPerformance {
  customerId: string;

  // Spending
  monthlySpending: { month: string; amount: number }[];
  categoryBreakdown: { category: string; amount: number; percentage: number }[];

  // Engagement Score (0-100)
  engagementScore: number;
  recencyScore: number;      // Days since last purchase
  frequencyScore: number;    // Orders per month
  monetaryScore: number;     // Average spending

  // Loyalty
  pointsEarned: number;
  pointsRedeemed: number;
  pointsBalance: number;
  tierProgress: number;      // Percentage to next tier
  nextTier: CustomerTier;
  pointsToNextTier: number;

  // Referral Program
  referralsSent: number;
  referralsConverted: number;
  referralEarnings: number;

  // Return Rate
  totalReturns: number;
  returnRate: number;

  // Cart Behavior
  averageCartSize: number;
  cartAbandonmentRate: number;
}

// ──────────────────────────────────────────────
// CUSTOMER LOGIN & ACTIVITY
// ──────────────────────────────────────────────

export type LoginStatus = 'success' | 'failed' | 'blocked';
export type DeviceType = 'mobile' | 'desktop' | 'tablet';

export interface CustomerLoginEntry {
  id: string;
  customerId: string;
  timestamp: string;
  status: LoginStatus;
  ipAddress: string;
  deviceType: DeviceType;
  deviceInfo: string;        // User agent summary
  location?: string;         // Geo-IP resolved
  sessionDuration?: number;  // Minutes
}

export type ActivityAction =
  | 'page_view'
  | 'product_view'
  | 'search'
  | 'add_to_cart'
  | 'remove_from_cart'
  | 'checkout_start'
  | 'purchase'
  | 'review_submit'
  | 'wishlist_add'
  | 'referral_share'
  | 'wallet_topup'
  | 'profile_update'
  | 'address_add'
  | 'subscription_change';

export interface CustomerActivity {
  id: string;
  customerId: string;
  action: ActivityAction;
  description: string;
  descriptionAr: string;
  metadata?: Record<string, string>;
  productId?: string;
  productTitle?: string;
  orderId?: string;
  orderNumber?: string;
  sessionId: string;
  deviceType: DeviceType;
  ipAddress: string;
  timestamp: string;
}

// ──────────────────────────────────────────────
// CUSTOMER LOGS & AUDIT TRAIL
// ──────────────────────────────────────────────

export type CustomerLogAction =
  | 'account_created'
  | 'profile_updated'
  | 'address_added'
  | 'address_removed'
  | 'password_changed'
  | 'status_changed'
  | 'tier_upgraded'
  | 'tier_downgraded'
  | 'wallet_credited'
  | 'wallet_debited'
  | 'loyalty_earned'
  | 'loyalty_redeemed'
  | 'referral_applied'
  | 'order_placed'
  | 'order_cancelled'
  | 'order_returned'
  | 'review_posted'
  | 'review_removed'
  | 'subscription_started'
  | 'subscription_cancelled'
  | 'account_blocked'
  | 'account_unblocked';

export type LogSeverity = 'info' | 'warning' | 'error' | 'critical';

export interface CustomerLogEntry {
  id: string;
  customerId: string;
  action: CustomerLogAction;
  severity: LogSeverity;
  description: string;
  descriptionAr: string;
  performedBy: string;       // Admin or 'system' or 'customer'
  performedByRole: string;
  oldValues?: Record<string, string>;
  newValues?: Record<string, string>;
  ipAddress?: string;
  timestamp: string;
}

// ──────────────────────────────────────────────
// CRM SUMMARY STATS
// ──────────────────────────────────────────────

export interface CustomerCRMStats {
  totalCustomers: number;
  activeCustomers: number;
  newThisMonth: number;
  churnRate: number;
  avgLifetimeValue: number;
  totalLoyaltyIssued: number;
  avgEngagementScore: number;
  topTierCount: number;
}
