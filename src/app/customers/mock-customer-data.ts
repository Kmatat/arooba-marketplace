/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer CRM Mock Data
 * ============================================================
 *
 * Comprehensive mock data for the Customer CRM module.
 * Covers customers, orders, reviews, performance, logins,
 * activity, and audit logs.
 * ============================================================
 */

import type {
  CustomerCRM,
  CustomerCRMStats,
  CustomerOrderCRM,
  CustomerReview,
  CustomerPerformance,
  CustomerLoginEntry,
  CustomerActivity,
  CustomerLogEntry,
} from './types';

// ──────────────────────────────────────────────
// CRM SUMMARY STATS
// ──────────────────────────────────────────────

export const mockCRMStats: CustomerCRMStats = {
  totalCustomers: 8420,
  activeCustomers: 6210,
  newThisMonth: 342,
  churnRate: 4.2,
  avgLifetimeValue: 2850,
  totalLoyaltyIssued: 1250000,
  avgEngagementScore: 68,
  topTierCount: 156,
};

// ──────────────────────────────────────────────
// CUSTOMER PROFILES
// ──────────────────────────────────────────────

export const mockCustomers: CustomerCRM[] = [
  {
    id: 'cust-001',
    userId: 'u-201',
    fullName: 'Ahmed Hassan',
    fullNameAr: 'أحمد حسن',
    mobileNumber: '+201012345678',
    email: 'ahmed.hassan@gmail.com',
    preferredLanguage: 'ar',
    status: 'active',
    tier: 'gold',
    loyaltyPoints: 4250,
    lifetimeLoyaltyPoints: 12800,
    walletBalance: 350,
    referralCode: 'AHMED50',
    referralCount: 8,
    totalOrders: 47,
    totalSpent: 12800,
    averageOrderValue: 272,
    lastOrderDate: '2026-02-05T14:30:00Z',
    totalReviews: 12,
    averageRating: 4.6,
    lastLoginAt: '2026-02-08T09:15:00Z',
    totalSessions: 234,
    registeredAt: '2025-10-15T08:00:00Z',
    addresses: [
      { id: 'addr-001', label: 'المنزل', fullAddress: '15 شارع النيل، المعادي', city: 'القاهرة', governorate: 'القاهرة', zoneId: 'zone-cairo', isDefault: true },
      { id: 'addr-002', label: 'العمل', fullAddress: '8 شارع التحرير، وسط البلد', city: 'القاهرة', governorate: 'القاهرة', zoneId: 'zone-cairo', isDefault: false },
    ],
  },
  {
    id: 'cust-002',
    userId: 'u-202',
    fullName: 'Fatima Al-Zahra',
    fullNameAr: 'فاطمة الزهراء',
    mobileNumber: '+201123456789',
    email: 'fatima.z@outlook.com',
    preferredLanguage: 'ar',
    status: 'active',
    tier: 'platinum',
    loyaltyPoints: 8900,
    lifetimeLoyaltyPoints: 28500,
    walletBalance: 1200,
    referralCode: 'FATIMA50',
    referredBy: 'AHMED50',
    referralCount: 15,
    totalOrders: 89,
    totalSpent: 28500,
    averageOrderValue: 320,
    lastOrderDate: '2026-02-07T16:45:00Z',
    totalReviews: 34,
    averageRating: 4.8,
    lastLoginAt: '2026-02-08T11:30:00Z',
    totalSessions: 512,
    registeredAt: '2025-10-20T10:00:00Z',
    addresses: [
      { id: 'addr-003', label: 'المنزل', fullAddress: '22 شارع الهرم، الجيزة', city: 'الجيزة', governorate: 'الجيزة', zoneId: 'zone-giza', isDefault: true },
    ],
  },
  {
    id: 'cust-003',
    userId: 'u-203',
    fullName: 'Mohamed Samir',
    fullNameAr: 'محمد سمير',
    mobileNumber: '+201234567890',
    email: 'mo.samir@yahoo.com',
    preferredLanguage: 'en',
    status: 'active',
    tier: 'silver',
    loyaltyPoints: 1800,
    lifetimeLoyaltyPoints: 5600,
    walletBalance: 0,
    referralCode: 'MOSAMIR50',
    referralCount: 3,
    totalOrders: 22,
    totalSpent: 5600,
    averageOrderValue: 255,
    lastOrderDate: '2026-01-28T12:00:00Z',
    totalReviews: 5,
    averageRating: 4.2,
    lastLoginAt: '2026-02-06T15:20:00Z',
    totalSessions: 98,
    registeredAt: '2025-11-05T14:00:00Z',
    addresses: [
      { id: 'addr-004', label: 'المنزل', fullAddress: '5 شارع كورنيش النيل، الإسكندرية', city: 'الإسكندرية', governorate: 'الإسكندرية', zoneId: 'zone-alex', isDefault: true },
    ],
  },
  {
    id: 'cust-004',
    userId: 'u-204',
    fullName: 'Nour El-Din',
    fullNameAr: 'نور الدين',
    mobileNumber: '+201098765432',
    preferredLanguage: 'ar',
    status: 'inactive',
    tier: 'bronze',
    loyaltyPoints: 200,
    lifetimeLoyaltyPoints: 800,
    walletBalance: 50,
    referralCode: 'NOUR50',
    referralCount: 0,
    totalOrders: 3,
    totalSpent: 800,
    averageOrderValue: 267,
    lastOrderDate: '2025-12-10T10:00:00Z',
    totalReviews: 1,
    averageRating: 3.0,
    lastLoginAt: '2025-12-15T08:00:00Z',
    totalSessions: 12,
    registeredAt: '2025-11-20T09:00:00Z',
    addresses: [
      { id: 'addr-005', label: 'المنزل', fullAddress: '12 شارع السلام، المنصورة', city: 'المنصورة', governorate: 'الدقهلية', zoneId: 'zone-delta', isDefault: true },
    ],
  },
  {
    id: 'cust-005',
    userId: 'u-205',
    fullName: 'Sara Mahmoud',
    fullNameAr: 'سارة محمود',
    mobileNumber: '+201156789012',
    email: 'sara.m@gmail.com',
    preferredLanguage: 'ar',
    status: 'active',
    tier: 'gold',
    loyaltyPoints: 5100,
    lifetimeLoyaltyPoints: 15200,
    walletBalance: 800,
    referralCode: 'SARA50',
    referredBy: 'FATIMA50',
    referralCount: 6,
    totalOrders: 56,
    totalSpent: 15200,
    averageOrderValue: 271,
    lastOrderDate: '2026-02-06T18:20:00Z',
    totalReviews: 18,
    averageRating: 4.5,
    lastLoginAt: '2026-02-07T20:00:00Z',
    totalSessions: 310,
    registeredAt: '2025-10-25T11:00:00Z',
    addresses: [
      { id: 'addr-006', label: 'المنزل', fullAddress: '7 شارع المعز، الحسين', city: 'القاهرة', governorate: 'القاهرة', zoneId: 'zone-cairo', isDefault: true },
      { id: 'addr-007', label: 'أهلي', fullAddress: '30 شارع الجلاء، طنطا', city: 'طنطا', governorate: 'الغربية', zoneId: 'zone-delta', isDefault: false },
    ],
  },
  {
    id: 'cust-006',
    userId: 'u-206',
    fullName: 'Khaled Youssef',
    fullNameAr: 'خالد يوسف',
    mobileNumber: '+201067890123',
    email: 'k.youssef@hotmail.com',
    preferredLanguage: 'ar',
    status: 'blocked',
    tier: 'bronze',
    loyaltyPoints: 0,
    lifetimeLoyaltyPoints: 2100,
    walletBalance: 0,
    referralCode: 'KHALED50',
    referralCount: 1,
    totalOrders: 8,
    totalSpent: 2100,
    averageOrderValue: 263,
    lastOrderDate: '2026-01-05T09:30:00Z',
    totalReviews: 2,
    averageRating: 2.0,
    lastLoginAt: '2026-01-10T07:00:00Z',
    totalSessions: 45,
    registeredAt: '2025-11-10T16:00:00Z',
    addresses: [
      { id: 'addr-008', label: 'المنزل', fullAddress: '18 شارع الملك فيصل، فيصل', city: 'الجيزة', governorate: 'الجيزة', zoneId: 'zone-giza', isDefault: true },
    ],
  },
  {
    id: 'cust-007',
    userId: 'u-207',
    fullName: 'Yasmin Abdel-Fattah',
    fullNameAr: 'ياسمين عبد الفتاح',
    mobileNumber: '+201278901234',
    email: 'yasmin.af@gmail.com',
    preferredLanguage: 'en',
    status: 'active',
    tier: 'silver',
    loyaltyPoints: 2300,
    lifetimeLoyaltyPoints: 7800,
    walletBalance: 150,
    referralCode: 'YASMIN50',
    referralCount: 4,
    totalOrders: 31,
    totalSpent: 7800,
    averageOrderValue: 252,
    lastOrderDate: '2026-02-04T13:15:00Z',
    totalReviews: 9,
    averageRating: 4.4,
    lastLoginAt: '2026-02-08T08:45:00Z',
    totalSessions: 178,
    registeredAt: '2025-10-30T13:00:00Z',
    addresses: [
      { id: 'addr-009', label: 'Home', fullAddress: '45 Road 9, Maadi', city: 'Cairo', governorate: 'Cairo', zoneId: 'zone-cairo', isDefault: true },
    ],
  },
  {
    id: 'cust-008',
    userId: 'u-208',
    fullName: 'Omar Fathy',
    fullNameAr: 'عمر فتحي',
    mobileNumber: '+201189012345',
    preferredLanguage: 'ar',
    status: 'churned',
    tier: 'bronze',
    loyaltyPoints: 50,
    lifetimeLoyaltyPoints: 450,
    walletBalance: 0,
    referralCode: 'OMAR50',
    referralCount: 0,
    totalOrders: 2,
    totalSpent: 450,
    averageOrderValue: 225,
    lastOrderDate: '2025-11-20T11:00:00Z',
    totalReviews: 0,
    averageRating: 0,
    lastLoginAt: '2025-11-25T14:00:00Z',
    totalSessions: 8,
    registeredAt: '2025-11-15T10:00:00Z',
    addresses: [
      { id: 'addr-010', label: 'المنزل', fullAddress: '3 شارع أسيوط، أسيوط', city: 'أسيوط', governorate: 'أسيوط', zoneId: 'zone-upper', isDefault: true },
    ],
  },
];

