/**
 * AROOBA MARKETPLACE — Mock Data for Local API Server
 * Mirrors the data structures from the frontend mock files.
 */

const { randomUUID } = require('crypto');

// ── Helpers ───────────────────────────────────────────────
function uuid() { return randomUUID(); }
function paginate(items, page = 1, pageSize = 10) {
  const start = (page - 1) * pageSize;
  return {
    items: items.slice(start, start + pageSize),
    pageNumber: page,
    pageSize,
    totalCount: items.length,
    totalPages: Math.ceil(items.length / pageSize),
    hasPreviousPage: page > 1,
    hasNextPage: start + pageSize < items.length,
  };
}

// ── Vendors ───────────────────────────────────────────────
const vendors = [
  {
    id: 'v-001', userId: 'u-v001', businessName: 'Nile Crafts', businessNameAr: 'حرف النيل',
    type: 'legalized', status: 'active', commercialRegNumber: 'CR-12345', taxId: 'TX-98765',
    isVatRegistered: true, cooperativeId: null, defaultCommissionRate: 0.20,
    bankName: 'CIB Egypt', bankAccountNumber: '****5678',
    reliabilityStrikes: 0, averageRating: 4.7, totalOrders: 342, totalRevenue: 187500,
    subVendorIds: ['sv-001', 'sv-002'], createdAt: '2025-06-15T10:00:00Z', updatedAt: '2026-01-20T14:30:00Z',
  },
  {
    id: 'v-002', userId: 'u-v002', businessName: 'Desert Rose Leather', businessNameAr: 'وردة الصحراء للجلود',
    type: 'legalized', status: 'active', commercialRegNumber: 'CR-67890', taxId: 'TX-54321',
    isVatRegistered: true, cooperativeId: null, defaultCommissionRate: 0.20,
    bankName: 'Banque Misr', bankAccountNumber: '****1234',
    reliabilityStrikes: 0, averageRating: 4.5, totalOrders: 528, totalRevenue: 324000,
    subVendorIds: ['sv-003'], createdAt: '2025-05-01T08:00:00Z', updatedAt: '2026-02-01T09:15:00Z',
  },
  {
    id: 'v-003', userId: 'u-v003', businessName: 'Siwa Pottery', businessNameAr: 'فخار سيوة',
    type: 'non_legalized', status: 'active', commercialRegNumber: null, taxId: null,
    isVatRegistered: false, cooperativeId: 'coop-001', defaultCommissionRate: 0.25,
    bankName: null, bankAccountNumber: null,
    reliabilityStrikes: 1, averageRating: 4.2, totalOrders: 87, totalRevenue: 42300,
    subVendorIds: [], createdAt: '2025-09-10T12:00:00Z', updatedAt: '2026-01-15T16:45:00Z',
  },
  {
    id: 'v-004', userId: 'u-v004', businessName: 'Cairo Textiles Co.', businessNameAr: 'شركة القاهرة للمنسوجات',
    type: 'legalized', status: 'active', commercialRegNumber: 'CR-11223', taxId: 'TX-33445',
    isVatRegistered: true, cooperativeId: null, defaultCommissionRate: 0.20,
    bankName: 'NBE', bankAccountNumber: '****9012',
    reliabilityStrikes: 0, averageRating: 4.8, totalOrders: 1450, totalRevenue: 892000,
    subVendorIds: ['sv-004', 'sv-005'], createdAt: '2025-03-20T07:00:00Z', updatedAt: '2026-02-05T11:00:00Z',
  },
  {
    id: 'v-005', userId: 'u-v005', businessName: 'Aswan Beads', businessNameAr: 'خرز أسوان',
    type: 'non_legalized', status: 'pending', commercialRegNumber: null, taxId: null,
    isVatRegistered: false, cooperativeId: 'coop-002', defaultCommissionRate: 0.25,
    bankName: null, bankAccountNumber: null,
    reliabilityStrikes: 0, averageRating: 0, totalOrders: 0, totalRevenue: 0,
    subVendorIds: [], createdAt: '2026-01-28T09:30:00Z', updatedAt: '2026-01-28T09:30:00Z',
  },
];

