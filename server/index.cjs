/**
 * AROOBA MARKETPLACE — Local Development API Server
 *
 * A lightweight Express server that mocks the ASP.NET Core backend API.
 * This allows the frontend to run against realistic API responses without
 * needing Docker, .NET SDK, or SQL Server.
 *
 * Endpoints mirror the backend controllers:
 *   /api/auth, /api/products, /api/orders, /api/vendors,
 *   /api/customers, /api/finance, /api/dashboard, /api/analytics,
 *   /api/categories, /api/shipping, /api/pricing, /api/approvals,
 *   /api/auditlogs, /api/adminconfig, /health
 */

const express = require('express');
const cors = require('cors');
const data = require('./mock-data.cjs');

const app = express();
const PORT = process.env.API_PORT || 5000;

// ── Middleware ─────────────────────────────────────────────
app.use(cors());
app.use(express.json());

// Request logging
app.use((req, _res, next) => {
  console.log(`${new Date().toISOString()} ${req.method} ${req.path}`);
  next();
});

// ── Health Check ──────────────────────────────────────────
app.get('/health', (_req, res) => {
  res.json({ status: 'Healthy', timestamp: new Date().toISOString(), service: 'Arooba Mock API' });
});

// ══════════════════════════════════════════════════════════
// AUTH — /api/auth
// ══════════════════════════════════════════════════════════
app.post('/api/auth/register', (req, res) => {
  const { name, mobileNumber } = req.body;
  res.status(201).json({ userId: data.uuid(), otpDeliveryStatus: 'sent', mobileNumber });
});

app.post('/api/auth/send-otp', (req, res) => {
  res.json({ deliveryStatus: 'sent', mobileNumber: req.body.mobileNumber });
});

app.post('/api/auth/verify-otp', (_req, res) => {
  res.json({
    accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock-token',
    refreshToken: data.uuid(),
    expiresAt: new Date(Date.now() + 3600000).toISOString(),
    userId: 'admin-001',
    userRole: 'admin_super',
  });
});

app.post('/api/auth/login', (_req, res) => {
  res.json({
    accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock-token',
    refreshToken: data.uuid(),
    expiresAt: new Date(Date.now() + 3600000).toISOString(),
    userId: 'admin-001',
    userRole: 'admin_super',
  });
});

app.post('/api/auth/social-login', (req, res) => {
  res.json({
    token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock-social-token',
    requiresMobileVerification: !req.body.mobileNumber,
  });
});

app.post('/api/auth/refresh', (_req, res) => {
  res.json({
    accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock-refreshed',
    refreshToken: data.uuid(),
    expiresAt: new Date(Date.now() + 3600000).toISOString(),
  });
});

// ══════════════════════════════════════════════════════════
// PRODUCTS — /api/products
// ══════════════════════════════════════════════════════════
app.get('/api/products', (req, res) => {
  let filtered = [...data.products];
  if (req.query.category) filtered = filtered.filter(p => p.categoryId === req.query.category);
  if (req.query.vendor) filtered = filtered.filter(p => p.parentVendorId === req.query.vendor);
  if (req.query.status) filtered = filtered.filter(p => p.status === req.query.status);
  if (req.query.search) {
    const s = req.query.search.toLowerCase();
    filtered = filtered.filter(p => p.title.toLowerCase().includes(s) || p.titleAr.includes(s));
  }
  if (req.query.minPrice) filtered = filtered.filter(p => p.finalPrice >= Number(req.query.minPrice));
  if (req.query.maxPrice) filtered = filtered.filter(p => p.finalPrice <= Number(req.query.maxPrice));
  res.json(data.paginate(filtered, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 10));
});

app.get('/api/products/:id', (req, res) => {
  const product = data.products.find(p => p.id === req.params.id);
  if (!product) return res.status(404).json({ error: 'Product not found' });
  res.json(product);
});

app.post('/api/products', (req, res) => {
  const id = data.uuid();
  data.products.push({ id, ...req.body, status: 'draft', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() });
  res.status(201).json(id);
});

app.put('/api/products/:id', (req, res) => {
  const idx = data.products.findIndex(p => p.id === req.params.id);
  if (idx === -1) return res.status(404).json({ error: 'Product not found' });
  data.products[idx] = { ...data.products[idx], ...req.body, updatedAt: new Date().toISOString() };
  res.sendStatus(204);
});

