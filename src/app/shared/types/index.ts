/**
 * ============================================================
 * AROOBA MARKETPLACE — Core Type Definitions
 * ============================================================
 * 
 * These types map directly to the Master Functional Specification.
 * Each type references the module it belongs to.
 * ============================================================
 */

// ──────────────────────────────────────────────
// MODULE 1: IAM (Identity & Access Management)
// ──────────────────────────────────────────────

export type UserRole = 
  | 'customer'
  | 'parent_vendor'
  | 'sub_vendor'
  | 'admin_super'
  | 'admin_finance'
  | 'admin_operations'
  | 'admin_support';

export interface User {
  id: string;
  mobileNumber: string;          // Primary identifier (+20 format)
  email?: string;                 // Optional
  fullName: string;
  fullNameAr?: string;
  role: UserRole;
  isVerified: boolean;
  avatarUrl?: string;
  lastLoginIp?: string;
  lastLoginDeviceId?: string;
  createdAt: string;
  updatedAt: string;
}

// ──────────────────────────────────────────────
// MODULE 2: VENDOR ECOSYSTEM
// ──────────────────────────────────────────────

export type VendorStatus = 'pending' | 'active' | 'suspended' | 'rejected';
export type VendorType = 'legalized' | 'non_legalized';

export interface ParentVendor {
  id: string;
  userId: string;
  businessName: string;
  businessNameAr: string;
  type: VendorType;
  status: VendorStatus;
  
  // Legal Documents (Gatekeeper)
  commercialRegNumber?: string;
  taxId?: string;
  isVatRegistered: boolean;
  
  // Cooperative (for non-legalized)
  cooperativeId?: string;
  
  // Financial
  defaultCommissionRate: number;
  bankName?: string;
  bankAccountNumber?: string;
  
  // Metrics
  reliabilityStrikes: number;
  averageRating: number;
  totalOrders: number;
  totalRevenue: number;
  
  // Sub-vendors managed by this parent
  subVendorIds: string[];
  
  createdAt: string;
  updatedAt: string;
}

export interface SubVendor {
  id: string;
  parentVendorId: string;
  internalName: string;           // e.g., "Aunt Nadia"
  internalNameAr: string;
  defaultLeadTimeDays: number;
  
  // Uplift from Parent
  upliftType: 'fixed' | 'percentage';
  upliftValue: number;            // EGP or %
  
  isActive: boolean;
  createdAt: string;
}

export interface Cooperative {
  id: string;
  name: string;
  nameAr: string;
  taxId: string;
  feePercentage: number;         // Default 5%
  vendorIds: string[];
  isActive: boolean;
}

// ──────────────────────────────────────────────
// MODULE 3: PRODUCT (PIM)
// ──────────────────────────────────────────────

export type StockMode = 'ready_stock' | 'made_to_order';
export type ProductStatus = 'draft' | 'pending_review' | 'active' | 'paused' | 'rejected';

export interface PickupLocation {
  id: string;
  vendorId: string;
  label: string;                  // "Main Warehouse", "Home", "Factory"
  address: string;
  city: string;
  zoneId: string;
  gpsLat?: number;
  gpsLng?: number;
  contactName: string;
  contactPhone: string;
  isDefault: boolean;
}

export interface Product {
  id: string;
  sku: string;                    // PARENT-SUB-CAT-001
  parentVendorId: string;
  subVendorId?: string;
  categoryId: string;
  
  title: string;
  titleAr: string;
  description: string;
  descriptionAr: string;
  images: string[];               // URLs, auto-compressed to <150KB
  
  // Pricing (The Engine)
  costPrice: number;              // Visible only to vendor
  sellingPrice: number;           // Before uplifts
  cooperativeFee: number;         // 5% if non-legalized, else 0
  marketplaceUplift: number;      // Calculated uplift amount
  finalPrice: number;             // What customer sees
  
  // Logistics
  pickupLocationId: string;
  stockMode: StockMode;
  leadTimeDays?: number;          // For made-to-order
  quantityAvailable: number;
  
  // Physical (for shipping calc)
  weightKg: number;
  dimensionL?: number;
  dimensionW?: number;
  dimensionH?: number;
  volumetricWeight?: number;      // (L*W*H)/5000
  
  // Restrictions
  isLocalOnly: boolean;           // Geo-fencing
  allowedZoneIds?: string[];
  
  status: ProductStatus;
  isFeatured: boolean;
  
  createdAt: string;
  updatedAt: string;
}

