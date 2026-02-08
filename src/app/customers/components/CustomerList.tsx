/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer List Component
 * ============================================================
 *
 * Paginated, searchable customer list with status filters,
 * tier badges, and quick stats. Entry point to customer CRM.
 * ============================================================
 */

import React, { useState, useMemo } from 'react';
import { useAppStore } from '../../../store/app-store';
import { StatCard, Badge, formatMoney, formatDate } from '../../shared/components';
import { mockCustomers, mockCRMStats } from '../mock-customer-data';
import type { CustomerCRM, CustomerStatus, CustomerTier } from '../types';

type FilterStatus = 'all' | CustomerStatus;

interface CustomerListProps {
  onSelectCustomer: (customer: CustomerCRM) => void;
}

const ITEMS_PER_PAGE = 10;

export function CustomerList({ onSelectCustomer }: CustomerListProps) {
  const { language } = useAppStore();
  const [filter, setFilter] = useState<FilterStatus>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);

  const filteredCustomers = useMemo(() => {
    return mockCustomers.filter((c) => {
      if (filter !== 'all' && c.status !== filter) return false;
      if (searchQuery) {
        const q = searchQuery.toLowerCase();
        return (
          c.fullName.toLowerCase().includes(q) ||
          c.fullNameAr.includes(searchQuery) ||
          c.mobileNumber.includes(searchQuery) ||
          (c.email && c.email.toLowerCase().includes(q))
        );
      }
      return true;
    });
  }, [filter, searchQuery]);

  const totalPages = Math.ceil(filteredCustomers.length / ITEMS_PER_PAGE);
  const paginatedCustomers = filteredCustomers.slice(
    (currentPage - 1) * ITEMS_PER_PAGE,
    currentPage * ITEMS_PER_PAGE
  );

  const stats = mockCRMStats;

  const statusVariant = (status: CustomerStatus): 'success' | 'pending' | 'danger' | 'neutral' | 'warning' => {
    switch (status) {
      case 'active': return 'success';
      case 'inactive': return 'warning';
      case 'blocked': return 'danger';
      case 'churned': return 'neutral';
    }
  };

  const statusLabel = (status: CustomerStatus): string => {
    const labels: Record<CustomerStatus, { ar: string; en: string }> = {
      active: { ar: 'نشط', en: 'Active' },
      inactive: { ar: 'غير نشط', en: 'Inactive' },
      blocked: { ar: 'محظور', en: 'Blocked' },
      churned: { ar: 'مفقود', en: 'Churned' },
    };
    return labels[status][language];
  };

  const tierLabel = (tier: CustomerTier): string => {
    const labels: Record<CustomerTier, { ar: string; en: string }> = {
      bronze: { ar: 'برونزي', en: 'Bronze' },
      silver: { ar: 'فضي', en: 'Silver' },
      gold: { ar: 'ذهبي', en: 'Gold' },
      platinum: { ar: 'بلاتيني', en: 'Platinum' },
    };
    return labels[tier][language];
  };

  const tierColor = (tier: CustomerTier): string => {
    switch (tier) {
      case 'bronze': return 'bg-amber-100 text-amber-700';
      case 'silver': return 'bg-gray-100 text-gray-700';
      case 'gold': return 'bg-yellow-100 text-yellow-800';
      case 'platinum': return 'bg-purple-100 text-purple-700';
    }
  };

  const filterButtons: { key: FilterStatus; label: { ar: string; en: string }; count: number }[] = [
    { key: 'all', label: { ar: 'الكل', en: 'All' }, count: mockCustomers.length },
    { key: 'active', label: { ar: 'نشط', en: 'Active' }, count: mockCustomers.filter(c => c.status === 'active').length },
    { key: 'inactive', label: { ar: 'غير نشط', en: 'Inactive' }, count: mockCustomers.filter(c => c.status === 'inactive').length },
    { key: 'blocked', label: { ar: 'محظور', en: 'Blocked' }, count: mockCustomers.filter(c => c.status === 'blocked').length },
    { key: 'churned', label: { ar: 'مفقود', en: 'Churned' }, count: mockCustomers.filter(c => c.status === 'churned').length },
  ];

  return (
    <div className="space-y-6">
      {/* KPI Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label={language === 'ar' ? 'إجمالي العملاء' : 'Total Customers'}
          value={stats.totalCustomers.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}
          trend={8}
          trendLabel={language === 'ar' ? 'هذا الشهر' : 'this month'}
          accent="blue"
          icon={<span className="text-2xl">👥</span>}
        />
        <StatCard
          label={language === 'ar' ? 'عملاء نشطون' : 'Active Customers'}
          value={stats.activeCustomers.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}
          trend={5}
          trendLabel={language === 'ar' ? 'نمو' : 'growth'}
          accent="green"
          icon={<span className="text-2xl">✅</span>}
        />
        <StatCard
          label={language === 'ar' ? 'متوسط القيمة الدائمة' : 'Avg. Lifetime Value'}
          value={formatMoney(stats.avgLifetimeValue)}
          trend={12}
          trendLabel={language === 'ar' ? 'زيادة' : 'increase'}
          accent="orange"
          icon={<span className="text-2xl">💰</span>}
        />
        <StatCard
          label={language === 'ar' ? 'معدل الفقد' : 'Churn Rate'}
          value={`${stats.churnRate}%`}
          trend={-1.2}
          trendLabel={language === 'ar' ? 'تحسن' : 'improved'}
          accent="red"
          icon={<span className="text-2xl">📉</span>}
        />
      </div>

      {/* Secondary Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'جدد هذا الشهر' : 'New This Month'}</p>
          <p className="text-xl font-bold text-earth-900">{stats.newThisMonth}</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'نقاط الولاء الصادرة' : 'Loyalty Issued'}</p>
          <p className="text-xl font-bold text-arooba-600">{(stats.totalLoyaltyIssued / 1000).toFixed(0)}K</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'متوسط التفاعل' : 'Avg Engagement'}</p>
          <p className="text-xl font-bold text-nile-600">{stats.avgEngagementScore}%</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'عملاء VIP' : 'VIP Customers'}</p>
          <p className="text-xl font-bold text-purple-600">{stats.topTierCount}</p>
        </div>
      </div>

      {/* Search & Filter */}
      <div className="card p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <input
              type="text"
              placeholder={language === 'ar' ? 'بحث بالاسم، الموبايل، أو الإيميل...' : 'Search by name, mobile, or email...'}
              value={searchQuery}
              onChange={(e) => { setSearchQuery(e.target.value); setCurrentPage(1); }}
              className="w-full px-4 py-2.5 rounded-xl border border-earth-200 bg-earth-50 text-sm focus:outline-none focus:ring-2 focus:ring-arooba-400 focus:border-transparent transition-all"
            />
          </div>
          <div className="flex gap-2 flex-wrap">
            {filterButtons.map((btn) => (
              <button
                key={btn.key}
                onClick={() => { setFilter(btn.key); setCurrentPage(1); }}
                className={`px-3 py-2 rounded-lg text-xs font-medium transition-all ${
                  filter === btn.key
                    ? 'bg-arooba-500 text-white shadow-sm'
                    : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                }`}
              >
                {btn.label[language]} ({btn.count})
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Customer Table */}
      <div className="card overflow-hidden">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>{language === 'ar' ? 'العميل' : 'Customer'}</th>
                <th>{language === 'ar' ? 'الموبايل' : 'Mobile'}</th>
                <th>{language === 'ar' ? 'الحالة' : 'Status'}</th>
                <th>{language === 'ar' ? 'المستوى' : 'Tier'}</th>
                <th>{language === 'ar' ? 'الطلبات' : 'Orders'}</th>
                <th>{language === 'ar' ? 'إجمالي الإنفاق' : 'Total Spent'}</th>
                <th>{language === 'ar' ? 'نقاط الولاء' : 'Loyalty'}</th>
                <th>{language === 'ar' ? 'آخر دخول' : 'Last Login'}</th>
              </tr>
            </thead>
            <tbody>
              {paginatedCustomers.map((customer) => (
                <tr
                  key={customer.id}
                  onClick={() => onSelectCustomer(customer)}
                  className="cursor-pointer hover:bg-earth-50 transition-colors"
                >
                  <td>
                    <div>
                      <p className="font-medium text-earth-900">{customer.fullNameAr}</p>
                      <p className="text-xs text-earth-500">{customer.email || '-'}</p>
                    </div>
                  </td>
                  <td>
                    <span className="text-sm font-mono dir-ltr">{customer.mobileNumber}</span>
                  </td>
                  <td>
                    <Badge variant={statusVariant(customer.status)}>
                      {statusLabel(customer.status)}
                    </Badge>
                  </td>
                  <td>
                    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${tierColor(customer.tier)}`}>
                      {tierLabel(customer.tier)}
                    </span>
                  </td>
                  <td>
                    <span className="font-medium">{customer.totalOrders}</span>
                  </td>
                  <td>
                    <span className="font-medium text-earth-900">{formatMoney(customer.totalSpent)}</span>
                  </td>
                  <td>
                    <span className="font-medium text-arooba-600">{customer.loyaltyPoints.toLocaleString()}</span>
                  </td>
                  <td>
                    <span className="text-xs text-earth-500">
                      {customer.lastLoginAt ? formatDate(customer.lastLoginAt) : '-'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-earth-100">
            <p className="text-sm text-earth-500">
              {language === 'ar'
                ? `عرض ${(currentPage - 1) * ITEMS_PER_PAGE + 1}-${Math.min(currentPage * ITEMS_PER_PAGE, filteredCustomers.length)} من ${filteredCustomers.length}`
                : `Showing ${(currentPage - 1) * ITEMS_PER_PAGE + 1}-${Math.min(currentPage * ITEMS_PER_PAGE, filteredCustomers.length)} of ${filteredCustomers.length}`
              }
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="px-3 py-1.5 rounded-lg text-xs font-medium bg-earth-100 text-earth-600 hover:bg-earth-200 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {language === 'ar' ? 'السابق' : 'Previous'}
              </button>
              <button
                onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="px-3 py-1.5 rounded-lg text-xs font-medium bg-earth-100 text-earth-600 hover:bg-earth-200 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {language === 'ar' ? 'التالي' : 'Next'}
              </button>
            </div>
          </div>
        )}

        {paginatedCustomers.length === 0 && (
          <div className="p-12 text-center">
            <p className="text-earth-500">{language === 'ar' ? 'لا يوجد عملاء مطابقون' : 'No matching customers'}</p>
          </div>
        )}
      </div>
    </div>
  );
}