app.patch('/api/products/:id/status', (req, res) => {
  const product = data.products.find(p => p.id === req.params.id);
  if (!product) return res.status(404).json({ error: 'Product not found' });
  product.status = req.body.newStatus;
  product.updatedAt = new Date().toISOString();
  res.sendStatus(204);
});

// ══════════════════════════════════════════════════════════
// ORDERS — /api/orders
// ══════════════════════════════════════════════════════════
app.get('/api/orders', (req, res) => {
  let filtered = [...data.orders];
  if (req.query.status) filtered = filtered.filter(o => o.status === req.query.status);
  if (req.query.customerId) filtered = filtered.filter(o => o.customerId === req.query.customerId);
  if (req.query.vendorId) filtered = filtered.filter(o => o.items.some(i => i.vendorId === req.query.vendorId));
  res.json(data.paginate(filtered, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 10));
});

app.get('/api/orders/:id', (req, res) => {
  const order = data.orders.find(o => o.id === req.params.id);
  if (!order) return res.status(404).json({ error: 'Order not found' });
  res.json(order);
});

app.post('/api/orders', (req, res) => {
  const id = data.uuid();
  data.orders.push({ id, ...req.body, status: 'pending', shipments: [], createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() });
  res.status(201).json(id);
});

app.patch('/api/orders/:id/status', (req, res) => {
  const order = data.orders.find(o => o.id === req.params.id);
  if (!order) return res.status(404).json({ error: 'Order not found' });
  order.status = req.body.newStatus;
  order.updatedAt = new Date().toISOString();
  res.sendStatus(204);
});

// ══════════════════════════════════════════════════════════
// VENDORS — /api/vendors
// ══════════════════════════════════════════════════════════
app.get('/api/vendors', (req, res) => {
  let filtered = [...data.vendors];
  if (req.query.status) filtered = filtered.filter(v => v.status === req.query.status);
  if (req.query.type) filtered = filtered.filter(v => v.type === req.query.type);
  if (req.query.search) {
    const s = req.query.search.toLowerCase();
    filtered = filtered.filter(v => v.businessName.toLowerCase().includes(s) || v.businessNameAr.includes(s));
  }
  res.json(data.paginate(filtered, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 10));
});

app.get('/api/vendors/:id', (req, res) => {
  const vendor = data.vendors.find(v => v.id === req.params.id);
  if (!vendor) return res.status(404).json({ error: 'Vendor not found' });
  const subs = data.subVendors.filter(sv => sv.parentVendorId === vendor.id);
  res.json({ ...vendor, subVendors: subs });
});

app.post('/api/vendors', (req, res) => {
  const id = data.uuid();
  data.vendors.push({ id, ...req.body, status: 'pending', reliabilityStrikes: 0, averageRating: 0, totalOrders: 0, totalRevenue: 0, subVendorIds: [], createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() });
  res.status(201).json(id);
});

app.put('/api/vendors/:id', (req, res) => {
  const idx = data.vendors.findIndex(v => v.id === req.params.id);
  if (idx === -1) return res.status(404).json({ error: 'Vendor not found' });
  data.vendors[idx] = { ...data.vendors[idx], ...req.body, updatedAt: new Date().toISOString() };
  res.sendStatus(204);
});

app.get('/api/vendors/:id/sub-vendors', (req, res) => {
  const subs = data.subVendors.filter(sv => sv.parentVendorId === req.params.id);
  res.json(subs);
});

app.post('/api/vendors/:id/sub-vendors', (req, res) => {
  const id = data.uuid();
  data.subVendors.push({ id, parentVendorId: req.params.id, ...req.body, isActive: true, createdAt: new Date().toISOString() });
  res.status(201).json(id);
});

// ══════════════════════════════════════════════════════════
// CUSTOMERS — /api/customers
// ══════════════════════════════════════════════════════════
app.get('/api/customers', (req, res) => {
  let filtered = [...data.customers];
  if (req.query.search) {
    const s = req.query.search.toLowerCase();
    filtered = filtered.filter(c => c.fullName.toLowerCase().includes(s) || c.fullNameAr.includes(s) || c.mobileNumber.includes(s));
  }
  res.json(data.paginate(filtered, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 10));
});

app.get('/api/customers/:id', (req, res) => {
  const customer = data.customers.find(c => c.id === req.params.id);
  if (!customer) return res.status(404).json({ error: 'Customer not found' });
  res.json(customer);
});

