/**
 * AROOBA MARKETPLACE — Mock Data (continued)
 * Wallets, Dashboard Stats, and Analytics
 */

import type { VendorWallet, DashboardStats, TimeSeriesData } from '../app/shared/types';

export const mockWallets: VendorWallet[] = [
  { vendorId: 'v-001', totalBalance: 45200, pendingBalance: 12800, availableBalance: 32400, lifetimeEarnings: 187500 },
  { vendorId: 'v-002', totalBalance: 78500, pendingBalance: 28000, availableBalance: 50500, lifetimeEarnings: 324000 },
  { vendorId: 'v-003', totalBalance: 8700, pendingBalance: 3200, availableBalance: 5500, lifetimeEarnings: 42300 },
  { vendorId: 'v-004', totalBalance: 125000, pendingBalance: 45000, availableBalance: 80000, lifetimeEarnings: 892000 },
];

export const mockDashboardStats: DashboardStats = {
  totalGmv: 2450000,
  totalOrders: 1213,
  activeVendors: 347,
  registeredCustomers: 8420,
  avgOrderValue: 285,
  codRatio: 0.62,
  returnRate: 0.08,
  monthlyGrowth: 0.23,
};

export const mockGmvTimeSeries: TimeSeriesData[] = [
  { date: '2025-10', value: 320000, label: 'أكتوبر' },
  { date: '2025-11', value: 580000, label: 'نوفمبر' },
  { date: '2025-12', value: 890000, label: 'ديسمبر' },
  { date: '2026-01', value: 1250000, label: 'يناير' },
  { date: '2026-02', value: 1680000, label: 'فبراير' },
  { date: '2026-03', value: 2450000, label: 'مارس' },
];

export const mockOrdersTimeSeries: TimeSeriesData[] = [
  { date: '2025-10', value: 85, label: 'أكتوبر' },
  { date: '2025-11', value: 210, label: 'نوفمبر' },
  { date: '2025-12', value: 380, label: 'ديسمبر' },
  { date: '2026-01', value: 620, label: 'يناير' },
  { date: '2026-02', value: 950, label: 'فبراير' },
  { date: '2026-03', value: 1213, label: 'مارس' },
];

export const mockCategoryBreakdown = [
  { name: 'ديكور (هش)', nameEn: 'Home Decor (Fragile)', value: 28, color: '#ee7711' },
  { name: 'جلود', nameEn: 'Leather Goods', value: 22, color: '#b94508' },
  { name: 'منسوجات', nameEn: 'Textiles', value: 18, color: '#1fa76d' },
  { name: 'أزياء', nameEn: 'Fashion', value: 15, color: '#3b82f6' },
  { name: 'مجوهرات', nameEn: 'Jewelry', value: 10, color: '#d79352' },
  { name: 'جمال', nameEn: 'Beauty', value: 7, color: '#8b5cf6' },
];

export const mockZonePerformance = [
  { zone: 'القاهرة الكبرى', orders: 680, slaHit: 0.92, avgDays: 1.4 },
  { zone: 'الإسكندرية', orders: 210, slaHit: 0.88, avgDays: 1.8 },
  { zone: 'الدلتا', orders: 165, slaHit: 0.82, avgDays: 2.6 },
  { zone: 'صعيد مصر', orders: 98, slaHit: 0.75, avgDays: 4.1 },
  { zone: 'القنال', orders: 45, slaHit: 0.84, avgDays: 2.3 },
  { zone: 'سيناء', orders: 15, slaHit: 0.67, avgDays: 5.8 },
];
