/**
 * ============================================================
 * AROOBA MARKETPLACE — User Analytics Dashboard
 * ============================================================
 *
 * Management dashboard for monitoring all user actions:
 * - KPI summary cards (sessions, users, views, conversions)
 * - Daily activity trend chart
 * - Conversion funnel visualization
 * - Product performance analytics
 * - Related products analysis
 * - User activity log
 * - Device & search analytics
 * ============================================================
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader, StatCard, formatMoney } from '../../shared/components';
import { mockAnalyticsSummary } from '../mock-analytics-data';
import { ConversionFunnel } from './ConversionFunnel';
import { ProductAnalytics } from './ProductAnalytics';
import { UserActivityLog } from './UserActivityLog';
import { ActivityTrendChart } from './ActivityTrendChart';
import { DeviceAndSearchAnalytics } from './DeviceAndSearchAnalytics';

type TabId = 'overview' | 'products' | 'activity';

export function UserAnalyticsDashboard() {
  const { language } = useAppStore();
  const [activeTab, setActiveTab] = useState<TabId>('overview');
  const summary = mockAnalyticsSummary;

  const tabs: { id: TabId; labelAr: string; labelEn: string }[] = [
    { id: 'overview', labelAr: 'نظرة عامة', labelEn: 'Overview' },
    { id: 'products', labelAr: 'تحليل المنتجات', labelEn: 'Product Analytics' },
    { id: 'activity', labelAr: 'سجل النشاط', labelEn: 'Activity Log' },
  ];

  return (
    <div className="space-y-6">
      <SectionHeader
        title={language === 'ar' ? 'تحليلات المستخدمين' : 'User Analytics'}
        subtitle={language === 'ar'
          ? 'مراقبة وتحليل سلوك المستخدمين - المشاهدات، السلة، التحويل، والمنتجات'
          : 'Monitor and analyze user behavior - views, cart, conversion, and products'
        }
      />

      {/* KPI Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label={language === 'ar' ? 'الجلسات' : 'Sessions'}
          value={summary.totalSessions.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}
          trend={12}
          trendLabel={language === 'ar' ? 'هذا الشهر' : 'this month'}
          accent="blue"
          icon={<span className="text-2xl">👁</span>}
        />
        <StatCard
          label={language === 'ar' ? 'المستخدمون الفريدون' : 'Unique Users'}
          value={summary.uniqueUsers.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}
          trend={8}
          trendLabel={language === 'ar' ? 'نمو' : 'growth'}
          accent="green"
          icon={<span className="text-2xl">👥</span>}
        />
        <StatCard
          label={language === 'ar' ? 'معدل التحويل' : 'Conversion Rate'}
          value={`${summary.overallConversionRate}%`}
          trend={0.5}
          trendLabel={language === 'ar' ? 'تحسن' : 'improvement'}
          accent="orange"
          icon={<span className="text-2xl">🎯</span>}
        />
        <StatCard
          label={language === 'ar' ? 'متوسط قيمة السلة' : 'Avg Cart Value'}
          value={formatMoney(summary.averageCartValue)}
          trend={5}
          trendLabel={language === 'ar' ? 'زيادة' : 'increase'}
          accent="orange"
          icon={<span className="text-2xl">🛒</span>}
        />
      </div>

      {/* Secondary KPI Row */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'مشاهدات المنتجات' : 'Product Views'}</p>
          <p className="text-xl font-bold text-earth-900 dir-ltr">{summary.totalProductViews.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'إضافة للسلة' : 'Cart Adds'}</p>
          <p className="text-xl font-bold text-arooba-600 dir-ltr">{summary.totalCartAdds.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'عمليات الشراء' : 'Purchases'}</p>
          <p className="text-xl font-bold text-nile-600 dir-ltr">{summary.totalPurchases.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'عمليات البحث' : 'Searches'}</p>
          <p className="text-xl font-bold text-blue-600 dir-ltr">{summary.totalSearches.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}</p>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="flex gap-1 bg-earth-100 rounded-xl p-1">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`
              flex-1 py-2.5 px-4 rounded-lg text-sm font-medium transition-all duration-200
              ${activeTab === tab.id
                ? 'bg-white text-arooba-700 shadow-sm'
                : 'text-earth-500 hover:text-earth-700'
              }
            `}
          >
            {language === 'ar' ? tab.labelAr : tab.labelEn}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      {activeTab === 'overview' && (
        <div className="space-y-6">
          <ActivityTrendChart />
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <ConversionFunnel />
            <DeviceAndSearchAnalytics />
          </div>
        </div>
      )}

      {activeTab === 'products' && <ProductAnalytics />}
      {activeTab === 'activity' && <UserActivityLog />}
    </div>
  );
}