app.post('/api/customers', (req, res) => {
  const id = data.uuid();
  data.customers.push({ id, ...req.body, status: 'active', tier: 'bronze', loyaltyPoints: 0, lifetimeLoyaltyPoints: 0, walletBalance: 0, totalOrders: 0, totalSpent: 0, averageOrderValue: 0, registeredAt: new Date().toISOString(), addresses: [] });
  res.status(201).json(id);
});

app.put('/api/customers/:id', (req, res) => {
  const idx = data.customers.findIndex(c => c.id === req.params.id);
  if (idx === -1) return res.status(404).json({ error: 'Customer not found' });
  data.customers[idx] = { ...data.customers[idx], ...req.body };
  res.sendStatus(204);
});

app.put('/api/customers/:id/status', (req, res) => {
  const customer = data.customers.find(c => c.id === req.params.id);
  if (!customer) return res.status(404).json({ error: 'Customer not found' });
  customer.status = req.body.newStatus;
  res.sendStatus(204);
});

app.get('/api/customers/:id/addresses', (req, res) => {
  const customer = data.customers.find(c => c.id === req.params.id);
  if (!customer) return res.status(404).json({ error: 'Customer not found' });
  res.json(customer.addresses);
});

app.post('/api/customers/:id/addresses', (req, res) => {
  const customer = data.customers.find(c => c.id === req.params.id);
  if (!customer) return res.status(404).json({ error: 'Customer not found' });
  const id = data.uuid();
  customer.addresses.push({ id, ...req.body });
  res.status(201).json(id);
});

app.get('/api/customers/:id/reviews', (_req, res) => {
  res.json([
    { id: data.uuid(), productId: 'p-001', productTitle: 'Handmade Ceramic Vase', rating: 5, reviewText: 'منتج رائع!', isVerifiedPurchase: true, helpfulCount: 12, status: 'approved', createdAt: '2026-01-20T10:00:00Z' },
    { id: data.uuid(), productId: 'p-003', productTitle: 'Hand-woven Cotton Scarf', rating: 4, reviewText: 'جودة ممتازة', isVerifiedPurchase: true, helpfulCount: 5, status: 'approved', createdAt: '2026-01-25T14:00:00Z' },
  ]);
});

app.post('/api/customers/:id/reviews', (_req, res) => {
  res.status(201).json(data.uuid());
});

app.get('/api/customers/:id/performance', (_req, res) => {
  res.json({
    monthlySpending: [
      { month: '2025-10', amount: 1200 }, { month: '2025-11', amount: 2100 },
      { month: '2025-12', amount: 3500 }, { month: '2026-01', amount: 2800 },
      { month: '2026-02', amount: 1900 },
    ],
    categoryBreakdown: [
      { category: 'home-decor-fragile', percentage: 35 }, { category: 'leather-goods', percentage: 28 },
      { category: 'home-decor-textiles', percentage: 20 }, { category: 'jewelry-accessories', percentage: 17 },
    ],
    engagementScore: 82, recencyScore: 90, frequencyScore: 75, monetaryScore: 88,
    pointsEarned: 8200, pointsRedeemed: 5750, pointsBalance: 2450,
    tierProgress: 0.72, nextTier: 'platinum', pointsToNextTier: 1800,
    referralsSent: 8, referralsConverted: 5, referralEarnings: 250,
    totalReturns: 2, returnRate: 0.048,
  });
});

app.get('/api/customers/:id/logins', (_req, res) => {
  res.json([
    { timestamp: '2026-02-08T08:30:00Z', status: 'success', ipAddress: '41.32.xx.xx', deviceType: 'mobile', deviceInfo: 'iPhone 14, iOS 17', location: 'Cairo', sessionDuration: 1245 },
    { timestamp: '2026-02-07T20:15:00Z', status: 'success', ipAddress: '41.32.xx.xx', deviceType: 'mobile', deviceInfo: 'iPhone 14, iOS 17', location: 'Cairo', sessionDuration: 890 },
  ]);
});

app.get('/api/customers/:id/activity', (_req, res) => {
  res.json([
    { id: data.uuid(), action: 'purchase_completed', description: 'Completed order #ORD-001', descriptionAr: 'إتمام الطلب #ORD-001', productId: null, orderId: 'ord-001', createdAt: '2026-02-07T11:20:00Z' },
    { id: data.uuid(), action: 'product_viewed', description: 'Viewed: Silver Bedouin Necklace', descriptionAr: 'شاهد: عقد بدوي فضي', productId: 'p-004', orderId: null, createdAt: '2026-02-07T11:15:00Z' },
  ]);
});