const subVendors = [
  { id: 'sv-001', parentVendorId: 'v-001', internalName: 'Nile Crafts - Ceramics', internalNameAr: 'حرف النيل - سيراميك', defaultLeadTimeDays: 3, upliftType: 'percentage', upliftValue: 5, isActive: true, createdAt: '2025-07-01T10:00:00Z' },
  { id: 'sv-002', parentVendorId: 'v-001', internalName: 'Nile Crafts - Wood', internalNameAr: 'حرف النيل - خشب', defaultLeadTimeDays: 5, upliftType: 'fixed', upliftValue: 25, isActive: true, createdAt: '2025-08-15T10:00:00Z' },
  { id: 'sv-003', parentVendorId: 'v-002', internalName: 'Desert Rose - Bags', internalNameAr: 'وردة الصحراء - حقائب', defaultLeadTimeDays: 7, upliftType: 'percentage', upliftValue: 8, isActive: true, createdAt: '2025-06-01T10:00:00Z' },
  { id: 'sv-004', parentVendorId: 'v-004', internalName: 'Cairo Textiles - Scarves', internalNameAr: 'القاهرة للمنسوجات - أوشحة', defaultLeadTimeDays: 2, upliftType: 'fixed', upliftValue: 15, isActive: true, createdAt: '2025-04-10T10:00:00Z' },
  { id: 'sv-005', parentVendorId: 'v-004', internalName: 'Cairo Textiles - Rugs', internalNameAr: 'القاهرة للمنسوجات - سجاد', defaultLeadTimeDays: 10, upliftType: 'percentage', upliftValue: 10, isActive: false, createdAt: '2025-05-20T10:00:00Z' },
];