// ──────────────────────────────────────────────
// CUSTOMER ORDERS (for detail view)
// ──────────────────────────────────────────────

export const mockCustomerOrders: Record<string, CustomerOrderCRM[]> = {
  'cust-001': [
    {
      id: 'ord-101', orderNumber: 'ARB-2026-0047', status: 'delivered', totalAmount: 575,
      itemCount: 3, paymentMethod: 'card', deliveryAddress: '15 شارع النيل، المعادي',
      deliveryCity: 'القاهرة', createdAt: '2026-02-05T14:30:00Z', deliveredAt: '2026-02-07T11:00:00Z',
      items: [
        { id: 'oi-1', productTitle: 'طبق خزف مرسوم يدوياً', productImage: '', vendorName: 'خزفيات حسن', quantity: 2, unitPrice: 180, totalPrice: 360 },
        { id: 'oi-2', productTitle: 'شال حرير سيوة', productImage: '', vendorName: 'سيوة للمنسوجات', quantity: 1, unitPrice: 215, totalPrice: 215 },
      ],
    },
    {
      id: 'ord-098', orderNumber: 'ARB-2026-0044', status: 'in_transit', totalAmount: 320,
      itemCount: 1, paymentMethod: 'cod', deliveryAddress: '8 شارع التحرير، وسط البلد',
      deliveryCity: 'القاهرة', createdAt: '2026-02-03T10:20:00Z',
      items: [
        { id: 'oi-3', productTitle: 'حقيبة جلد طبيعي', productImage: '', vendorName: 'جلود خان الخليلي', quantity: 1, unitPrice: 320, totalPrice: 320 },
      ],
    },
    {
      id: 'ord-085', orderNumber: 'ARB-2026-0031', status: 'delivered', totalAmount: 450,
      itemCount: 2, paymentMethod: 'fawry', deliveryAddress: '15 شارع النيل، المعادي',
      deliveryCity: 'القاهرة', createdAt: '2026-01-22T16:00:00Z', deliveredAt: '2026-01-24T14:30:00Z',
      items: [
        { id: 'oi-4', productTitle: 'مفرش طاولة يدوي', productImage: '', vendorName: 'يدوية نادية', quantity: 1, unitPrice: 250, totalPrice: 250 },
        { id: 'oi-5', productTitle: 'مجموعة بهارات أسوان', productImage: '', vendorName: 'بهارات أسوان', quantity: 1, unitPrice: 200, totalPrice: 200 },
      ],
    },
    {
      id: 'ord-072', orderNumber: 'ARB-2026-0018', status: 'returned', totalAmount: 180,
      itemCount: 1, paymentMethod: 'card', deliveryAddress: '15 شارع النيل، المعادي',
      deliveryCity: 'القاهرة', createdAt: '2026-01-10T09:00:00Z', deliveredAt: '2026-01-12T10:00:00Z',
      items: [
        { id: 'oi-6', productTitle: 'طبق خزف صغير', productImage: '', vendorName: 'خزفيات حسن', quantity: 1, unitPrice: 180, totalPrice: 180 },
      ],
    },
    {
      id: 'ord-055', orderNumber: 'ARB-2025-0155', status: 'delivered', totalAmount: 680,
      itemCount: 4, paymentMethod: 'wallet', deliveryAddress: '15 شارع النيل، المعادي',
      deliveryCity: 'القاهرة', createdAt: '2025-12-20T13:45:00Z', deliveredAt: '2025-12-22T16:00:00Z',
      items: [
        { id: 'oi-7', productTitle: 'طقم أكواب نحاسية', productImage: '', vendorName: 'خزفيات حسن', quantity: 4, unitPrice: 170, totalPrice: 680 },
      ],
    },
  ],
  'cust-002': [
    {
      id: 'ord-102', orderNumber: 'ARB-2026-0048', status: 'accepted', totalAmount: 890,
      itemCount: 3, paymentMethod: 'card', deliveryAddress: '22 شارع الهرم، الجيزة',
      deliveryCity: 'الجيزة', createdAt: '2026-02-07T16:45:00Z',
      items: [
        { id: 'oi-8', productTitle: 'سجادة يدوية كبيرة', productImage: '', vendorName: 'سيوة للمنسوجات', quantity: 1, unitPrice: 650, totalPrice: 650 },
        { id: 'oi-9', productTitle: 'وسادة مطرزة', productImage: '', vendorName: 'يدوية نادية', quantity: 2, unitPrice: 120, totalPrice: 240 },
      ],
    },
  ],
};