app.get('/api/customers/:id/audit-logs', (_req, res) => {
  res.json([
    { id: data.uuid(), action: 'tier_upgraded', details: 'Tier upgraded from Silver to Gold', severity: 'info', createdAt: '2026-01-15T10:00:00Z', performedBy: 'system' },
    { id: data.uuid(), action: 'wallet_credited', details: 'Referral bonus: +50 EGP', severity: 'info', createdAt: '2026-01-10T14:30:00Z', performedBy: 'system' },
  ]);
});

// ══════════════════════════════════════════════════════════
// FINANCE — /api/finance
// ══════════════════════════════════════════════════════════
app.get('/api/finance/wallets/:vendorId', (req, res) => {
  const wallet = data.wallets.find(w => w.vendorId === req.params.vendorId);
  if (!wallet) return res.status(404).json({ error: 'Wallet not found' });
  res.json(wallet);
});

app.get('/api/finance/ledger/:vendorId', (req, res) => {
  const entries = [
    { id: data.uuid(), vendorId: req.params.vendorId, transactionType: 'order_revenue', amount: 586.88, runningBalance: 45200, relatedOrderId: 'ord-001', notes: 'Revenue from order ORD-001', createdAt: '2026-01-17T14:30:00Z' },
    { id: data.uuid(), vendorId: req.params.vendorId, transactionType: 'payout', amount: -5000, runningBalance: 40200, relatedPayoutId: 'po-001', notes: 'Weekly payout batch', createdAt: '2026-01-20T10:00:00Z' },
  ];
  res.json(data.paginate(entries, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 20));
});

app.get('/api/finance/splits/:orderId', (req, res) => {
  const order = data.orders.find(o => o.id === req.params.orderId);
  if (!order) return res.status(404).json({ error: 'Order not found' });
  res.json(order.items.map(item => ({
    orderItemId: item.id,
    productTitle: item.productTitle,
    ...item.bucketSplit,
    total: Object.values(item.bucketSplit).reduce((sum, v) => sum + v, 0),
  })));
});

app.post('/api/finance/payouts', (req, res) => {
  const wallet = data.wallets.find(w => w.vendorId === req.body.vendorId);
  if (!wallet) return res.status(404).json({ error: 'Wallet not found' });
  if (wallet.availableBalance < 500) return res.status(400).json({ error: 'Below minimum payout threshold (500 EGP)' });
  const amount = req.body.amount || wallet.availableBalance;
  wallet.availableBalance -= amount;
  wallet.totalBalance -= amount;
  res.status(201).json(data.uuid());
});

// ══════════════════════════════════════════════════════════
// DASHBOARD — /api/dashboard
// ══════════════════════════════════════════════════════════
app.get('/api/dashboard/stats', (_req, res) => {
  res.json(data.dashboardStats);
});

app.get('/api/dashboard/gmv-trend', (req, res) => {
  const months = Math.min(Number(req.query.months) || 6, 24);
  res.json(data.gmvTrend.slice(-months));
});

// ══════════════════════════════════════════════════════════
// ANALYTICS — /api/analytics
// ══════════════════════════════════════════════════════════
app.post('/api/analytics/track', (_req, res) => {
  res.status(201).json(data.uuid());
});

app.get('/api/analytics/summary', (_req, res) => {
  res.json(data.analyticsSummary);
});

app.get('/api/analytics/conversion-funnel', (_req, res) => {
  res.json(data.conversionFunnel);
});

app.get('/api/analytics/products', (_req, res) => {
  const productAnalytics = data.products.map(p => ({
    productId: p.id, productTitle: p.title, productTitleAr: p.titleAr, categoryId: p.categoryId,
    views: Math.floor(Math.random() * 5000) + 500,
    addedToCart: Math.floor(Math.random() * 1000) + 100,
    purchases: Math.floor(Math.random() * 200) + 10,
    conversionRate: (Math.random() * 0.1 + 0.02).toFixed(4),
    relatedProductClicks: Math.floor(Math.random() * 300) + 50,
  }));
  res.json({ items: productAnalytics, totalCount: productAnalytics.length });
});

app.get('/api/analytics/activity-log', (req, res) => {
  const activities = [
    { id: data.uuid(), userId: 'cust-001', action: 'purchase_completed', productId: 'p-001', orderId: 'ord-001', searchQuery: null, timestamp: '2026-02-07T11:20:00Z', deviceType: 'mobile' },
    { id: data.uuid(), userId: 'cust-002', action: 'added_to_cart', productId: 'p-002', orderId: null, searchQuery: null, timestamp: '2026-02-07T10:45:00Z', deviceType: 'mobile' },
    { id: data.uuid(), userId: 'cust-003', action: 'product_searched', productId: null, orderId: null, searchQuery: 'حقيبة جلد', timestamp: '2026-02-07T10:30:00Z', deviceType: 'desktop' },
  ];
  res.json(data.paginate(activities, Number(req.query.page) || 1, Number(req.query.pageSize) || 50));
});