// ── Products ──────────────────────────────────────────────
const products = [
  {
    id: 'p-001', sku: 'ARO-CER-001', parentVendorId: 'v-001', subVendorId: 'sv-001', categoryId: 'home-decor-fragile',
    title: 'Handmade Ceramic Vase', titleAr: 'مزهرية خزفية يدوية', description: 'Beautiful hand-painted ceramic vase from Fustat.', descriptionAr: 'مزهرية خزفية مرسومة يدوياً من الفسطاط.',
    images: ['/images/vase-1.jpg'], costPrice: 180, sellingPrice: 257.40, cooperativeFee: 0, marketplaceUplift: 45, finalPrice: 293.44,
    pickupLocationId: 'loc-001', stockMode: 'ready_stock', leadTimeDays: null, quantityAvailable: 25,
    weightKg: 1.5, dimensionL: 20, dimensionW: 20, dimensionH: 35, volumetricWeight: 2.8,
    isLocalOnly: false, allowedZoneIds: null, status: 'active', isFeatured: true,
    createdAt: '2025-10-01T10:00:00Z', updatedAt: '2026-01-15T14:00:00Z',
  },
  {
    id: 'p-002', sku: 'ARO-LTH-001', parentVendorId: 'v-002', subVendorId: 'sv-003', categoryId: 'leather-goods',
    title: 'Genuine Leather Crossbody Bag', titleAr: 'حقيبة كروس جلد طبيعي', description: 'Handcrafted crossbody bag from premium Egyptian leather.', descriptionAr: 'حقيبة كروس مصنوعة يدوياً من أجود أنواع الجلود المصرية.',
    images: ['/images/bag-1.jpg'], costPrice: 450, sellingPrice: 594, cooperativeFee: 0, marketplaceUplift: 90, finalPrice: 677.16,
    pickupLocationId: 'loc-002', stockMode: 'made_to_order', leadTimeDays: 7, quantityAvailable: 0,
    weightKg: 0.8, dimensionL: 30, dimensionW: 10, dimensionH: 25, volumetricWeight: 1.5,
    isLocalOnly: false, allowedZoneIds: null, status: 'active', isFeatured: true,
    createdAt: '2025-09-15T08:00:00Z', updatedAt: '2026-02-01T10:00:00Z',
  },
  {
    id: 'p-003', sku: 'ARO-TXT-001', parentVendorId: 'v-004', subVendorId: 'sv-004', categoryId: 'home-decor-textiles',
    title: 'Hand-woven Cotton Scarf', titleAr: 'وشاح قطني منسوج يدوياً', description: 'Traditional hand-woven scarf from Upper Egypt.', descriptionAr: 'وشاح تقليدي منسوج يدوياً من صعيد مصر.',
    images: ['/images/scarf-1.jpg'], costPrice: 85, sellingPrice: 120, cooperativeFee: 0, marketplaceUplift: 20, finalPrice: 136.80,
    pickupLocationId: 'loc-003', stockMode: 'ready_stock', leadTimeDays: null, quantityAvailable: 150,
    weightKg: 0.2, dimensionL: 25, dimensionW: 5, dimensionH: 15, volumetricWeight: 0.375,
    isLocalOnly: false, allowedZoneIds: null, status: 'active', isFeatured: false,
    createdAt: '2025-11-20T12:00:00Z', updatedAt: '2026-01-10T16:00:00Z',
  },
  {
    id: 'p-004', sku: 'ARO-JWL-001', parentVendorId: 'v-003', subVendorId: null, categoryId: 'jewelry-accessories',
    title: 'Silver Bedouin Necklace', titleAr: 'عقد بدوي فضي', description: 'Authentic Bedouin-style silver necklace.', descriptionAr: 'عقد فضي بتصميم بدوي أصيل.',
    images: ['/images/necklace-1.jpg'], costPrice: 320, sellingPrice: 425.60, cooperativeFee: 16, marketplaceUplift: 64, finalPrice: 485.18,
    pickupLocationId: 'loc-004', stockMode: 'ready_stock', leadTimeDays: null, quantityAvailable: 8,
    weightKg: 0.15, dimensionL: 15, dimensionW: 10, dimensionH: 5, volumetricWeight: 0.15,
    isLocalOnly: false, allowedZoneIds: null, status: 'active', isFeatured: true,
    createdAt: '2025-12-05T09:00:00Z', updatedAt: '2026-01-25T11:00:00Z',
  },
  {
    id: 'p-005', sku: 'ARO-WDW-001', parentVendorId: 'v-001', subVendorId: 'sv-002', categoryId: 'furniture-woodwork',
    title: 'Arabesque Wooden Trinket Box', titleAr: 'صندوق خشبي أرابيسك', description: 'Intricate arabesque woodwork trinket box.', descriptionAr: 'صندوق تحف خشبي بنقوش أرابيسك دقيقة.',
    images: ['/images/box-1.jpg'], costPrice: 60, sellingPrice: 92, cooperativeFee: 0, marketplaceUplift: 20, finalPrice: 104.88,
    pickupLocationId: 'loc-001', stockMode: 'made_to_order', leadTimeDays: 5, quantityAvailable: 0,
    weightKg: 0.5, dimensionL: 15, dimensionW: 10, dimensionH: 8, volumetricWeight: 0.24,
    isLocalOnly: false, allowedZoneIds: null, status: 'pending_review', isFeatured: false,
    createdAt: '2026-01-20T14:00:00Z', updatedAt: '2026-01-20T14:00:00Z',
  },
];