// ──────────────────────────────────────────────
// CUSTOMER REVIEWS
// ──────────────────────────────────────────────

export const mockCustomerReviews: Record<string, CustomerReview[]> = {
  'cust-001': [
    {
      id: 'rev-001', customerId: 'cust-001', orderId: 'ord-101', orderNumber: 'ARB-2026-0047',
      productId: 'p-001', productTitle: 'طبق خزف مرسوم يدوياً', productImage: '',
      vendorName: 'خزفيات حسن', rating: 5,
      reviewText: 'جودة ممتازة والألوان حلوة جداً. التغليف كان محترم والتوصيل سريع.',
      isVerifiedPurchase: true, helpfulCount: 12, status: 'published',
      createdAt: '2026-02-07T12:00:00Z',
    },
    {
      id: 'rev-002', customerId: 'cust-001', orderId: 'ord-101', orderNumber: 'ARB-2026-0047',
      productId: 'p-002', productTitle: 'شال حرير سيوة', productImage: '',
      vendorName: 'سيوة للمنسوجات', rating: 4,
      reviewText: 'الشال حلو بس اللون مختلف شوية عن الصورة. في المجمل راضي.',
      isVerifiedPurchase: true, helpfulCount: 5, status: 'published',
      createdAt: '2026-02-07T12:15:00Z',
    },
    {
      id: 'rev-003', customerId: 'cust-001', orderId: 'ord-085', orderNumber: 'ARB-2026-0031',
      productId: 'p-003', productTitle: 'مفرش طاولة يدوي', productImage: '',
      vendorName: 'يدوية نادية', rating: 5,
      reviewText: 'شغل يدوي ممتاز. هدية رائعة لأمي.',
      isVerifiedPurchase: true, helpfulCount: 8, status: 'published',
      createdAt: '2026-01-25T10:00:00Z',
    },
    {
      id: 'rev-004', customerId: 'cust-001', orderId: 'ord-072', orderNumber: 'ARB-2026-0018',
      productId: 'p-004', productTitle: 'طبق خزف صغير', productImage: '',
      vendorName: 'خزفيات حسن', rating: 2,
      reviewText: 'وصل مكسور. طلبت إرجاع.',
      isVerifiedPurchase: true, helpfulCount: 3, status: 'published',
      adminReply: 'نعتذر عن ذلك. تم معالجة طلب الإرجاع وإعادة المبلغ.',
      createdAt: '2026-01-13T08:00:00Z',
    },
  ],
  'cust-002': [
    {
      id: 'rev-005', customerId: 'cust-002', orderId: 'ord-090', orderNumber: 'ARB-2026-0036',
      productId: 'p-001', productTitle: 'طبق خزف مرسوم يدوياً', productImage: '',
      vendorName: 'خزفيات حسن', rating: 5,
      reviewText: 'من أجمل المنتجات اللي اشتريتها. فنان فعلاً!',
      isVerifiedPurchase: true, helpfulCount: 20, status: 'published',
      createdAt: '2026-01-30T14:00:00Z',
    },
  ],
};

