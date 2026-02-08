/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer Login & Activity Tab
 * ============================================================
 *
 * Login history with device/IP tracking, session durations,
 * and full activity timeline with action type filtering.
 * ============================================================
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { Badge, formatDate } from '../../shared/components';
import { mockCustomerLogins, mockCustomerActivities } from '../mock-customer-data';
import type { CustomerCRM, LoginStatus, DeviceType, ActivityAction } from '../types';

interface CustomerActivityProps {
  customer: CustomerCRM;
}

type ActivityTab = 'logins' | 'activity';

export function CustomerActivityTab({ customer }: CustomerActivityProps) {
  const { language } = useAppStore();
  const [activeTab, setActiveTab] = useState<ActivityTab>('logins');
  const [activityFilter, setActivityFilter] = useState<ActivityAction | 'all'>('all');

  const logins = mockCustomerLogins[customer.id] || [];
  const activities = mockCustomerActivities[customer.id] || [];

  const filteredActivities = activityFilter === 'all'
    ? activities
    : activities.filter(a => a.action === activityFilter);

  const tabs: { id: ActivityTab; label: { ar: string; en: string } }[] = [
    { id: 'logins', label: { ar: 'سجل الدخول', en: 'Login History' } },
    { id: 'activity', label: { ar: 'النشاطات', en: 'Activity Log' } },
  ];

  const loginStatusConfig: Record<LoginStatus, { ar: string; en: string; variant: 'success' | 'danger' | 'warning' }> = {
    success: { ar: 'ناجح', en: 'Success', variant: 'success' },
    failed: { ar: 'فاشل', en: 'Failed', variant: 'danger' },
    blocked: { ar: 'محظور', en: 'Blocked', variant: 'warning' },
  };

  const deviceIcon = (type: DeviceType): string => {
    switch (type) {
      case 'mobile': return '📱';
      case 'desktop': return '💻';
      case 'tablet': return '📟';
    }
  };

  const activityIcon = (action: ActivityAction): string => {
    const icons: Record<ActivityAction, string> = {
      page_view: '👁',
      product_view: '🔍',
      search: '🔎',
      add_to_cart: '🛒',
      remove_from_cart: '❌',
      checkout_start: '💳',
      purchase: '✅',
      review_submit: '⭐',
      wishlist_add: '❤️',
      referral_share: '🔗',
      wallet_topup: '💰',
      profile_update: '👤',
      address_add: '📍',
      subscription_change: '📦',
    };
    return icons[action];
  };

  const activityLabel = (action: ActivityAction): { ar: string; en: string } => {
    const labels: Record<ActivityAction, { ar: string; en: string }> = {
      page_view: { ar: 'مشاهدة صفحة', en: 'Page View' },
      product_view: { ar: 'مشاهدة منتج', en: 'Product View' },
      search: { ar: 'بحث', en: 'Search' },
      add_to_cart: { ar: 'إضافة للسلة', en: 'Add to Cart' },
      remove_from_cart: { ar: 'إزالة من السلة', en: 'Remove from Cart' },
      checkout_start: { ar: 'بدء الدفع', en: 'Checkout Start' },
      purchase: { ar: 'شراء', en: 'Purchase' },
      review_submit: { ar: 'كتابة تقييم', en: 'Review Submit' },
      wishlist_add: { ar: 'إضافة للمفضلة', en: 'Wishlist Add' },
      referral_share: { ar: 'مشاركة إحالة', en: 'Referral Share' },
      wallet_topup: { ar: 'شحن المحفظة', en: 'Wallet Top-up' },
      profile_update: { ar: 'تحديث الملف', en: 'Profile Update' },
      address_add: { ar: 'إضافة عنوان', en: 'Address Add' },
      subscription_change: { ar: 'تغيير اشتراك', en: 'Subscription Change' },
    };
    return labels[action];
  };

  // Login summary
  const successLogins = logins.filter(l => l.status === 'success').length;
  const failedLogins = logins.filter(l => l.status === 'failed').length;
  const blockedLogins = logins.filter(l => l.status === 'blocked').length;
  const avgSessionDuration = logins
    .filter(l => l.sessionDuration)
    .reduce((sum, l, _, arr) => sum + (l.sessionDuration || 0) / arr.length, 0);

  const formatTime = (dateStr: string): string => {
    return new Date(dateStr).toLocaleTimeString(language === 'ar' ? 'ar-EG' : 'en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatDateTime = (dateStr: string): string => {
    return `${formatDate(dateStr)} ${formatTime(dateStr)}`;
  };

  // Unique activity types for filter
  const activityTypes = Array.from(new Set(activities.map(a => a.action)));

  return (
    <div className="space-y-4">
      {/* Sub-tabs */}
      <div className="flex gap-2 border-b border-earth-200 pb-2">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`px-4 py-2 rounded-t-lg text-sm font-medium transition-all ${
              activeTab === tab.id
                ? 'bg-arooba-500 text-white'
                : 'text-earth-500 hover:text-earth-700 hover:bg-earth-100'
            }`}
          >
            {tab.label[language]}
          </button>
        ))}
      </div>

      {/* LOGIN HISTORY */}
      {activeTab === 'logins' && (
        <div className="space-y-4">
          {/* Login Stats */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <div className="card p-3 text-center">
              <p className="text-xs text-earth-500">{language === 'ar' ? 'إجمالي الجلسات' : 'Total Sessions'}</p>
              <p className="text-xl font-bold text-earth-900">{customer.totalSessions}</p>
            </div>
            <div className="card p-3 text-center">
              <p className="text-xs text-earth-500">{language === 'ar' ? 'دخول ناجح' : 'Successful'}</p>
              <p className="text-xl font-bold text-nile-600">{successLogins}</p>
            </div>
            <div className="card p-3 text-center">
              <p className="text-xs text-earth-500">{language === 'ar' ? 'محاولات فاشلة' : 'Failed'}</p>
              <p className="text-xl font-bold text-red-600">{failedLogins + blockedLogins}</p>
            </div>
            <div className="card p-3 text-center">
              <p className="text-xs text-earth-500">{language === 'ar' ? 'متوسط الجلسة' : 'Avg Session'}</p>
              <p className="text-xl font-bold text-blue-600">{avgSessionDuration.toFixed(0)} {language === 'ar' ? 'د' : 'min'}</p>
            </div>
          </div>

          {/* Login Entries */}
          <div className="card overflow-hidden">
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>{language === 'ar' ? 'التاريخ والوقت' : 'Date & Time'}</th>
                    <th>{language === 'ar' ? 'الحالة' : 'Status'}</th>
                    <th>{language === 'ar' ? 'الجهاز' : 'Device'}</th>
                    <th>{language === 'ar' ? 'عنوان IP' : 'IP Address'}</th>
                    <th>{language === 'ar' ? 'الموقع' : 'Location'}</th>
                    <th>{language === 'ar' ? 'مدة الجلسة' : 'Duration'}</th>
                  </tr>
                </thead>
                <tbody>
                  {logins.map((login) => {
                    const sc = loginStatusConfig[login.status];
                    return (
                      <tr key={login.id}>
                        <td>
                          <span className="text-sm">{formatDateTime(login.timestamp)}</span>
                        </td>
                        <td>
                          <Badge variant={sc.variant}>{sc[language]}</Badge>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <span>{deviceIcon(login.deviceType)}</span>
                            <div>
                              <p className="text-xs text-earth-700 max-w-[200px] truncate">{login.deviceInfo}</p>
                            </div>
                          </div>
                        </td>
                        <td>
                          <span className="font-mono text-xs">{login.ipAddress}</span>
                        </td>
                        <td>
                          <span className="text-sm text-earth-600">{login.location || '-'}</span>
                        </td>
                        <td>
                          {login.sessionDuration ? (
                            <span className="text-sm">{login.sessionDuration} {language === 'ar' ? 'دقيقة' : 'min'}</span>
                          ) : (
                            <span className="text-xs text-earth-400">-</span>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* ACTIVITY LOG */}
      {activeTab === 'activity' && (
        <div className="space-y-4">
          {/* Activity Type Filter */}
          <div className="flex gap-2 flex-wrap">
            <button
              onClick={() => setActivityFilter('all')}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                activityFilter === 'all'
                  ? 'bg-arooba-500 text-white'
                  : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
              }`}
            >
              {language === 'ar' ? 'الكل' : 'All'} ({activities.length})
            </button>
            {activityTypes.map((type) => {
              const label = activityLabel(type);
              const count = activities.filter(a => a.action === type).length;
              return (
                <button
                  key={type}
                  onClick={() => setActivityFilter(type)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                    activityFilter === type
                      ? 'bg-arooba-500 text-white'
                      : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                  }`}
                >
                  {activityIcon(type)} {label[language]} ({count})
                </button>
              );
            })}
          </div>

          {/* Activity Timeline */}
          <div className="card p-4">
            <div className="space-y-0">
              {filteredActivities.map((activity, idx) => (
                <div key={activity.id} className="flex gap-4">
                  {/* Timeline Line */}
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full bg-earth-100 flex items-center justify-center text-sm shrink-0">
                      {activityIcon(activity.action)}
                    </div>
                    {idx < filteredActivities.length - 1 && (
                      <div className="w-px h-full bg-earth-200 my-1" />
                    )}
                  </div>

                  {/* Content */}
                  <div className="pb-6 flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-sm font-medium text-earth-900">
                          {language === 'ar' ? activity.descriptionAr : activity.description}
                        </p>
                        {activity.productTitle && (
                          <p className="text-xs text-arooba-600 mt-0.5">{activity.productTitle}</p>
                        )}
                        {activity.orderNumber && (
                          <p className="text-xs text-blue-600 mt-0.5">
                            {language === 'ar' ? 'طلب' : 'Order'}: {activity.orderNumber}
                          </p>
                        )}
                        {activity.metadata && (
                          <div className="flex gap-2 mt-1 flex-wrap">
                            {Object.entries(activity.metadata).map(([key, val]) => (
                              <span key={key} className="text-[10px] px-1.5 py-0.5 rounded bg-earth-100 text-earth-600">
                                {key}: {val}
                              </span>
                            ))}
                          </div>
                        )}
                      </div>
                      <div className="text-left shrink-0">
                        <p className="text-xs text-earth-400">{formatDateTime(activity.timestamp)}</p>
                        <p className="text-[10px] text-earth-300 mt-0.5">
                          {deviceIcon(activity.deviceType)} {activity.ipAddress}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              ))}

              {filteredActivities.length === 0 && (
                <div className="py-8 text-center">
                  <p className="text-earth-500">{language === 'ar' ? 'لا توجد نشاطات' : 'No activities found'}</p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