// ── Orders ────────────────────────────────────────────────
const orders = [
  {
    id: 'ord-001', customerId: 'cust-001', customerName: 'فاطمة أحمد',
    items: [
      { id: 'oi-001', productId: 'p-001', productTitle: 'Handmade Ceramic Vase', vendorId: 'v-001', vendorName: 'Nile Crafts', quantity: 2, unitPrice: 293.44, totalPrice: 586.88, pickupLocationId: 'loc-001', weight: 1.5, dimensions: { l: 20, w: 20, h: 35 }, bucketSplit: { A: 360, B: 50.40, C: 90, D: 12.60, E: 74.88 } },
    ],
    subtotal: 586.88, totalDeliveryFee: 65, totalAmount: 651.88,
    paymentMethod: 'cod', status: 'delivered',
    deliveryAddressId: 'addr-001', deliveryZoneId: 'cairo',
    shipments: [{ id: 'shp-001', trackingNumber: 'SC-20260115-001', courierProvider: 'SmartCom', status: 'delivered', codAmountDue: 651.88, pickedUpAt: '2026-01-16T10:00:00Z', deliveredAt: '2026-01-17T14:30:00Z' }],
    createdAt: '2026-01-15T08:30:00Z', updatedAt: '2026-01-17T14:30:00Z', estimatedDeliveryDate: '2026-01-17', actualDeliveryDate: '2026-01-17',
  },
  {
    id: 'ord-002', customerId: 'cust-002', customerName: 'محمد حسن',
    items: [
      { id: 'oi-002', productId: 'p-002', productTitle: 'Genuine Leather Crossbody Bag', vendorId: 'v-002', vendorName: 'Desert Rose Leather', quantity: 1, unitPrice: 677.16, totalPrice: 677.16, pickupLocationId: 'loc-002', weight: 0.8, dimensions: { l: 30, w: 10, h: 25 }, bucketSplit: { A: 450, B: 63, C: 90, D: 12.60, E: 61.56 } },
    ],
    subtotal: 677.16, totalDeliveryFee: 55, totalAmount: 732.16,
    paymentMethod: 'fawry', status: 'in_transit',
    deliveryAddressId: 'addr-002', deliveryZoneId: 'alexandria',
    shipments: [{ id: 'shp-002', trackingNumber: 'SC-20260201-002', courierProvider: 'SmartCom', status: 'in_transit', codAmountDue: 0, pickedUpAt: '2026-02-02T09:00:00Z', deliveredAt: null }],
    createdAt: '2026-02-01T16:45:00Z', updatedAt: '2026-02-02T09:00:00Z', estimatedDeliveryDate: '2026-02-04', actualDeliveryDate: null,
  },
  {
    id: 'ord-003', customerId: 'cust-003', customerName: 'نورا سعيد',
    items: [
      { id: 'oi-003', productId: 'p-003', productTitle: 'Hand-woven Cotton Scarf', vendorId: 'v-004', vendorName: 'Cairo Textiles Co.', quantity: 3, unitPrice: 136.80, totalPrice: 410.40, pickupLocationId: 'loc-003', weight: 0.2, dimensions: { l: 25, w: 5, h: 15 }, bucketSplit: { A: 255, B: 35.70, C: 60, D: 8.40, E: 51.30 } },
      { id: 'oi-004', productId: 'p-004', productTitle: 'Silver Bedouin Necklace', vendorId: 'v-003', vendorName: 'Siwa Pottery', quantity: 1, unitPrice: 485.18, totalPrice: 485.18, pickupLocationId: 'loc-004', weight: 0.15, dimensions: { l: 15, w: 10, h: 5 }, bucketSplit: { A: 320, B: 0, C: 80, D: 11.20, E: 73.98 } },
    ],
    subtotal: 895.58, totalDeliveryFee: 80, totalAmount: 975.58,
    paymentMethod: 'card', status: 'pending',
    deliveryAddressId: 'addr-003', deliveryZoneId: 'delta',
    shipments: [],
    createdAt: '2026-02-07T11:20:00Z', updatedAt: '2026-02-07T11:20:00Z', estimatedDeliveryDate: '2026-02-10', actualDeliveryDate: null,
  },
];

