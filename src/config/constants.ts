/**
 * ============================================================
 * AROOBA MARKETPLACE — Business Constants
 * ============================================================
 * 
 * This file encodes the core business rules from the
 * Arooba specifications. Every magic number, policy rule,
 * and pricing threshold lives here — not scattered in components.
 * 
 * WHY THIS MATTERS (Business Context):
 * In a marketplace with vendors, cooperatives, and complex pricing,
 * having a single source of truth for rules prevents disputes.
 * If a vendor argues about their payout, we point to these constants.
 * ============================================================
 */

// ──────────────────────────────────────────────
// PLATFORM IDENTITY
// ──────────────────────────────────────────────
export const PLATFORM = {
  name: 'أروبة',
  nameEn: 'Arooba',
  tagline: 'منصة المنتجات اليدوية والحرف المصرية',
  taglineEn: "Egypt's Most Inclusive Local Marketplace",
  website: 'www.aroobh.com',
  email: 'Info@aroobh.com',
  currency: 'EGP',
  currencySymbol: 'ج.م',
  country: 'EG',
  phonePrefix: '+20',
} as const;

// ──────────────────────────────────────────────
// VAT & TAX CONFIGURATION
// ──────────────────────────────────────────────
/**
 * Egypt's standard VAT rate is 14%.
 * 
 * BUSINESS LOGIC:
 * - If a vendor IS VAT-registered → VAT is added to their portion
 *   and the vendor receives Bucket B (VAT amount) in their payout
 * - If a vendor is NOT registered → no VAT on their share
 * - Arooba ALWAYS charges VAT on its own commission (Bucket D)
 */
export const TAX = {
  vatRate: 0.14, // 14% Egyptian VAT
  aroobaAlwaysChargesVat: true,
} as const;

// ──────────────────────────────────────────────
// UPLIFT MATRIX (Pricing Engine)
// ──────────────────────────────────────────────
/**
 * The "Additive Uplift Model" — we NEVER reduce vendor prices.
 * These percentages are added ON TOP of the vendor's base price.
 * 
 * BUSINESS CHALLENGE:
 * Different product categories carry different risk profiles.
 * Fragile ceramics break in transit (25-30% markup covers losses),
 * while jewelry ships easily (15-18% is sufficient).
 */
export const UPLIFT_MATRIX = {
  'jewelry-accessories': { min: 0.15, max: 0.18, default: 0.15, risk: 'low' },
  'fashion-apparel':     { min: 0.22, max: 0.25, default: 0.22, risk: 'high' },
  'home-decor-fragile':  { min: 0.25, max: 0.30, default: 0.25, risk: 'high' },
  'home-decor-textiles': { min: 0.20, max: 0.20, default: 0.20, risk: 'medium' },
  'leather-goods':       { min: 0.20, max: 0.20, default: 0.20, risk: 'medium' },
  'beauty-personal':     { min: 0.20, max: 0.20, default: 0.20, risk: 'medium' },
  'furniture-woodwork':  { min: 0.15, max: 0.15, default: 0.15, risk: 'medium' },
  'food-essentials':     { min: 0.10, max: 0.15, default: 0.12, risk: 'low' },
} as const;

/**
 * MVP Strategy: Start with a flat 20% uplift across the board,
 * EXCEPT fragile items (25%) and items under 100 EGP (fixed 20 EGP).
 */
export const UPLIFT_RULES = {
  mvpFlatRate: 0.20,
  fragileOverride: 0.25,
  minimumFixedUplift: 15,        // EGP — protects against cheap items
  lowPriceThreshold: 100,        // Items under this get fixed markup
  lowPriceFixedMarkup: 20,       // EGP fixed for items < 100 EGP
  cooperativeFee: 0.05,          // 5% for non-legalized vendors
  logisticsSurcharge: 10,        // EGP added to buffer shipping costs
} as const;