// ──────────────────────────────────────────────
// CUSTOMER PERFORMANCE
// ──────────────────────────────────────────────

export const mockCustomerPerformance: Record<string, CustomerPerformance> = {
  'cust-001': {
    customerId: 'cust-001',
    monthlySpending: [
      { month: '2025-10', amount: 850 },
      { month: '2025-11', amount: 1200 },
      { month: '2025-12', amount: 2100 },
      { month: '2026-01', amount: 1650 },
      { month: '2026-02', amount: 895 },
    ],
    categoryBreakdown: [
      { category: 'ديكور منزلي', amount: 5120, percentage: 40 },
      { category: 'منسوجات', amount: 3200, percentage: 25 },
      { category: 'جلود', amount: 2560, percentage: 20 },
      { category: 'بهارات وأغذية', amount: 1280, percentage: 10 },
      { category: 'مجوهرات', amount: 640, percentage: 5 },
    ],
    engagementScore: 82,
    recencyScore: 95,
    frequencyScore: 78,
    monetaryScore: 72,
    pointsEarned: 12800,
    pointsRedeemed: 8550,
    pointsBalance: 4250,
    tierProgress: 72,
    nextTier: 'platinum',
    pointsToNextTier: 7200,
    referralsSent: 15,
    referralsConverted: 8,
    referralEarnings: 400,
    totalReturns: 1,
    returnRate: 2.1,
    averageCartSize: 2.8,
    cartAbandonmentRate: 15,
  },
  'cust-002': {
    customerId: 'cust-002',
    monthlySpending: [
      { month: '2025-10', amount: 2200 },
      { month: '2025-11', amount: 3500 },
      { month: '2025-12', amount: 5800 },
      { month: '2026-01', amount: 4200 },
      { month: '2026-02', amount: 890 },
    ],
    categoryBreakdown: [
      { category: 'منسوجات', amount: 11400, percentage: 40 },
      { category: 'ديكور منزلي', amount: 8550, percentage: 30 },
      { category: 'أزياء', amount: 5700, percentage: 20 },
      { category: 'مجوهرات', amount: 2850, percentage: 10 },
    ],
    engagementScore: 95,
    recencyScore: 98,
    frequencyScore: 92,
    monetaryScore: 95,
    pointsEarned: 28500,
    pointsRedeemed: 19600,
    pointsBalance: 8900,
    tierProgress: 100,
    nextTier: 'platinum',
    pointsToNextTier: 0,
    referralsSent: 25,
    referralsConverted: 15,
    referralEarnings: 750,
    totalReturns: 2,
    returnRate: 2.2,
    averageCartSize: 3.4,
    cartAbandonmentRate: 8,
  },
};