// ══════════════════════════════════════════════════════════
// CATEGORIES — /api/categories
// ══════════════════════════════════════════════════════════
app.get('/api/categories', (_req, res) => {
  res.json(data.categories);
});

app.get('/api/categories/:id', (req, res) => {
  const cat = data.categories.find(c => c.id === req.params.id);
  if (!cat) return res.status(404).json({ error: 'Category not found' });
  res.json(cat);
});

// ══════════════════════════════════════════════════════════
// SHIPPING — /api/shipping
// ══════════════════════════════════════════════════════════
app.get('/api/shipping/zones', (_req, res) => {
  res.json(data.shippingZones);
});

app.get('/api/shipping/rates', (_req, res) => {
  res.json(data.shippingZones.map(z => ({ zoneId: z.id, zoneName: z.name, basePrice: z.basePrice, pricePerKg: z.pricePerKg })));
});

app.post('/api/shipping/calculate-fee', (req, res) => {
  const { zoneId, weightKg, dimensionL, dimensionW, dimensionH } = req.body;
  const zone = data.shippingZones.find(z => z.id === zoneId) || data.shippingZones[0];
  const volumetricWeight = (dimensionL || 0) * (dimensionW || 0) * (dimensionH || 0) / 5000;
  const chargeableWeight = Math.max(weightKg || 0, volumetricWeight);
  const fee = zone.basePrice + (chargeableWeight * zone.pricePerKg);
  res.json({ zoneId: zone.id, actualWeight: weightKg, volumetricWeight, chargeableWeight, fee: Math.round(fee * 100) / 100 });
});

// ══════════════════════════════════════════════════════════
// PRICING — /api/pricing
// ══════════════════════════════════════════════════════════
app.post('/api/pricing/calculate', (req, res) => {
  const { costPrice, categoryId, isVatRegistered, isNonLegalized } = req.body;
  const upliftRate = costPrice < 100 ? 0 : 0.20;
  const fixedMarkup = costPrice < 100 ? 20 : 0;
  const subVendorUplift = req.body.subVendorUplift || 0;
  const cooperativeFee = isNonLegalized ? costPrice * 0.05 : 0;
  const marketplaceUplift = costPrice * upliftRate + fixedMarkup;
  const vendorRevenue = costPrice + subVendorUplift;
  const vendorVat = isVatRegistered ? vendorRevenue * 0.14 : 0;
  const aroobaRevenue = marketplaceUplift + cooperativeFee;
  const aroobaVat = aroobaRevenue * 0.14;
  const finalPrice = vendorRevenue + vendorVat + aroobaRevenue + aroobaVat;

  res.json({
    costPrice, subVendorUplift, cooperativeFee, marketplaceUplift, vatRate: 0.14,
    bucketA: vendorRevenue, bucketB: vendorVat, bucketC: aroobaRevenue, bucketD: aroobaVat,
    finalCustomerPrice: Math.round(finalPrice * 100) / 100,
  });
});

app.post('/api/pricing/check-deviation', (req, res) => {
  const { costPrice, proposedPrice } = req.body;
  const deviation = (proposedPrice - costPrice) / costPrice;
  res.json({
    costPrice, proposedPrice, deviationPercent: Math.round(deviation * 10000) / 100,
    flagged: Math.abs(deviation) > 0.20,
    message: Math.abs(deviation) > 0.20 ? 'Price deviation exceeds 20% threshold — manual review required.' : 'Price within acceptable range.',
  });
});

// ══════════════════════════════════════════════════════════
// APPROVALS — /api/approvals
// ══════════════════════════════════════════════════════════
app.get('/api/approvals', (req, res) => {
  let filtered = [...data.approvals];
  if (req.query.status) filtered = filtered.filter(a => a.status === req.query.status);
  if (req.query.actionType) filtered = filtered.filter(a => a.actionType === req.query.actionType);
  if (req.query.vendorId) filtered = filtered.filter(a => a.vendorId === req.query.vendorId);
  res.json(data.paginate(filtered, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 20));
});

