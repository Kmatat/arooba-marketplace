/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer CRM Module (Main)
 * ============================================================
 *
 * Complete Customer Relationship Management module with:
 * - Customer list with search, filter, and pagination
 * - Detailed customer profile view
 * - Tab-based navigation: Orders, Ratings, Performance, Activity, Logs
 *
 * Replaces the placeholder component in the main App router.
 * ============================================================
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader } from '../../shared/components';
import { CustomerList } from './CustomerList';
import { CustomerProfile } from './CustomerProfile';
import { CustomerOrders } from './CustomerOrders';
import { CustomerRatings } from './CustomerRatings';
import { CustomerPerformanceTab } from './CustomerPerformance';
import { CustomerActivityTab } from './CustomerActivity';
import { CustomerLogs } from './CustomerLogs';
import type { CustomerCRM as CustomerCRMType } from '../types';

type DetailTab = 'orders' | 'ratings' | 'performance' | 'activity' | 'logs';

export function CustomerCRM() {
  const { language } = useAppStore();
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerCRMType | null>(null);
  const [activeTab, setActiveTab] = useState<DetailTab>('orders');

  const tabs: { id: DetailTab; labelAr: string; labelEn: string; icon: string }[] = [
    { id: 'orders', labelAr: 'الطلبات', labelEn: 'Orders', icon: '📦' },
    { id: 'ratings', labelAr: 'التقييمات', labelEn: 'Ratings', icon: '⭐' },
    { id: 'performance', labelAr: 'الأداء والولاء', labelEn: 'Performance', icon: '📊' },
    { id: 'activity', labelAr: 'النشاط والدخول', labelEn: 'Activity', icon: '📱' },
    { id: 'logs', labelAr: 'سجل التغييرات', labelEn: 'Audit Logs', icon: '📋' },
  ];

  // Detail view
  if (selectedCustomer) {
    return (
      <div className="space-y-6">
        {/* Header with Back */}
        <div className="flex items-center gap-4">
          <button
            onClick={() => { setSelectedCustomer(null); setActiveTab('orders'); }}
            className="px-3 py-2 rounded-lg bg-earth-100 text-earth-600 hover:bg-earth-200 text-sm font-medium transition-all"
          >
            {language === 'ar' ? '→ العودة للقائمة' : '<- Back to List'}
          </button>
          <SectionHeader
            title={language === 'ar' ? 'ملف العميل' : 'Customer Profile'}
            subtitle={selectedCustomer.fullNameAr}
          />
        </div>

        {/* Customer Profile Card */}
        <CustomerProfile customer={selectedCustomer} />

        {/* Tab Navigation */}
        <div className="border-b border-earth-200">
          <div className="flex gap-1 overflow-x-auto pb-px">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-all whitespace-nowrap ${
                  activeTab === tab.id
                    ? 'border-arooba-500 text-arooba-700 bg-arooba-50/50'
                    : 'border-transparent text-earth-500 hover:text-earth-700 hover:border-earth-300'
                }`}
              >
                <span>{tab.icon}</span>
                <span>{language === 'ar' ? tab.labelAr : tab.labelEn}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Tab Content */}
        <div>
          {activeTab === 'orders' && <CustomerOrders customer={selectedCustomer} />}
          {activeTab === 'ratings' && <CustomerRatings customer={selectedCustomer} />}
          {activeTab === 'performance' && <CustomerPerformanceTab customer={selectedCustomer} />}
          {activeTab === 'activity' && <CustomerActivityTab customer={selectedCustomer} />}
          {activeTab === 'logs' && <CustomerLogs customer={selectedCustomer} />}
        </div>
      </div>
    );
  }

  // List view
  return (
    <div className="space-y-6">
      <SectionHeader
        title={language === 'ar' ? 'إدارة العملاء - CRM' : 'Customer Management - CRM'}
        subtitle={
          language === 'ar'
            ? 'إدارة شاملة للعملاء - البيانات، الطلبات، التقييمات، الأداء، النشاط والسجلات'
            : 'Complete customer management - Data, Orders, Ratings, Performance, Activity & Logs'
        }
      />
      <CustomerList onSelectCustomer={(c) => { setSelectedCustomer(c); setActiveTab('orders'); }} />
    </div>
  );
}