// ──────────────────────────────────────────────
// PAYMENT WATERFALL (The 5-Bucket Split)
// ──────────────────────────────────────────────
/**
 * Every customer payment is split into 5 buckets.
 * This is THE core financial engine of the marketplace.
 * 
 * BUSINESS CONTEXT:
 * When a customer pays 575 EGP for a ceramic mug:
 * - Bucket A: Vendor gets their base price + any parent uplift
 * - Bucket B: VAT on vendor's share (IF they're VAT registered)
 * - Bucket C: Arooba's commission + platform uplift
 * - Bucket D: VAT on Arooba's share (always applies)
 * - Bucket E: Delivery fee → goes to courier company
 */
export const PAYMENT_BUCKETS = {
  A: 'vendor_revenue',
  B: 'vendor_vat',
  C: 'arooba_revenue',
  D: 'arooba_vat',
  E: 'logistics_fee',
} as const;

// ──────────────────────────────────────────────
// ESCROW & PAYOUT RULES
// ──────────────────────────────────────────────
/**
 * The 14-Day Hold protects against returns and fraud.
 * 
 * BUSINESS CHALLENGE:
 * If we pay vendors immediately and the customer returns,
 * we lose money. The 14-day hold gives time for returns.
 * This is standard practice (Amazon uses 14 days too).
 */
export const ESCROW = {
  holdDays: 14,                  // Days before funds become available
  minimumPayoutThreshold: 500,   // EGP — minimum to trigger a payout
  payoutBatchDay: 'weekly',      // Payouts are batched weekly
  codDepositCycle: 48,           // Hours — couriers must deposit COD funds
  codDiscrepancyThreshold: 0.01, // 1% — triggers freeze on courier
} as const;

// ──────────────────────────────────────────────
// ORDER & LOGISTICS CONFIGURATION
// ──────────────────────────────────────────────
export const ORDER_STATUS = {
  PENDING: 'pending',
  ACCEPTED: 'accepted',
  READY_TO_SHIP: 'ready_to_ship',
  IN_TRANSIT: 'in_transit',
  DELIVERED: 'delivered',
  RETURNED: 'returned',
  CANCELLED: 'cancelled',
  REJECTED_SHIPPING: 'rejected_shipping',
} as const;

/**
 * SOP 4.1: The "Accept or Die" Rule
 * Vendors MUST accept orders within 24 hours.
 * Failure = auto-cancel + reliability strike.
 */
export const VENDOR_SLAS = {
  acceptanceWindowHours: 24,
  maxReliabilityStrikes: 3,      // 3 strikes = account review
  wastedTripFee: 20,             // EGP — courier arrives, vendor not ready
  maxDispatchHours: 24,          // After acceptance
  minimumRating: 4.0,            // Below this = ranking drop
  maxReturnRate: 0.12,           // 12% — triggers review
} as const;

/**
 * Shipping fee calculation uses zones and weight.
 * Volumetric Weight = (L × W × H) / 5000
 * Chargeable Weight = MAX(Actual, Volumetric)
 */
export const SHIPPING = {
  volumetricDivisor: 5000,
  smartComBaseRate: 50,          // EGP approximate base rate
  subsidizedRate: 45,            // What customer sees after buffer
  imageMaxSizeKB: 150,          // Auto-compress for 3G networks
} as const;

// ──────────────────────────────────────────────
// DELIVERY ZONES & SLA TARGETS
// ──────────────────────────────────────────────
export const DELIVERY_ZONES = [
  { id: 'cairo',       name: 'القاهرة الكبرى', nameEn: 'Greater Cairo',  slaDays: '1-2' },
  { id: 'alexandria',  name: 'الإسكندرية',     nameEn: 'Alexandria',     slaDays: '1-2' },
  { id: 'delta',       name: 'الدلتا',         nameEn: 'Delta',          slaDays: '2-3' },
  { id: 'upper-egypt', name: 'صعيد مصر',       nameEn: 'Upper Egypt',    slaDays: '3-5' },
  { id: 'canal',       name: 'القنال',         nameEn: 'Canal Cities',   slaDays: '2-3' },
  { id: 'sinai',       name: 'سيناء',          nameEn: 'Sinai',          slaDays: '5-7' },
] as const;