// ── Customers ─────────────────────────────────────────────
const customers = [
  {
    id: 'cust-001', userId: 'u-c001', fullName: 'Fatma Ahmed', fullNameAr: 'فاطمة أحمد', mobileNumber: '+201012345678', email: 'fatma@example.com',
    avatarUrl: null, preferredLanguage: 'ar', status: 'active', tier: 'gold',
    loyaltyPoints: 2450, lifetimeLoyaltyPoints: 8200, walletBalance: 150,
    referralCode: 'FATMA50', referredBy: null, referralCount: 5,
    totalOrders: 42, totalSpent: 18500, averageOrderValue: 440, lastOrderDate: '2026-02-07',
    totalReviews: 15, averageRating: 4.6, lastLoginAt: '2026-02-08T08:30:00Z', totalSessions: 180, registeredAt: '2025-06-10T10:00:00Z',
    addresses: [
      { id: 'addr-001', label: 'المنزل', fullAddress: '15 شارع التحرير، المعادي', city: 'القاهرة', governorate: 'القاهرة', zoneId: 'cairo' },
    ],
  },
  {
    id: 'cust-002', userId: 'u-c002', fullName: 'Mohamed Hassan', fullNameAr: 'محمد حسن', mobileNumber: '+201098765432', email: 'mohamed.h@example.com',
    avatarUrl: null, preferredLanguage: 'ar', status: 'active', tier: 'silver',
    loyaltyPoints: 980, lifetimeLoyaltyPoints: 3200, walletBalance: 0,
    referralCode: 'MOHAMED50', referredBy: 'FATMA50', referralCount: 2,
    totalOrders: 18, totalSpent: 7800, averageOrderValue: 433, lastOrderDate: '2026-02-01',
    totalReviews: 8, averageRating: 4.3, lastLoginAt: '2026-02-07T20:15:00Z', totalSessions: 95, registeredAt: '2025-08-20T14:00:00Z',
    addresses: [
      { id: 'addr-002', label: 'المنزل', fullAddress: '8 شارع الحرية، سموحة', city: 'الإسكندرية', governorate: 'الإسكندرية', zoneId: 'alexandria' },
    ],
  },
  {
    id: 'cust-003', userId: 'u-c003', fullName: 'Noura Said', fullNameAr: 'نورا سعيد', mobileNumber: '+201155566677', email: 'noura.s@example.com',
    avatarUrl: null, preferredLanguage: 'en', status: 'active', tier: 'bronze',
    loyaltyPoints: 320, lifetimeLoyaltyPoints: 850, walletBalance: 50,
    referralCode: 'NOURA50', referredBy: null, referralCount: 0,
    totalOrders: 5, totalSpent: 2100, averageOrderValue: 420, lastOrderDate: '2026-02-07',
    totalReviews: 2, averageRating: 5.0, lastLoginAt: '2026-02-08T10:00:00Z', totalSessions: 28, registeredAt: '2025-12-01T09:00:00Z',
    addresses: [
      { id: 'addr-003', label: 'المنزل', fullAddress: '22 شارع الجمهورية، المنصورة', city: 'المنصورة', governorate: 'الدقهلية', zoneId: 'delta' },
    ],
  },
];

// ── Wallets ───────────────────────────────────────────────
const wallets = [
  { vendorId: 'v-001', totalBalance: 45200, pendingBalance: 12800, availableBalance: 32400, lifetimeEarnings: 187500 },
  { vendorId: 'v-002', totalBalance: 78500, pendingBalance: 28000, availableBalance: 50500, lifetimeEarnings: 324000 },
  { vendorId: 'v-003', totalBalance: 8700, pendingBalance: 3200, availableBalance: 5500, lifetimeEarnings: 42300 },
  { vendorId: 'v-004', totalBalance: 125000, pendingBalance: 45000, availableBalance: 80000, lifetimeEarnings: 892000 },
];

// ── Dashboard ─────────────────────────────────────────────
const dashboardStats = {
  totalGmv: 2450000, totalOrders: 1213, activeVendors: 347, registeredCustomers: 8420,
  avgOrderValue: 285, codRatio: 0.62, returnRate: 0.08, monthlyGrowth: 0.23,
};

const gmvTrend = [
  { date: '2025-10', value: 320000, label: 'أكتوبر' },
  { date: '2025-11', value: 580000, label: 'نوفمبر' },
  { date: '2025-12', value: 890000, label: 'ديسمبر' },
  { date: '2026-01', value: 1250000, label: 'يناير' },
  { date: '2026-02', value: 1680000, label: 'فبراير' },
  { date: '2026-03', value: 2450000, label: 'مارس' },
];