// ──────────────────────────────────────────────
// MODULE 4: ORDERS (OMS)
// ──────────────────────────────────────────────

export type OrderStatus = 
  | 'pending'
  | 'accepted'
  | 'ready_to_ship'
  | 'in_transit'
  | 'delivered'
  | 'returned'
  | 'cancelled'
  | 'rejected_shipping';

export type PaymentMethod = 'cod' | 'fawry' | 'card' | 'wallet';

export interface Order {
  id: string;
  customerId: string;
  customerName: string;
  
  // Items
  items: OrderItem[];
  
  // Financials
  subtotal: number;
  totalDeliveryFee: number;
  totalAmount: number;
  paymentMethod: PaymentMethod;
  
  // Delivery
  deliveryAddress: string;
  deliveryCity: string;
  deliveryZoneId: string;
  
  // Splitting: one order → multiple shipments
  shipments: Shipment[];
  
  status: OrderStatus;
  createdAt: string;
  updatedAt: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productTitle: string;
  productImage: string;
  vendorId: string;
  vendorName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  pickupLocationId: string;
  
  // The 5-Bucket Split (per item)
  bucketA_vendorRevenue: number;
  bucketB_vendorVat: number;
  bucketC_aroobaRevenue: number;
  bucketD_aroobaVat: number;
  bucketE_logisticsFee: number;
}

export interface Shipment {
  id: string;
  orderId: string;
  pickupLocationId: string;
  trackingNumber?: string;
  courierProvider?: string;
  deliveryFee: number;
  codAmountDue: number;
  status: OrderStatus;
  estimatedDeliveryDate?: string;
  actualDeliveryDate?: string;
}

// ──────────────────────────────────────────────
// MODULE 5: CUSTOMER EXPERIENCE
// ──────────────────────────────────────────────

export interface Customer {
  id: string;
  userId: string;
  fullName: string;
  
  // Loyalty
  loyaltyPoints: number;
  walletBalance: number;         // EGP in Arooba wallet
  referralCode: string;
  referredBy?: string;
  
  // Subscription
  activeSubscriptions: Subscription[];
  
  // History
  totalOrders: number;
  totalSpent: number;
  
  addresses: CustomerAddress[];
}

export interface CustomerAddress {
  id: string;
  label: string;                  // "Home", "Office"
  fullAddress: string;
  city: string;
  zoneId: string;
  isDefault: boolean;
}

export interface Subscription {
  id: string;
  customerId: string;
  name: string;                   // "Weekly Essentials Box"
  frequency: 'weekly' | 'biweekly' | 'monthly';
  items: { productId: string; quantity: number }[];
  nextDeliveryDate: string;
  isActive: boolean;
}

// ──────────────────────────────────────────────
// MODULE 6: FINANCE & RECONCILIATION
// ──────────────────────────────────────────────

export type TransactionType = 'sale' | 'commission' | 'vat' | 'shipping' | 'refund' | 'payout';
export type BalanceStatus = 'pending' | 'available' | 'withdrawn';

export interface VendorWallet {
  vendorId: string;
  totalBalance: number;
  pendingBalance: number;         // In 14-day hold
  availableBalance: number;       // Withdrawable
  lifetimeEarnings: number;
}

export interface LedgerEntry {
  id: string;
  transactionId: string;
  type: TransactionType;
  amount: number;
  balanceAfter: number;
  description: string;
  orderId?: string;
  vendorId?: string;
  createdAt: string;
}

export interface TransactionSplit {
  id: string;
  orderItemId: string;
  orderId: string;
  bucketA: number;
  bucketB: number;
  bucketC: number;
  bucketD: number;
  bucketE: number;
  totalAmount: number;
  createdAt: string;
}

// ──────────────────────────────────────────────
// SHARED: Shipping Zones & Rates
// ──────────────────────────────────────────────

export interface ShippingZone {
  id: string;
  name: string;
  nameAr: string;
  citiesCovered: string[];
}

export interface RateCard {
  id: string;
  fromZoneId: string;
  toZoneId: string;
  basePrice: number;
  pricePerKg: number;
}

// ──────────────────────────────────────────────
// DASHBOARD / ANALYTICS
// ──────────────────────────────────────────────

export interface DashboardStats {
  totalGmv: number;
  totalOrders: number;
  activeVendors: number;
  registeredCustomers: number;
  avgOrderValue: number;
  codRatio: number;
  returnRate: number;
  monthlyGrowth: number;
}

export interface TimeSeriesData {
  date: string;
  value: number;
  label?: string;
}