// ──────────────────────────────────────────────
// CUSTOMER & LOYALTY
// ──────────────────────────────────────────────
export const LOYALTY = {
  pointsPerEgp: 1,              // 1 point per 1 EGP spent
  referralGiveAmount: 50,       // "Give 50 EGP"
  referralGetAmount: 50,        // "Get 50 EGP"
  referralUnlockOnDelivery: true, // Only after first delivery
  subscriptionAdvanceHours: 48, // Generate order 48hrs before
} as const;

// ──────────────────────────────────────────────
// ANTI-FRAUD RULES
// ──────────────────────────────────────────────
/**
 * Egypt-specific fraud prevention.
 * COD fraud is the #1 challenge — customers order then refuse.
 * These thresholds help identify bad actors.
 */
export const FRAUD_RULES = {
  maxCodCancelsBeforeBlock: 3,
  maxUnreachableAttempts: 2,
  deviceFingerprintCheck: true,
  priceDeviationFlag: 0.20,     // ±20% from market = manual review
} as const;

// ──────────────────────────────────────────────
// PLATFORM KPI TARGETS (from monitoring checklist)
// ──────────────────────────────────────────────
export const KPI_TARGETS = {
  // UX Targets
  pageLoadTime: 2,              // seconds
  searchResponseTime: 1,        // seconds
  maxCheckoutSteps: 4,
  arabicContentPercent: 95,

  // Conversion Targets
  addToCartRate: 0.08,          // 8% minimum
  checkoutCompletion: 0.55,     // 55% minimum
  paymentFailureRate: 0.05,     // max 5%
  codRejectionRate: 0.25,       // max 25%

  // Operational Targets
  activeVendorPercent: 0.70,    // 70% selling in last 7 days
  inactiveVendorMax: 0.15,      // max 15% inactive
  orderAcceptanceRate: 0.95,    // 95% minimum
  orderCancellationMax: 0.10,   // max 10%
  deliveryFirstAttempt: 0.85,   // 85% success on first try

  // Financial Targets
  refundRateMax: 0.12,          // max 12%
  codRatioMax: 0.65,            // COD should be ≤65% of orders
  systemUptime: 0.999,          // 99.9%
} as const;

// ──────────────────────────────────────────────
// PRODUCT CATEGORIES (for the catalog)
// ──────────────────────────────────────────────
export const PRODUCT_CATEGORIES = [
  { id: 'jewelry-accessories',  nameAr: 'مجوهرات وإكسسوارات',  nameEn: 'Jewelry & Accessories',  icon: '💍' },
  { id: 'fashion-apparel',      nameAr: 'أزياء وملابس',         nameEn: 'Fashion & Apparel',      icon: '👗' },
  { id: 'home-decor-fragile',   nameAr: 'ديكور (هش)',           nameEn: 'Home Decor (Fragile)',   icon: '🏺' },
  { id: 'home-decor-textiles',  nameAr: 'ديكور (منسوجات)',      nameEn: 'Home Decor (Textiles)',  icon: '🧶' },
  { id: 'leather-goods',        nameAr: 'منتجات جلدية',         nameEn: 'Leather Goods',          icon: '👜' },
  { id: 'beauty-personal',      nameAr: 'جمال وعناية شخصية',   nameEn: 'Beauty & Personal Care', icon: '🧴' },
  { id: 'furniture-woodwork',   nameAr: 'أثاث وأعمال خشبية',   nameEn: 'Furniture & Woodwork',   icon: '🪑' },
  { id: 'food-essentials',      nameAr: 'أغذية ومستلزمات',      nameEn: 'Food & Essentials',      icon: '🍚' },
] as const;

// ──────────────────────────────────────────────
// USER ROLES (RBAC)
// ──────────────────────────────────────────────
export const USER_ROLES = {
  CUSTOMER: 'customer',
  PARENT_VENDOR: 'parent_vendor',
  SUB_VENDOR: 'sub_vendor',
  ADMIN_SUPER: 'admin_super',
  ADMIN_FINANCE: 'admin_finance',
  ADMIN_OPERATIONS: 'admin_operations',
  ADMIN_SUPPORT: 'admin_support',
} as const;