// ──────────────────────────────────────────────
// CUSTOMER LOGIN HISTORY
// ──────────────────────────────────────────────

export const mockCustomerLogins: Record<string, CustomerLoginEntry[]> = {
  'cust-001': [
    { id: 'login-001', customerId: 'cust-001', timestamp: '2026-02-08T09:15:00Z', status: 'success', ipAddress: '197.48.12.55', deviceType: 'mobile', deviceInfo: 'iPhone 15 Pro / iOS 18 / Safari', location: 'القاهرة، مصر', sessionDuration: 45 },
    { id: 'login-002', customerId: 'cust-001', timestamp: '2026-02-07T20:30:00Z', status: 'success', ipAddress: '197.48.12.55', deviceType: 'mobile', deviceInfo: 'iPhone 15 Pro / iOS 18 / Safari', location: 'القاهرة، مصر', sessionDuration: 22 },
    { id: 'login-003', customerId: 'cust-001', timestamp: '2026-02-07T08:00:00Z', status: 'success', ipAddress: '41.32.100.12', deviceType: 'desktop', deviceInfo: 'Chrome 122 / Windows 11', location: 'القاهرة، مصر', sessionDuration: 15 },
    { id: 'login-004', customerId: 'cust-001', timestamp: '2026-02-06T14:45:00Z', status: 'failed', ipAddress: '102.22.33.44', deviceType: 'mobile', deviceInfo: 'Unknown Device', location: 'الإسكندرية، مصر' },
    { id: 'login-005', customerId: 'cust-001', timestamp: '2026-02-05T10:00:00Z', status: 'success', ipAddress: '197.48.12.55', deviceType: 'mobile', deviceInfo: 'iPhone 15 Pro / iOS 18 / Safari', location: 'القاهرة، مصر', sessionDuration: 60 },
    { id: 'login-006', customerId: 'cust-001', timestamp: '2026-02-04T18:20:00Z', status: 'success', ipAddress: '197.48.12.55', deviceType: 'tablet', deviceInfo: 'iPad Air / iPadOS 18 / Safari', location: 'القاهرة، مصر', sessionDuration: 35 },
    { id: 'login-007', customerId: 'cust-001', timestamp: '2026-02-03T09:30:00Z', status: 'success', ipAddress: '41.32.100.12', deviceType: 'desktop', deviceInfo: 'Chrome 122 / Windows 11', location: 'القاهرة، مصر', sessionDuration: 28 },
    { id: 'login-008', customerId: 'cust-001', timestamp: '2026-02-02T21:00:00Z', status: 'blocked', ipAddress: '156.78.90.11', deviceType: 'desktop', deviceInfo: 'Firefox 123 / Linux', location: 'غير معروف' },
  ],
};

