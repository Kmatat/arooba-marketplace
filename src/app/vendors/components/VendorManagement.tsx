/**
 * ============================================================
 * AROOBA MARKETPLACE — Vendor Management Module
 * ============================================================
 * 
 * Manages the Parent Vendor / Sub-Vendor hierarchy.
 * 
 * BUSINESS CONTEXT:
 * This is where the B2B2C model comes alive. A "Parent Vendor"
 * like "Hassan Ceramics" can have sub-vendors like "Aunt Fatma"
 * who supplies from Fayoum. The Parent is legally responsible
 * for all sub-vendor products.
 * 
 * KEY CHALLENGES:
 * 1. Non-legalized vendors need a Cooperative intermediary
 * 2. VAT is inherited from the Parent's status
 * 3. Reliability strikes enforce the "Accept or Die" SOP
 * ============================================================
 */

import React, { useState } from 'react';
import { StatCard, Badge, DataTable, SectionHeader, formatMoney, formatDate } from '../../shared/components';
import type { ParentVendor } from '../../shared/types';

// Mock data import (in production, this comes from API)
const mockVendors: ParentVendor[] = [
  {
    id: 'v-001', userId: 'u-101', businessName: 'Hassan Ceramics', businessNameAr: 'خزفيات حسن',
    type: 'legalized', status: 'active', commercialRegNumber: 'CR-2024-001234', taxId: 'TX-9900112233',
    isVatRegistered: true, defaultCommissionRate: 0.25, bankName: 'CIB', bankAccountNumber: '****7890',
    reliabilityStrikes: 0, averageRating: 4.7, totalOrders: 342, totalRevenue: 187500,
    subVendorIds: ['sv-001'], createdAt: '2025-10-15T10:00:00Z', updatedAt: '2025-12-01T14:30:00Z',
  },
  {
    id: 'v-002', userId: 'u-102', businessName: 'Siwa Textiles Co.', businessNameAr: 'سيوة للمنسوجات',
    type: 'legalized', status: 'active', commercialRegNumber: 'CR-2024-005678', taxId: 'TX-8800223344',
    isVatRegistered: true, defaultCommissionRate: 0.20, bankName: 'Banque Misr', bankAccountNumber: '****4321',
    reliabilityStrikes: 0, averageRating: 4.5, totalOrders: 215, totalRevenue: 324000,
    subVendorIds: [], createdAt: '2025-10-20T08:00:00Z', updatedAt: '2025-11-28T16:00:00Z',
  },
  {
    id: 'v-003', userId: 'u-103', businessName: 'Nadia Handcraft', businessNameAr: 'يدوية نادية',
    type: 'non_legalized', status: 'active', isVatRegistered: false, cooperativeId: 'coop-01',
    defaultCommissionRate: 0.20, reliabilityStrikes: 1, averageRating: 4.2, totalOrders: 89,
    totalRevenue: 42300, subVendorIds: ['sv-002'], createdAt: '2025-11-01T12:00:00Z', updatedAt: '2025-12-02T10:00:00Z',
  },
  {
    id: 'v-004', userId: 'u-104', businessName: 'Khan El-Khalili Leather', businessNameAr: 'جلود خان الخليلي',
    type: 'legalized', status: 'active', commercialRegNumber: 'CR-2024-009012', taxId: 'TX-7700334455',
    isVatRegistered: true, defaultCommissionRate: 0.20, bankName: 'NBE', bankAccountNumber: '****6543',
    reliabilityStrikes: 0, averageRating: 4.8, totalOrders: 567, totalRevenue: 892000,
    subVendorIds: [], createdAt: '2025-10-10T09:00:00Z', updatedAt: '2025-12-03T11:00:00Z',
  },
  {
    id: 'v-005', userId: 'u-105', businessName: 'Aswan Spices', businessNameAr: 'بهارات أسوان',
    type: 'non_legalized', status: 'pending', isVatRegistered: false, cooperativeId: 'coop-01',
    defaultCommissionRate: 0.15, reliabilityStrikes: 0, averageRating: 0, totalOrders: 0,
    totalRevenue: 0, subVendorIds: [], createdAt: '2025-12-01T15:00:00Z', updatedAt: '2025-12-01T15:00:00Z',
  },
];

type FilterStatus = 'all' | 'active' | 'pending' | 'suspended';

