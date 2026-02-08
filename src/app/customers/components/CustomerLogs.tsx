/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer Logs & Audit Trail Tab
 * ============================================================
 *
 * Complete audit log for a customer: account changes, tier upgrades,
 * wallet transactions, order events, and admin actions.
 * ============================================================
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { Badge, formatDate } from '../../shared/components';
import { mockCustomerLogs } from '../mock-customer-data';
import type { CustomerCRM, CustomerLogAction, LogSeverity } from '../types';

interface CustomerLogsProps {
  customer: CustomerCRM;
}

export function CustomerLogs({ customer }: CustomerLogsProps) {
  const { language } = useAppStore();
  const [severityFilter, setSeverityFilter] = useState<LogSeverity | 'all'>('all');
  const logs = mockCustomerLogs[customer.id] || [];

  const filteredLogs = severityFilter === 'all'
    ? logs
    : logs.filter(l => l.severity === severityFilter);

  const severityConfig: Record<LogSeverity, { ar: string; en: string; variant: 'info' | 'warning' | 'danger' | 'neutral'; color: string }> = {
    info: { ar: 'معلومة', en: 'Info', variant: 'info', color: 'bg-blue-100 text-blue-700' },
    warning: { ar: 'تحذير', en: 'Warning', variant: 'warning', color: 'bg-amber-100 text-amber-700' },
    error: { ar: 'خطأ', en: 'Error', variant: 'danger', color: 'bg-red-100 text-red-700' },
    critical: { ar: 'حرج', en: 'Critical', variant: 'danger', color: 'bg-red-200 text-red-800' },
  };

  const actionIcon = (action: CustomerLogAction): string => {
    const icons: Record<CustomerLogAction, string> = {
      account_created: '🆕',
      profile_updated: '✏️',
      address_added: '📍',
      address_removed: '🗑',
      password_changed: '🔑',
      status_changed: '🔄',
      tier_upgraded: '⬆️',
      tier_downgraded: '⬇️',
      wallet_credited: '💰',
      wallet_debited: '💸',
      loyalty_earned: '⭐',
      loyalty_redeemed: '🎁',
      referral_applied: '🔗',
      order_placed: '📦',
      order_cancelled: '❌',
      order_returned: '↩️',
      review_posted: '📝',
      review_removed: '🚫',
      subscription_started: '📋',
      subscription_cancelled: '📋',
      account_blocked: '🚫',
      account_unblocked: '✅',
    };
    return icons[action] || '📋';
  };

  const actionLabel = (action: CustomerLogAction): { ar: string; en: string } => {
    const labels: Record<CustomerLogAction, { ar: string; en: string }> = {
      account_created: { ar: 'إنشاء حساب', en: 'Account Created' },
      profile_updated: { ar: 'تحديث الملف', en: 'Profile Updated' },
      address_added: { ar: 'إضافة عنوان', en: 'Address Added' },
      address_removed: { ar: 'إزالة عنوان', en: 'Address Removed' },
      password_changed: { ar: 'تغيير كلمة المرور', en: 'Password Changed' },
      status_changed: { ar: 'تغيير الحالة', en: 'Status Changed' },
      tier_upgraded: { ar: 'ترقية المستوى', en: 'Tier Upgraded' },
      tier_downgraded: { ar: 'خفض المستوى', en: 'Tier Downgraded' },
      wallet_credited: { ar: 'إيداع بالمحفظة', en: 'Wallet Credit' },
      wallet_debited: { ar: 'خصم من المحفظة', en: 'Wallet Debit' },
      loyalty_earned: { ar: 'اكتساب نقاط', en: 'Loyalty Earned' },
      loyalty_redeemed: { ar: 'استبدال نقاط', en: 'Loyalty Redeemed' },
      referral_applied: { ar: 'إحالة مُطبّقة', en: 'Referral Applied' },
      order_placed: { ar: 'طلب جديد', en: 'Order Placed' },
      order_cancelled: { ar: 'إلغاء طلب', en: 'Order Cancelled' },
      order_returned: { ar: 'إرجاع طلب', en: 'Order Returned' },
      review_posted: { ar: 'نشر تقييم', en: 'Review Posted' },
      review_removed: { ar: 'حذف تقييم', en: 'Review Removed' },
      subscription_started: { ar: 'بدء اشتراك', en: 'Subscription Started' },
      subscription_cancelled: { ar: 'إلغاء اشتراك', en: 'Subscription Cancelled' },
      account_blocked: { ar: 'حظر الحساب', en: 'Account Blocked' },
      account_unblocked: { ar: 'إلغاء حظر الحساب', en: 'Account Unblocked' },
    };
    return labels[action];
  };

  const formatTime = (dateStr: string): string => {
    return new Date(dateStr).toLocaleTimeString(language === 'ar' ? 'ar-EG' : 'en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const severityFilters: { key: LogSeverity | 'all'; label: { ar: string; en: string } }[] = [
    { key: 'all', label: { ar: 'الكل', en: 'All' } },
    { key: 'info', label: { ar: 'معلومة', en: 'Info' } },
    { key: 'warning', label: { ar: 'تحذير', en: 'Warning' } },
    { key: 'error', label: { ar: 'خطأ', en: 'Error' } },
    { key: 'critical', label: { ar: 'حرج', en: 'Critical' } },
  ];

  return (
    <div className="space-y-4">
      {/* Severity Filter */}
      <div className="flex gap-2 flex-wrap">
        {severityFilters.map((f) => {
          const count = f.key === 'all' ? logs.length : logs.filter(l => l.severity === f.key).length;
          if (count === 0 && f.key !== 'all') return null;
          return (
            <button
              key={f.key}
              onClick={() => setSeverityFilter(f.key)}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                severityFilter === f.key
                  ? 'bg-arooba-500 text-white'
                  : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
              }`}
            >
              {f.label[language]} ({count})
            </button>
          );
        })}
      </div>

      {/* Log Entries */}
      {filteredLogs.length === 0 ? (
        <div className="card p-12 text-center">
          <p className="text-earth-500">{language === 'ar' ? 'لا توجد سجلات' : 'No log entries found'}</p>
        </div>
      ) : (
        <div className="card overflow-hidden">
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>{language === 'ar' ? 'الإجراء' : 'Action'}</th>
                  <th>{language === 'ar' ? 'الخطورة' : 'Severity'}</th>
                  <th>{language === 'ar' ? 'الوصف' : 'Description'}</th>
                  <th>{language === 'ar' ? 'بواسطة' : 'Performed By'}</th>
                  <th>{language === 'ar' ? 'التفاصيل' : 'Details'}</th>
                  <th>{language === 'ar' ? 'التاريخ' : 'Date'}</th>
                </tr>
              </thead>
              <tbody>
                {filteredLogs.map((log) => {
                  const sc = severityConfig[log.severity];
                  const al = actionLabel(log.action);
                  return (
                    <tr key={log.id}>
                      <td>
                        <div className="flex items-center gap-2">
                          <span>{actionIcon(log.action)}</span>
                          <span className="text-sm font-medium text-earth-700">{al[language]}</span>
                        </div>
                      </td>
                      <td>
                        <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${sc.color}`}>
                          {sc[language]}
                        </span>
                      </td>
                      <td>
                        <p className="text-sm text-earth-700 max-w-[300px]">
                          {language === 'ar' ? log.descriptionAr : log.description}
                        </p>
                      </td>
                      <td>
                        <div>
                          <p className="text-sm text-earth-700">{log.performedBy}</p>
                          <p className="text-xs text-earth-400">{log.performedByRole}</p>
                        </div>
                      </td>
                      <td>
                        <div className="space-y-1 max-w-[200px]">
                          {log.oldValues && (
                            <div className="text-xs">
                              <span className="text-red-500">{language === 'ar' ? 'قبل:' : 'Before:'}</span>{' '}
                              {Object.entries(log.oldValues).map(([k, v]) => (
                                <span key={k} className="text-earth-500">{k}: {v} </span>
                              ))}
                            </div>
                          )}
                          {log.newValues && (
                            <div className="text-xs">
                              <span className="text-nile-600">{language === 'ar' ? 'بعد:' : 'After:'}</span>{' '}
                              {Object.entries(log.newValues).map(([k, v]) => (
                                <span key={k} className="text-earth-500">{k}: {v} </span>
                              ))}
                            </div>
                          )}
                          {log.ipAddress && (
                            <p className="text-[10px] text-earth-400 font-mono">IP: {log.ipAddress}</p>
                          )}
                        </div>
                      </td>
                      <td>
                        <div>
                          <p className="text-sm text-earth-700">{formatDate(log.timestamp)}</p>
                          <p className="text-xs text-earth-400">{formatTime(log.timestamp)}</p>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
