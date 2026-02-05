/**
 * ============================================================
 * AROOBA MARKETPLACE — Admin Dashboard Module
 * ============================================================
 * 
 * The command center for Arooba operations.
 * Shows all critical KPIs from the E-Commerce Monitoring Checklist:
 * - GMV growth
 * - Order volume
 * - Vendor activity
 * - COD vs Digital ratio
 * - Delivery SLA performance
 * ============================================================
 */

import React from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, BarChart, Bar,
} from 'recharts';
import { StatCard, Badge, ProgressBar, SectionHeader, formatMoney, formatPercent } from '../../shared/components';
import {
  mockDashboardStats,
  mockGmvTimeSeries,
  mockOrdersTimeSeries,
  mockCategoryBreakdown,
  mockZonePerformance,
} from '../../../lib/mock-data-extended';
import { KPI_TARGETS } from '../../../config/constants';

export function AdminDashboard() {
  const stats = mockDashboardStats;

  return (
    <div className="space-y-6">
      {/* KPI Row */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="إجمالي GMV"
          value={formatMoney(stats.totalGmv, true)}
          trend={stats.monthlyGrowth * 100}
          trendLabel="نمو شهري"
          accent="orange"
          icon={<span className="text-2xl">💰</span>}
        />
        <StatCard
          label="إجمالي الطلبات"
          value={stats.totalOrders.toLocaleString('ar-EG')}
          trend={18}
          trendLabel="هذا الشهر"
          accent="blue"
          icon={<span className="text-2xl">🛒</span>}
        />
        <StatCard
          label="الموردون النشطون"
          value={stats.activeVendors}
          trend={12}
          trendLabel="مورد جديد"
          accent="green"
          icon={<span className="text-2xl">🏪</span>}
        />
        <StatCard
          label="العملاء المسجلون"
          value={stats.registeredCustomers.toLocaleString('ar-EG')}
          trend={23}
          trendLabel="نمو شهري"
          accent="orange"
          icon={<span className="text-2xl">👥</span>}
        />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* GMV Growth Chart */}
        <div className="card p-5 lg:col-span-2">
          <SectionHeader title="نمو GMV" subtitle="الاتجاه الشهري" />
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={mockGmvTimeSeries}>
                <defs>
                  <linearGradient id="gmvGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#ee7711" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="#ee7711" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2d2b8" opacity={0.5} />
                <XAxis dataKey="label" tick={{ fontSize: 12, fill: '#714938' }} />
                <YAxis
                  tick={{ fontSize: 12, fill: '#714938' }}
                  tickFormatter={(v) => `${(v / 1000000).toFixed(1)}M`}
                />
                <Tooltip
                  formatter={(value: number) => [formatMoney(value), 'GMV']}
                  contentStyle={{ borderRadius: '12px', border: '1px solid #e2d2b8', direction: 'rtl' }}
                />
                <Area
                  type="monotone"
                  dataKey="value"
                  stroke="#ee7711"
                  strokeWidth={2.5}
                  fill="url(#gmvGradient)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Category Breakdown */}
        <div className="card p-5">
          <SectionHeader title="توزيع الفئات" subtitle="حسب المبيعات" />
          <div className="h-48">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={mockCategoryBreakdown}
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={3}
                  dataKey="value"
                >
                  {mockCategoryBreakdown.map((entry) => (
                    <Cell key={entry.name} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number) => [`${value}%`, '']}
                  contentStyle={{ borderRadius: '12px', border: '1px solid #e2d2b8', direction: 'rtl' }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
          <div className="space-y-2 mt-2">
            {mockCategoryBreakdown.slice(0, 4).map((cat) => (
              <div key={cat.name} className="flex items-center gap-2 text-xs">
                <span className="w-2.5 h-2.5 rounded-full shrink-0" style={{ backgroundColor: cat.color }} />
                <span className="text-earth-600 flex-1">{cat.name}</span>
                <span className="font-medium text-earth-800 dir-ltr">{cat.value}%</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Operations Health Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Quick Health Indicators */}
        <div className="card p-5">
          <SectionHeader title="صحة المنصة" subtitle="مؤشرات الأداء الرئيسية" />
          <div className="space-y-4">
            <ProgressBar
              label="نسبة COD"
              value={Math.round(stats.codRatio * 100)}
              color={stats.codRatio <= KPI_TARGETS.codRatioMax ? 'green' : 'red'}
            />
            <ProgressBar
              label="معدل الإرجاع"
              value={Math.round(stats.returnRate * 100)}
              color={stats.returnRate <= KPI_TARGETS.refundRateMax ? 'green' : 'red'}
            />
            <ProgressBar
              label="متوسط قيمة الطلب"
              value={Math.min(100, Math.round((stats.avgOrderValue / 500) * 100))}
              color="blue"
            />
            <ProgressBar
              label="الموردون النشطون"
              value={Math.round((stats.activeVendors / 500) * 100)}
              color="orange"
            />
          </div>

          <div className="mt-5 pt-4 border-t border-earth-100 grid grid-cols-2 gap-3">
            <div className="text-center p-3 rounded-xl bg-earth-50">
              <p className="text-xs text-earth-500">COD إلى رقمي</p>
              <p className="text-lg font-bold text-earth-800 dir-ltr">{Math.round(stats.codRatio * 100)}:{Math.round((1 - stats.codRatio) * 100)}</p>
              <Badge variant={stats.codRatio <= KPI_TARGETS.codRatioMax ? 'success' : 'warning'}>
                {stats.codRatio <= KPI_TARGETS.codRatioMax ? 'ضمن الهدف' : 'يحتاج تحسين'}
              </Badge>
            </div>
            <div className="text-center p-3 rounded-xl bg-earth-50">
              <p className="text-xs text-earth-500">نمو شهري</p>
              <p className="text-lg font-bold text-nile-600">↑ {formatPercent(stats.monthlyGrowth)}</p>
              <Badge variant="success">ممتاز</Badge>
            </div>
          </div>
        </div>

        {/* Zone Performance */}
        <div className="card p-5">
          <SectionHeader title="أداء التوصيل" subtitle="حسب المنطقة" />
          <div className="h-56">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={mockZonePerformance} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" stroke="#e2d2b8" opacity={0.5} />
                <XAxis type="number" tick={{ fontSize: 11, fill: '#714938' }} />
                <YAxis
                  type="category"
                  dataKey="zone"
                  tick={{ fontSize: 11, fill: '#714938' }}
                  width={100}
                />
                <Tooltip
                  contentStyle={{ borderRadius: '12px', border: '1px solid #e2d2b8', direction: 'rtl' }}
                />
                <Bar dataKey="orders" fill="#ee7711" radius={[0, 4, 4, 0]} name="طلبات" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div className="mt-4 space-y-2">
            {mockZonePerformance.slice(0, 3).map((zone) => (
              <div key={zone.zone} className="flex items-center justify-between text-xs">
                <span className="text-earth-600">{zone.zone}</span>
                <div className="flex items-center gap-3">
                  <span className="text-earth-500">SLA: {formatPercent(zone.slaHit)}</span>
                  <Badge variant={zone.slaHit >= 0.85 ? 'success' : zone.slaHit >= 0.75 ? 'warning' : 'danger'}>
                    {zone.avgDays} يوم
                  </Badge>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Orders Growth Mini Chart */}
      <div className="card p-5">
        <SectionHeader title="نمو الطلبات" subtitle="الاتجاه الشهري — الهدف السنوي: ١٤٠,٠٠٠ - ١٨٠,٠٠٠ طلب" />
        <div className="h-48">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={mockOrdersTimeSeries}>
              <defs>
                <linearGradient id="ordersGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#1fa76d" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#1fa76d" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#e2d2b8" opacity={0.5} />
              <XAxis dataKey="label" tick={{ fontSize: 12, fill: '#714938' }} />
              <YAxis tick={{ fontSize: 12, fill: '#714938' }} />
              <Tooltip
                formatter={(value: number) => [value.toLocaleString('ar-EG'), 'طلب']}
                contentStyle={{ borderRadius: '12px', border: '1px solid #e2d2b8', direction: 'rtl' }}
              />
              <Area
                type="monotone"
                dataKey="value"
                stroke="#1fa76d"
                strokeWidth={2.5}
                fill="url(#ordersGradient)"
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}