export function VendorManagement() {
  const [filter, setFilter] = useState<FilterStatus>('all');
  const [searchQuery, setSearchQuery] = useState('');

  const filteredVendors = mockVendors.filter((v) => {
    if (filter !== 'all' && v.status !== filter) return false;
    if (searchQuery && !v.businessNameAr.includes(searchQuery) && !v.businessName.toLowerCase().includes(searchQuery.toLowerCase())) return false;
    return true;
  });

  const activeCount = mockVendors.filter(v => v.status === 'active').length;
  const pendingCount = mockVendors.filter(v => v.status === 'pending').length;
  const totalRevenue = mockVendors.reduce((sum, v) => sum + v.totalRevenue, 0);

  const statusVariant = (status: string): 'success' | 'pending' | 'danger' | 'neutral' => {
    switch (status) {
      case 'active': return 'success';
      case 'pending': return 'pending';
      case 'suspended': return 'danger';
      default: return 'neutral';
    }
  };

  const statusLabel = (status: string): string => {
    switch (status) {
      case 'active': return 'نشط';
      case 'pending': return 'قيد المراجعة';
      case 'suspended': return 'معلّق';
      case 'rejected': return 'مرفوض';
      default: return status;
    }
  };

  const columns = [
    {
      key: 'vendor',
      header: 'المورد',
      render: (v: ParentVendor) => (
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-arooba-100 text-arooba-600 flex items-center justify-center text-sm font-bold shrink-0">
            {v.businessNameAr.charAt(0)}
          </div>
          <div>
            <p className="font-medium text-earth-800 text-sm">{v.businessNameAr}</p>
            <p className="text-xs text-earth-400 dir-ltr">{v.businessName}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'type',
      header: 'النوع',
      render: (v: ParentVendor) => (
        <Badge variant={v.type === 'legalized' ? 'info' : 'warning'} dot={false}>
          {v.type === 'legalized' ? 'مسجل قانوني' : 'غير مسجل (تعاونية)'}
        </Badge>
      ),
    },
    {
      key: 'status',
      header: 'الحالة',
      render: (v: ParentVendor) => (
        <Badge variant={statusVariant(v.status)}>
          {statusLabel(v.status)}
        </Badge>
      ),
    },
    {
      key: 'vat',
      header: 'ض.ق.م',
      render: (v: ParentVendor) => (
        <span className={`text-xs font-medium ${v.isVatRegistered ? 'text-nile-600' : 'text-earth-400'}`}>
          {v.isVatRegistered ? '✓ مسجل' : '✗ غير مسجل'}
        </span>
      ),
    },
    {
      key: 'orders',
      header: 'الطلبات',
      render: (v: ParentVendor) => (
        <span className="font-medium text-earth-700">{v.totalOrders}</span>
      ),
    },
    {
      key: 'revenue',
      header: 'الإيرادات',
      render: (v: ParentVendor) => (
        <span className="font-medium text-earth-800">{formatMoney(v.totalRevenue, true)}</span>
      ),
    },
    {
      key: 'rating',
      header: 'التقييم',
      render: (v: ParentVendor) => (
        <div className="flex items-center gap-1">
          <span className="text-amber-500">★</span>
          <span className="font-medium text-earth-700">{v.averageRating || '—'}</span>
        </div>
      ),
    },
    {
      key: 'strikes',
      header: 'الإنذارات',
      render: (v: ParentVendor) => (
        v.reliabilityStrikes > 0 ? (
          <Badge variant="danger">{v.reliabilityStrikes} إنذار</Badge>
        ) : (
          <span className="text-xs text-earth-400">—</span>
        )
      ),
    },
    {
      key: 'subs',
      header: 'فرعيون',
      render: (v: ParentVendor) => (
        <span className="text-sm text-earth-600">{v.subVendorIds.length || '—'}</span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* KPI Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="إجمالي الموردين" value={mockVendors.length} accent="orange" icon={<span className="text-2xl">🏪</span>} />
        <StatCard label="نشطون" value={activeCount} trend={12} accent="green" icon={<span className="text-2xl">✅</span>} />
        <StatCard label="قيد المراجعة" value={pendingCount} accent="blue" icon={<span className="text-2xl">⏳</span>} />
        <StatCard label="إجمالي الإيرادات" value={formatMoney(totalRevenue, true)} trend={23} accent="orange" icon={<span className="text-2xl">💰</span>} />
      </div>

      {/* Filters & Search */}
      <div className="card p-4">
        <div className="flex flex-wrap items-center gap-3">
          <input
            type="text"
            placeholder="بحث عن مورد..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="input max-w-xs"
          />
          <div className="flex gap-1">
            {(['all', 'active', 'pending', 'suspended'] as FilterStatus[]).map((status) => (
              <button
                key={status}
                onClick={() => setFilter(status)}
                className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                  filter === status
                    ? 'bg-arooba-500 text-white'
                    : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                }`}
              >
                {status === 'all' ? 'الكل' : statusLabel(status)}
              </button>
            ))}
          </div>
          <button className="btn-primary mr-auto">
            + إضافة مورد
          </button>
        </div>
      </div>

      {/* Vendors Table */}
      <DataTable
        columns={columns}
        data={filteredVendors}
        keyExtractor={(v) => v.id}
        onRowClick={(v) => console.log('Open vendor:', v.id)}
        emptyMessage="لا يوجد موردون"
      />
    </div>
  );
}