// ── Categories ────────────────────────────────────────────
const categories = [
  { id: 'jewelry-accessories', name: 'Jewelry & Accessories', nameAr: 'مجوهرات وإكسسوارات', uplift: { min: 0.15, max: 0.18, default: 0.15 } },
  { id: 'fashion-apparel', name: 'Fashion & Apparel', nameAr: 'أزياء وملابس', uplift: { min: 0.22, max: 0.25, default: 0.22 } },
  { id: 'home-decor-fragile', name: 'Home Decor (Fragile)', nameAr: 'ديكور (هش)', uplift: { min: 0.25, max: 0.30, default: 0.25 } },
  { id: 'home-decor-textiles', name: 'Home Decor (Textiles)', nameAr: 'ديكور (منسوجات)', uplift: { min: 0.20, max: 0.20, default: 0.20 } },
  { id: 'leather-goods', name: 'Leather Goods', nameAr: 'منتجات جلدية', uplift: { min: 0.20, max: 0.20, default: 0.20 } },
  { id: 'beauty-personal', name: 'Beauty & Personal Care', nameAr: 'جمال وعناية شخصية', uplift: { min: 0.20, max: 0.20, default: 0.20 } },
  { id: 'furniture-woodwork', name: 'Furniture & Woodwork', nameAr: 'أثاث وأعمال خشبية', uplift: { min: 0.15, max: 0.15, default: 0.15 } },
  { id: 'food-essentials', name: 'Food & Essentials', nameAr: 'أغذية ومستلزمات', uplift: { min: 0.10, max: 0.15, default: 0.12 } },
];

// ── Shipping Zones ────────────────────────────────────────
const shippingZones = [
  { id: 'cairo', name: 'Greater Cairo', nameAr: 'القاهرة الكبرى', slaDays: '1-2', basePrice: 45, pricePerKg: 5 },
  { id: 'alexandria', name: 'Alexandria', nameAr: 'الإسكندرية', slaDays: '1-2', basePrice: 50, pricePerKg: 6 },
  { id: 'delta', name: 'Delta', nameAr: 'الدلتا', slaDays: '2-3', basePrice: 55, pricePerKg: 7 },
  { id: 'upper-egypt', name: 'Upper Egypt', nameAr: 'صعيد مصر', slaDays: '3-5', basePrice: 65, pricePerKg: 8 },
  { id: 'canal', name: 'Canal Cities', nameAr: 'القنال', slaDays: '2-3', basePrice: 55, pricePerKg: 7 },
  { id: 'sinai', name: 'Sinai', nameAr: 'سيناء', slaDays: '5-7', basePrice: 80, pricePerKg: 10 },
];

// ── Analytics ─────────────────────────────────────────────
const analyticsSummary = {
  totalSessions: 24580, uniqueUsers: 8420, totalProductViews: 156200, totalCartAdds: 28680,
  totalPurchases: 1213, overallConversionRate: 0.0493, totalSearches: 45600, averageCartValue: 342,
  dailyTrend: [
    { date: '2026-02-01', views: 5200, cartAdds: 980, purchases: 42, sessions: 820 },
    { date: '2026-02-02', views: 4800, cartAdds: 910, purchases: 38, sessions: 780 },
    { date: '2026-02-03', views: 5600, cartAdds: 1050, purchases: 48, sessions: 890 },
    { date: '2026-02-04', views: 6100, cartAdds: 1120, purchases: 52, sessions: 950 },
    { date: '2026-02-05', views: 5900, cartAdds: 1080, purchases: 50, sessions: 920 },
    { date: '2026-02-06', views: 6300, cartAdds: 1150, purchases: 55, sessions: 980 },
    { date: '2026-02-07', views: 6800, cartAdds: 1240, purchases: 58, sessions: 1020 },
  ],
  deviceBreakdown: [
    { device: 'mobile', percentage: 0.69 },
    { device: 'desktop', percentage: 0.24 },
    { device: 'tablet', percentage: 0.07 },
  ],
  topSearches: [
    { query: 'حقيبة جلد', count: 3200 },
    { query: 'مزهرية', count: 2800 },
    { query: 'وشاح', count: 2100 },
    { query: 'عقد فضي', count: 1900 },
    { query: 'صندوق خشب', count: 1500 },
  ],
};

