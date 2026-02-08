/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer Profile Component
 * ============================================================
 *
 * Customer detail header with profile info, tier badge,
 * wallet balance, addresses, and key metrics summary.
 * ============================================================
 */

import React from 'react';
import { useAppStore } from '../../../store/app-store';
import { Badge, formatMoney, formatDate } from '../../shared/components';
import type { CustomerCRM, CustomerStatus, CustomerTier } from '../types';

interface CustomerProfileProps {
  customer: CustomerCRM;
}

export function CustomerProfile({ customer }: CustomerProfileProps) {
  const { language } = useAppStore();

  const statusVariant = (status: CustomerStatus): 'success' | 'pending' | 'danger' | 'neutral' | 'warning' => {
    switch (status) {
      case 'active': return 'success';
      case 'inactive': return 'warning';
      case 'blocked': return 'danger';
      case 'churned': return 'neutral';
    }
  };

  const statusLabel: Record<CustomerStatus, { ar: string; en: string }> = {
    active: { ar: 'نشط', en: 'Active' },
    inactive: { ar: 'غير نشط', en: 'Inactive' },
    blocked: { ar: 'محظور', en: 'Blocked' },
    churned: { ar: 'مفقود', en: 'Churned' },
  };

  const tierConfig: Record<CustomerTier, { ar: string; en: string; color: string; icon: string }> = {
    bronze: { ar: 'برونزي', en: 'Bronze', color: 'bg-amber-100 text-amber-700 border-amber-300', icon: '🥉' },
    silver: { ar: 'فضي', en: 'Silver', color: 'bg-gray-100 text-gray-700 border-gray-300', icon: '🥈' },
    gold: { ar: 'ذهبي', en: 'Gold', color: 'bg-yellow-100 text-yellow-800 border-yellow-300', icon: '🥇' },
    platinum: { ar: 'بلاتيني', en: 'Platinum', color: 'bg-purple-100 text-purple-700 border-purple-300', icon: '💎' },
  };

  const tier = tierConfig[customer.tier];

  return (
    <div className="card p-6">
      <div className="flex flex-col lg:flex-row gap-6">
        {/* Avatar & Basic Info */}
        <div className="flex items-start gap-4 flex-1">
          <div className="w-16 h-16 rounded-2xl bg-arooba-100 flex items-center justify-center text-2xl font-bold text-arooba-600 shrink-0">
            {customer.fullNameAr.charAt(0)}
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-3 flex-wrap">
              <h2 className="text-xl font-bold text-earth-900">{customer.fullNameAr}</h2>
              <Badge variant={statusVariant(customer.status)}>
                {statusLabel[customer.status][language]}
              </Badge>
              <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium border ${tier.color}`}>
                {tier.icon} {tier[language]}
              </span>
            </div>
            <p className="text-sm text-earth-500 mt-1">{customer.fullName}</p>
            <div className="flex flex-wrap gap-4 mt-3 text-sm text-earth-600">
              <span className="dir-ltr">{customer.mobileNumber}</span>
              {customer.email && <span>{customer.email}</span>}
              <span>{language === 'ar' ? 'اللغة:' : 'Lang:'} {customer.preferredLanguage.toUpperCase()}</span>
            </div>
            <p className="text-xs text-earth-400 mt-2">
              {language === 'ar' ? 'مسجل منذ' : 'Registered'}: {formatDate(customer.registeredAt)}
              {' | '}
              ID: <span className="font-mono">{customer.id}</span>
            </p>
          </div>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-2 gap-3 lg:w-72">
          <div className="p-3 rounded-xl bg-earth-50">
            <p className="text-xs text-earth-500">{language === 'ar' ? 'إجمالي الإنفاق' : 'Total Spent'}</p>
            <p className="text-lg font-bold text-earth-900">{formatMoney(customer.totalSpent)}</p>
          </div>
          <div className="p-3 rounded-xl bg-arooba-50">
            <p className="text-xs text-earth-500">{language === 'ar' ? 'نقاط الولاء' : 'Loyalty Points'}</p>
            <p className="text-lg font-bold text-arooba-700">{customer.loyaltyPoints.toLocaleString()}</p>
          </div>
          <div className="p-3 rounded-xl bg-nile-50">
            <p className="text-xs text-earth-500">{language === 'ar' ? 'المحفظة' : 'Wallet'}</p>
            <p className="text-lg font-bold text-nile-700">{formatMoney(customer.walletBalance)}</p>
          </div>
          <div className="p-3 rounded-xl bg-blue-50">
            <p className="text-xs text-earth-500">{language === 'ar' ? 'الطلبات' : 'Orders'}</p>
            <p className="text-lg font-bold text-blue-700">{customer.totalOrders}</p>
          </div>
        </div>
      </div>

      {/* Addresses */}
      {customer.addresses.length > 0 && (
        <div className="mt-6 pt-5 border-t border-earth-100">
          <h3 className="text-sm font-semibold text-earth-700 mb-3">
            {language === 'ar' ? 'العناوين' : 'Addresses'} ({customer.addresses.length})
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {customer.addresses.map((addr) => (
              <div
                key={addr.id}
                className={`p-3 rounded-xl border text-sm ${
                  addr.isDefault ? 'border-arooba-300 bg-arooba-50/50' : 'border-earth-200 bg-earth-50/50'
                }`}
              >
                <div className="flex items-center gap-2 mb-1">
                  <span className="font-medium text-earth-700">{addr.label}</span>
                  {addr.isDefault && (
                    <span className="text-[10px] px-1.5 py-0.5 rounded bg-arooba-200 text-arooba-700">
                      {language === 'ar' ? 'افتراضي' : 'Default'}
                    </span>
                  )}
                </div>
                <p className="text-earth-500 text-xs">{addr.fullAddress}</p>
                <p className="text-earth-400 text-xs mt-0.5">{addr.city}، {addr.governorate}</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Referral Info */}
      <div className="mt-5 pt-5 border-t border-earth-100">
        <div className="flex flex-wrap gap-6 text-sm">
          <div>
            <span className="text-earth-500">{language === 'ar' ? 'كود الإحالة:' : 'Referral Code:'}</span>{' '}
            <span className="font-mono font-medium text-arooba-600 bg-arooba-50 px-2 py-0.5 rounded">{customer.referralCode}</span>
          </div>
          {customer.referredBy && (
            <div>
              <span className="text-earth-500">{language === 'ar' ? 'محال من:' : 'Referred by:'}</span>{' '}
              <span className="font-mono text-earth-700">{customer.referredBy}</span>
            </div>
          )}
          <div>
            <span className="text-earth-500">{language === 'ar' ? 'إحالات ناجحة:' : 'Successful Referrals:'}</span>{' '}
            <span className="font-bold text-nile-600">{customer.referralCount}</span>
          </div>
          <div>
            <span className="text-earth-500">{language === 'ar' ? 'متوسط التقييم:' : 'Avg Rating:'}</span>{' '}
            <span className="font-bold text-yellow-600">
              {customer.averageRating > 0 ? `${customer.averageRating} / 5` : '-'}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