app.post('/api/approvals', (req, res) => {
  const id = data.uuid();
  data.approvals.push({ id, ...req.body, status: 'pending', createdAt: new Date().toISOString(), reviewedAt: null, reviewNotes: null });
  res.status(201).json(id);
});

app.put('/api/approvals/:id/review', (req, res) => {
  const approval = data.approvals.find(a => a.id === req.params.id);
  if (!approval) return res.status(404).json({ error: 'Approval request not found' });
  approval.status = req.body.isApproved ? 'approved' : 'rejected';
  approval.reviewNotes = req.body.reviewNotes || null;
  approval.reviewedAt = new Date().toISOString();
  res.sendStatus(204);
});

// ══════════════════════════════════════════════════════════
// AUDIT LOGS — /api/auditlogs
// ══════════════════════════════════════════════════════════
app.get('/api/auditlogs', (req, res) => {
  let filtered = [...data.auditLogs];
  if (req.query.userId) filtered = filtered.filter(l => l.userId === req.query.userId);
  if (req.query.action) filtered = filtered.filter(l => l.action === req.query.action);
  if (req.query.entityType) filtered = filtered.filter(l => l.entityType === req.query.entityType);
  res.json(data.paginate(filtered, Number(req.query.pageNumber) || 1, Number(req.query.pageSize) || 50));
});

// ══════════════════════════════════════════════════════════
// ADMIN CONFIG — /api/adminconfig
// ══════════════════════════════════════════════════════════
app.get('/api/adminconfig', (req, res) => {
  let filtered = [...data.adminConfigs];
  if (req.query.category) filtered = filtered.filter(c => c.category === req.query.category);
  res.json(filtered);
});

app.post('/api/adminconfig', (req, res) => {
  const existing = data.adminConfigs.find(c => c.key === req.body.key);
  if (existing) {
    Object.assign(existing, req.body);
    res.json(existing.id);
  } else {
    const id = data.uuid();
    data.adminConfigs.push({ id, ...req.body, isActive: true, sortOrder: data.adminConfigs.length + 1 });
    res.status(201).json(id);
  }
});

// ── Swagger mock ──────────────────────────────────────────
app.get('/swagger/v1/swagger.json', (_req, res) => {
  res.json({
    openapi: '3.0.1',
    info: { title: 'Arooba Marketplace API', version: 'v1', description: "Egypt's Most Inclusive Local E-Commerce Marketplace API (Mock)" },
    paths: {},
  });
});

// ── 404 handler ───────────────────────────────────────────
app.use((req, res) => {
  res.status(404).json({ error: `Route not found: ${req.method} ${req.path}` });
});

// ── Start ─────────────────────────────────────────────────
app.listen(PORT, '0.0.0.0', () => {
  console.log('');
  console.log('  ╔═══════════════════════════════════════════════════════╗');
  console.log('  ║   AROOBA MARKETPLACE — Local Mock API Server         ║');
  console.log('  ╠═══════════════════════════════════════════════════════╣');
  console.log(`  ║   API:     http://localhost:${PORT}                    ║`);
  console.log(`  ║   Health:  http://localhost:${PORT}/health              ║`);
  console.log(`  ║   Swagger: http://localhost:${PORT}/swagger             ║`);
  console.log('  ╚═══════════════════════════════════════════════════════╝');
  console.log('');
  console.log('  Available endpoints:');
  console.log('    POST /api/auth/register, /send-otp, /verify-otp, /login, /social-login, /refresh');
  console.log('    GET|POST /api/products, GET|PUT /api/products/:id, PATCH /api/products/:id/status');
  console.log('    GET|POST /api/orders, GET /api/orders/:id, PATCH /api/orders/:id/status');
  console.log('    GET|POST /api/vendors, GET|PUT /api/vendors/:id, GET|POST /api/vendors/:id/sub-vendors');
  console.log('    GET|POST /api/customers, GET|PUT /api/customers/:id, PUT /api/customers/:id/status');
  console.log('    GET /api/finance/wallets/:vendorId, /ledger/:vendorId, /splits/:orderId');
  console.log('    GET /api/dashboard/stats, /gmv-trend');
  console.log('    GET /api/analytics/summary, /conversion-funnel, /products, /activity-log');
  console.log('    GET /api/categories, /shipping/zones, /shipping/rates');
  console.log('    POST /api/pricing/calculate, /pricing/check-deviation');
  console.log('    GET|POST /api/approvals, PUT /api/approvals/:id/review');
  console.log('    GET /api/auditlogs, GET|POST /api/adminconfig');
  console.log('');
});