const conversionFunnel = {
  productViews: 156200, addedToCart: 28680, checkoutsStarted: 13400, purchasesCompleted: 1213,
  viewToCartRate: 0.1836, cartToCheckoutRate: 0.4675, checkoutToCompletionRate: 0.0905,
  overallConversionRate: 0.0078,
};

// ── Admin Configs ─────────────────────────────────────────
const adminConfigs = [
  { id: 'cfg-001', key: 'vat_rate', value: '0.14', category: 'tax', label: 'VAT Rate', labelAr: 'نسبة ضريبة القيمة المضافة', valueType: 'percentage', isActive: true, requiresApproval: true, sortOrder: 1 },
  { id: 'cfg-002', key: 'mvp_flat_rate', value: '0.20', category: 'uplift', label: 'MVP Flat Rate', labelAr: 'نسبة الهامش الثابت', valueType: 'percentage', isActive: true, requiresApproval: true, sortOrder: 1 },
  { id: 'cfg-003', key: 'escrow_hold_days', value: '14', category: 'escrow', label: 'Escrow Hold Period', labelAr: 'فترة الحجز', valueType: 'number', isActive: true, requiresApproval: true, sortOrder: 1 },
  { id: 'cfg-004', key: 'min_payout', value: '500', category: 'escrow', label: 'Minimum Payout', labelAr: 'الحد الأدنى للتحويل', valueType: 'number', isActive: true, requiresApproval: false, sortOrder: 2 },
];

// ── Audit Logs ────────────────────────────────────────────
const auditLogs = [
  { id: 'al-001', userId: 'admin-001', userName: 'كريم مطاط', action: 'config_updated', entityType: 'config', entityId: 'cfg-001', details: 'Updated VAT rate from 0.12 to 0.14', severity: 'warning', createdAt: '2026-02-07T10:00:00Z' },
  { id: 'al-002', userId: 'admin-001', userName: 'كريم مطاط', action: 'vendor_approved', entityType: 'vendor', entityId: 'v-003', details: 'Approved Siwa Pottery vendor application', severity: 'info', createdAt: '2026-02-06T14:30:00Z' },
  { id: 'al-003', userId: 'admin-001', userName: 'كريم مطاط', action: 'product_created', entityType: 'product', entityId: 'p-005', details: 'New product: Arabesque Wooden Trinket Box', severity: 'info', createdAt: '2026-01-20T14:00:00Z' },
];

// ── Approvals ─────────────────────────────────────────────
const approvals = [
  { id: 'req-001', vendorId: 'v-001', vendorName: 'Nile Crafts', actionType: 'product_listing', details: 'New product: Hand-painted Tea Set', status: 'pending', createdAt: '2026-02-07T09:00:00Z', reviewedAt: null, reviewNotes: null },
  { id: 'req-002', vendorId: 'v-002', vendorName: 'Desert Rose Leather', actionType: 'price_change', details: 'Price increase on Crossbody Bag: 450→520 EGP (+15.6%)', status: 'pending', createdAt: '2026-02-06T16:00:00Z', reviewedAt: null, reviewNotes: null },
  { id: 'req-003', vendorId: 'v-004', vendorName: 'Cairo Textiles Co.', actionType: 'sub_vendor_add', details: 'New sub-vendor: Cairo Textiles - Cushions', status: 'approved', createdAt: '2026-02-05T11:00:00Z', reviewedAt: '2026-02-05T14:00:00Z', reviewNotes: 'Approved. Good track record.' },
];

module.exports = {
  uuid,
  paginate,
  vendors,
  subVendors,
  products,
  orders,
  customers,
  wallets,
  dashboardStats,
  gmvTrend,
  categories,
  shippingZones,
  analyticsSummary,
  conversionFunnel,
  adminConfigs,
  auditLogs,
  approvals,
};