// ──────────────────────────────────────────────
// CUSTOMER ACTIVITY LOG
// ──────────────────────────────────────────────

export const mockCustomerActivities: Record<string, CustomerActivity[]> = {
  'cust-001': [
    { id: 'act-001', customerId: 'cust-001', action: 'purchase', description: 'Completed order ARB-2026-0047', descriptionAr: 'أتم طلب ARB-2026-0047', orderId: 'ord-101', orderNumber: 'ARB-2026-0047', sessionId: 'sess-234', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-05T14:30:00Z' },
    { id: 'act-002', customerId: 'cust-001', action: 'checkout_start', description: 'Started checkout with 3 items', descriptionAr: 'بدأ عملية الدفع بـ 3 منتجات', sessionId: 'sess-234', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-05T14:25:00Z' },
    { id: 'act-003', customerId: 'cust-001', action: 'add_to_cart', description: 'Added "Siwa Silk Shawl" to cart', descriptionAr: 'أضاف "شال حرير سيوة" للسلة', productId: 'p-002', productTitle: 'شال حرير سيوة', sessionId: 'sess-234', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-05T14:10:00Z' },
    { id: 'act-004', customerId: 'cust-001', action: 'product_view', description: 'Viewed "Hand-painted Ceramic Plate"', descriptionAr: 'شاهد "طبق خزف مرسوم يدوياً"', productId: 'p-001', productTitle: 'طبق خزف مرسوم يدوياً', sessionId: 'sess-234', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-05T14:05:00Z' },
    { id: 'act-005', customerId: 'cust-001', action: 'search', description: 'Searched for "خزف"', descriptionAr: 'بحث عن "خزف"', metadata: { query: 'خزف', results: '12' }, sessionId: 'sess-234', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-05T14:00:00Z' },
    { id: 'act-006', customerId: 'cust-001', action: 'review_submit', description: 'Submitted review for "Hand-painted Ceramic Plate"', descriptionAr: 'كتب تقييم لـ "طبق خزف مرسوم يدوياً"', productId: 'p-001', productTitle: 'طبق خزف مرسوم يدوياً', sessionId: 'sess-233', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-07T12:00:00Z' },
    { id: 'act-007', customerId: 'cust-001', action: 'referral_share', description: 'Shared referral code AHMED50', descriptionAr: 'شارك كود الإحالة AHMED50', metadata: { channel: 'whatsapp' }, sessionId: 'sess-230', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-02-03T16:00:00Z' },
    { id: 'act-008', customerId: 'cust-001', action: 'wallet_topup', description: 'Added 200 EGP to wallet via Fawry', descriptionAr: 'أضاف 200 ج.م للمحفظة عبر فوري', metadata: { amount: '200', method: 'fawry' }, sessionId: 'sess-228', deviceType: 'desktop', ipAddress: '41.32.100.12', timestamp: '2026-02-01T11:30:00Z' },
    { id: 'act-009', customerId: 'cust-001', action: 'profile_update', description: 'Updated email address', descriptionAr: 'حدّث البريد الإلكتروني', sessionId: 'sess-225', deviceType: 'desktop', ipAddress: '41.32.100.12', timestamp: '2026-01-28T09:15:00Z' },
    { id: 'act-010', customerId: 'cust-001', action: 'wishlist_add', description: 'Added "Leather Bag" to wishlist', descriptionAr: 'أضاف "حقيبة جلد" للمفضلة', productId: 'p-005', productTitle: 'حقيبة جلد طبيعي', sessionId: 'sess-220', deviceType: 'mobile', ipAddress: '197.48.12.55', timestamp: '2026-01-25T19:00:00Z' },
  ],
};

// ──────────────────────────────────────────────
// CUSTOMER AUDIT LOGS
// ──────────────────────────────────────────────

export const mockCustomerLogs: Record<string, CustomerLogEntry[]> = {
  'cust-001': [
    { id: 'log-001', customerId: 'cust-001', action: 'tier_upgraded', severity: 'info', description: 'Customer upgraded from Silver to Gold tier', descriptionAr: 'ترقية العميل من الفضي إلى الذهبي', performedBy: 'system', performedByRole: 'system', oldValues: { tier: 'silver' }, newValues: { tier: 'gold' }, timestamp: '2026-01-15T00:00:00Z' },
    { id: 'log-002', customerId: 'cust-001', action: 'wallet_credited', severity: 'info', description: 'Wallet credited 200 EGP via Fawry', descriptionAr: 'تم إيداع 200 ج.م بالمحفظة عبر فوري', performedBy: 'customer', performedByRole: 'customer', newValues: { amount: '200', method: 'fawry', balance: '350' }, timestamp: '2026-02-01T11:30:00Z' },
    { id: 'log-003', customerId: 'cust-001', action: 'order_returned', severity: 'warning', description: 'Order ARB-2026-0018 returned - item arrived broken', descriptionAr: 'تم إرجاع طلب ARB-2026-0018 - المنتج وصل مكسور', performedBy: 'customer', performedByRole: 'customer', newValues: { orderId: 'ord-072', reason: 'damaged' }, timestamp: '2026-01-13T08:00:00Z' },
    { id: 'log-004', customerId: 'cust-001', action: 'loyalty_earned', severity: 'info', description: 'Earned 575 loyalty points from order ARB-2026-0047', descriptionAr: 'حصل على 575 نقطة ولاء من طلب ARB-2026-0047', performedBy: 'system', performedByRole: 'system', newValues: { points: '575', orderId: 'ord-101', balance: '4250' }, timestamp: '2026-02-07T11:00:00Z' },
    { id: 'log-005', customerId: 'cust-001', action: 'referral_applied', severity: 'info', description: 'Referral code AHMED50 used by new customer', descriptionAr: 'تم استخدام كود الإحالة AHMED50 من عميل جديد', performedBy: 'system', performedByRole: 'system', newValues: { referredCustomer: 'cust-009', bonus: '50' }, timestamp: '2026-02-03T16:30:00Z' },
    { id: 'log-006', customerId: 'cust-001', action: 'profile_updated', severity: 'info', description: 'Customer updated email address', descriptionAr: 'العميل حدّث البريد الإلكتروني', performedBy: 'customer', performedByRole: 'customer', oldValues: { email: 'ahmed.h@gmail.com' }, newValues: { email: 'ahmed.hassan@gmail.com' }, ipAddress: '41.32.100.12', timestamp: '2026-01-28T09:15:00Z' },
    { id: 'log-007', customerId: 'cust-001', action: 'address_added', severity: 'info', description: 'Added new delivery address "Work"', descriptionAr: 'أضاف عنوان توصيل جديد "العمل"', performedBy: 'customer', performedByRole: 'customer', newValues: { label: 'العمل', address: '8 شارع التحرير' }, timestamp: '2025-11-10T14:00:00Z' },
    { id: 'log-008', customerId: 'cust-001', action: 'account_created', severity: 'info', description: 'Customer account created via mobile registration', descriptionAr: 'تم إنشاء حساب العميل عبر التسجيل بالموبايل', performedBy: 'system', performedByRole: 'system', newValues: { mobile: '+201012345678', method: 'otp' }, timestamp: '2025-10-15T08:00:00Z' },
  ],
  'cust-006': [
    { id: 'log-020', customerId: 'cust-006', action: 'account_blocked', severity: 'critical', description: 'Account blocked due to suspicious activity', descriptionAr: 'تم حظر الحساب بسبب نشاط مشبوه', performedBy: 'كريم مطاط', performedByRole: 'admin_super', newValues: { reason: 'multiple_chargebacks', chargebacks: '3' }, ipAddress: '10.0.0.1', timestamp: '2026-01-10T07:00:00Z' },
    { id: 'log-021', customerId: 'cust-006', action: 'order_cancelled', severity: 'warning', description: 'Order ARB-2026-0005 cancelled by customer', descriptionAr: 'تم إلغاء طلب ARB-2026-0005 من العميل', performedBy: 'customer', performedByRole: 'customer', newValues: { orderId: 'ord-060', reason: 'changed_mind' }, timestamp: '2026-01-05T10:00:00Z' },
  ],
};
