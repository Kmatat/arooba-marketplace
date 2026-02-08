/**
 * AROOBA MARKETPLACE — User Activity Log Component
 * Displays a filterable, real-time log of all user activity events.
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader, Badge, formatMoney, formatDate } from '../../shared/components';
import { mockActivityLog } from '../mock-analytics-data';
import type { UserActivityAction } from '../types';

const ACTION_CONFIG: Record<UserActivityAction, {
  labelAr: string;
  labelEn: string;
  variant: 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'pending';
  icon: string;
}> = {
  product_viewed: { labelAr: 'مشاهدة منتج', labelEn: 'Product Viewed', variant: 'info', icon: '👁' },
  added_to_cart: { labelAr: 'إضافة للسلة', labelEn: 'Added to Cart', variant: 'pending', icon: '🛒' },
  removed_from_cart: { labelAr: 'إزالة من السلة', labelEn: 'Removed from Cart', variant: 'neutral', icon: '❌' },
  cart_quantity_changed: { labelAr: 'تغيير الكمية', labelEn: 'Qty Changed', variant: 'neutral', icon: '🔢' },
  checkout_started: { labelAr: 'بدء الدفع', labelEn: 'Checkout Started', variant: 'warning', icon: '💳' },
  purchase_completed: { labelAr: 'إتمام الشراء', labelEn: 'Purchase Completed', variant: 'success', icon: '✅' },
  checkout_abandoned: { labelAr: 'تخلي عن الدفع', labelEn: 'Checkout Abandoned', variant: 'danger', icon: '🚪' },
  product_searched: { labelAr: 'بحث عن منتج', labelEn: 'Product Searched', variant: 'info', icon: '🔍' },
  filter_applied: { labelAr: 'تطبيق فلتر', labelEn: 'Filter Applied', variant: 'neutral', icon: '⚙' },
  category_viewed: { labelAr: 'مشاهدة فئة', labelEn: 'Category Viewed', variant: 'info', icon: '📂' },
  wishlist_added: { labelAr: 'إضافة للمفضلة', labelEn: 'Wishlist Added', variant: 'pending', icon: '❤' },
  product_shared: { labelAr: 'مشاركة منتج', labelEn: 'Product Shared', variant: 'info', icon: '📤' },
  session_started: { labelAr: 'بدء جلسة', labelEn: 'Session Started', variant: 'info', icon: '🔵' },
  session_ended: { labelAr: 'انتهاء جلسة', labelEn: 'Session Ended', variant: 'neutral', icon: '⚫' },
  order_history_viewed: { labelAr: 'مشاهدة الطلبات', labelEn: 'Orders Viewed', variant: 'neutral', icon: '📋' },
  return_initiated: { labelAr: 'طلب إرجاع', labelEn: 'Return Initiated', variant: 'danger', icon: '↩' },
  related_product_clicked: { labelAr: 'نقر منتج مرتبط', labelEn: 'Related Product Click', variant: 'info', icon: '🔗' },
  vendor_page_viewed: { labelAr: 'مشاهدة صفحة مورد', labelEn: 'Vendor Page Viewed', variant: 'info', icon: '🏪' },
};

type FilterAction = 'all' | 'purchase_completed' | 'added_to_cart' | 'checkout_abandoned' | 'product_viewed' | 'product_searched' | 'return_initiated';

export function UserActivityLog() {
  const { language } = useAppStore();
  const [filter, setFilter] = useState<FilterAction>('all');

  const filterOptions: { key: FilterAction; labelAr: string; labelEn: string }[] = [
    { key: 'all', labelAr: 'الكل', labelEn: 'All' },
    { key: 'purchase_completed', labelAr: 'المشتريات', labelEn: 'Purchases' },
    { key: 'added_to_cart', labelAr: 'السلة', labelEn: 'Cart' },
    { key: 'checkout_abandoned', labelAr: 'تخلي', labelEn: 'Abandoned' },
    { key: 'product_viewed', labelAr: 'المشاهدات', labelEn: 'Views' },
    { key: 'product_searched', labelAr: 'البحث', labelEn: 'Searches' },
    { key: 'return_initiated', labelAr: 'الإرجاع', labelEn: 'Returns' },
  ];

  const filtered = filter === 'all'
    ? mockActivityLog
    : mockActivityLog.filter((e) => e.action === filter);

  return (
    <div className="space-y-4">
      <div className="card p-5">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4">
          <SectionHeader
            title={language === 'ar' ? 'سجل نشاط المستخدمين' : 'User Activity Log'}
            subtitle={language === 'ar' ? 'جميع إجراءات المستخدمين بالتفصيل' : 'All user actions in detail'}
          />
          <div className="flex flex-wrap gap-1">
            {filterOptions.map((opt) => (
              <button
                key={opt.key}
                onClick={() => setFilter(opt.key)}
                className={`
                  px-3 py-1.5 rounded-lg text-xs font-medium transition-colors
                  ${filter === opt.key
                    ? 'bg-arooba-100 text-arooba-700'
                    : 'text-earth-500 hover:bg-earth-100'
                  }
                `}
              >
                {language === 'ar' ? opt.labelAr : opt.labelEn}
              </button>
            ))}
          </div>
        </div>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>{language === 'ar' ? 'الوقت' : 'Time'}</th>
                <th>{language === 'ar' ? 'المستخدم' : 'User'}</th>
                <th>{language === 'ar' ? 'الإجراء' : 'Action'}</th>
                <th>{language === 'ar' ? 'التفاصيل' : 'Details'}</th>
                <th>{language === 'ar' ? 'الجهاز' : 'Device'}</th>
                <th>{language === 'ar' ? 'قيمة السلة' : 'Cart Value'}</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((event) => {
                const config = ACTION_CONFIG[event.action];
                return (
                  <tr key={event.id}>
                    <td className="text-xs text-earth-500 whitespace-nowrap">
                      {new Date(event.createdAt).toLocaleTimeString(language === 'ar' ? 'ar-EG' : 'en-US', {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                      <br />
                      <span className="text-[10px] text-earth-400">
                        {formatDate(event.createdAt)}
                      </span>
                    </td>
                    <td>
                      <div className="flex items-center gap-2">
                        <div className="w-7 h-7 rounded-full bg-earth-100 flex items-center justify-center text-xs font-bold text-earth-600">
                          {event.userName.charAt(0)}
                        </div>
                        <div>
                          <p className="text-xs font-medium text-earth-800">{event.userName}</p>
                          <p className="text-[10px] text-earth-400">{event.userId}</p>
                        </div>
                      </div>
                    </td>
                    <td>
                      <div className="flex items-center gap-1.5">
                        <span className="text-sm">{config.icon}</span>
                        <Badge variant={config.variant}>
                          {language === 'ar' ? config.labelAr : config.labelEn}
                        </Badge>
                      </div>
                    </td>
                    <td className="text-xs text-earth-600 max-w-[200px]">
                      {event.productTitle && (
                        <p className="truncate font-medium">{event.productTitle}</p>
                      )}
                      {event.searchQuery && (
                        <p className="truncate">
                          <span className="text-earth-400">{language === 'ar' ? 'بحث: ' : 'Search: '}</span>
                          "{event.searchQuery}"
                        </p>
                      )}
                      {event.orderId && (
                        <p className="text-[10px] text-earth-400">{language === 'ar' ? 'طلب: ' : 'Order: '}{event.orderId}</p>
                      )}
                      {event.categoryId && !event.productTitle && (
                        <p className="truncate">{language === 'ar' ? 'فئة: ' : 'Category: '}{event.categoryId}</p>
                      )}
                    </td>
                    <td>
                      {event.deviceType && (
                        <span className={`
                          inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium
                          ${event.deviceType === 'mobile' ? 'bg-arooba-50 text-arooba-600' : ''}
                          ${event.deviceType === 'desktop' ? 'bg-blue-50 text-blue-600' : ''}
                          ${event.deviceType === 'tablet' ? 'bg-nile-50 text-nile-600' : ''}
                        `}>
                          {event.deviceType === 'mobile' ? '📱' : event.deviceType === 'desktop' ? '💻' : '📟'}{' '}
                          {event.deviceType}
                        </span>
                      )}
                    </td>
                    <td className="dir-ltr text-xs">
                      {event.cartValue != null ? (
                        <div>
                          <p className="font-medium text-earth-800">{formatMoney(event.cartValue)}</p>
                          {event.cartItemCount != null && (
                            <p className="text-[10px] text-earth-400">
                              {event.cartItemCount} {language === 'ar' ? 'عنصر' : 'items'}
                            </p>
                          )}
                        </div>
                      ) : (
                        <span className="text-earth-300">-</span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {filtered.length === 0 && (
          <div className="text-center py-12 text-earth-400 text-sm">
            {language === 'ar' ? 'لا توجد أحداث مطابقة' : 'No matching events'}
          </div>
        )}
      </div>
    </div>
  );
}
