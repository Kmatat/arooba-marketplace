/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer Orders Tab
 * ============================================================
 *
 * Displays full order history for a customer with status tracking,
 * payment methods, item details, and order timeline.
 * ============================================================
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { Badge, formatMoney, formatDate } from '../../shared/components';
import { mockCustomerOrders } from '../mock-customer-data';
import type { CustomerCRM, OrderStatusCRM, PaymentMethodCRM } from '../types';

interface CustomerOrdersProps {
  customer: CustomerCRM;
}

export function CustomerOrders({ customer }: CustomerOrdersProps) {
  const { language } = useAppStore();
  const [expandedOrderId, setExpandedOrderId] = useState<string | null>(null);
  const orders = mockCustomerOrders[customer.id] || [];

  const statusConfig: Record<OrderStatusCRM, { ar: string; en: string; variant: 'success' | 'pending' | 'info' | 'danger' | 'warning' | 'neutral' }> = {
    pending: { ar: 'قيد الانتظار', en: 'Pending', variant: 'pending' },
    accepted: { ar: 'مقبول', en: 'Accepted', variant: 'info' },
    ready_to_ship: { ar: 'جاهز للشحن', en: 'Ready to Ship', variant: 'info' },
    in_transit: { ar: 'في الطريق', en: 'In Transit', variant: 'warning' },
    delivered: { ar: 'تم التوصيل', en: 'Delivered', variant: 'success' },
    returned: { ar: 'مرتجع', en: 'Returned', variant: 'danger' },
    cancelled: { ar: 'ملغي', en: 'Cancelled', variant: 'neutral' },
  };

  const paymentLabels: Record<PaymentMethodCRM, { ar: string; en: string }> = {
    cod: { ar: 'الدفع عند الاستلام', en: 'Cash on Delivery' },
    fawry: { ar: 'فوري', en: 'Fawry' },
    card: { ar: 'بطاقة بنكية', en: 'Card' },
    wallet: { ar: 'المحفظة', en: 'Wallet' },
  };

  // Summary stats
  const deliveredOrders = orders.filter(o => o.status === 'delivered').length;
  const returnedOrders = orders.filter(o => o.status === 'returned').length;
  const pendingOrders = orders.filter(o => ['pending', 'accepted', 'ready_to_ship', 'in_transit'].includes(o.status)).length;

  return (
    <div className="space-y-4">
      {/* Order Summary Bar */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
        <div className="card p-3 text-center">
          <p className="text-xs text-earth-500">{language === 'ar' ? 'إجمالي الطلبات' : 'Total Orders'}</p>
          <p className="text-xl font-bold text-earth-900">{customer.totalOrders}</p>
        </div>
        <div className="card p-3 text-center">
          <p className="text-xs text-earth-500">{language === 'ar' ? 'تم التوصيل' : 'Delivered'}</p>
          <p className="text-xl font-bold text-nile-600">{deliveredOrders}</p>
        </div>
        <div className="card p-3 text-center">
          <p className="text-xs text-earth-500">{language === 'ar' ? 'جاري التنفيذ' : 'In Progress'}</p>
          <p className="text-xl font-bold text-blue-600">{pendingOrders}</p>
        </div>
        <div className="card p-3 text-center">
          <p className="text-xs text-earth-500">{language === 'ar' ? 'مرتجعات' : 'Returns'}</p>
          <p className="text-xl font-bold text-red-600">{returnedOrders}</p>
        </div>
        <div className="card p-3 text-center">
          <p className="text-xs text-earth-500">{language === 'ar' ? 'متوسط الطلب' : 'Avg. Order'}</p>
          <p className="text-xl font-bold text-arooba-600">{formatMoney(customer.averageOrderValue)}</p>
        </div>
      </div>

      {/* Orders List */}
      {orders.length === 0 ? (
        <div className="card p-12 text-center">
          <p className="text-earth-500">{language === 'ar' ? 'لا توجد طلبات بعد' : 'No orders yet'}</p>
        </div>
      ) : (
        <div className="space-y-3">
          {orders.map((order) => {
            const sc = statusConfig[order.status];
            const isExpanded = expandedOrderId === order.id;

            return (
              <div key={order.id} className="card overflow-hidden">
                {/* Order Header */}
                <div
                  className="p-4 flex flex-col sm:flex-row sm:items-center gap-3 cursor-pointer hover:bg-earth-50/50 transition-colors"
                  onClick={() => setExpandedOrderId(isExpanded ? null : order.id)}
                >
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-3 flex-wrap">
                      <span className="font-mono font-bold text-earth-900">{order.orderNumber}</span>
                      <Badge variant={sc.variant}>{sc[language]}</Badge>
                    </div>
                    <div className="flex flex-wrap gap-4 mt-1.5 text-xs text-earth-500">
                      <span>{formatDate(order.createdAt)}</span>
                      <span>{order.itemCount} {language === 'ar' ? 'منتج' : 'items'}</span>
                      <span>{paymentLabels[order.paymentMethod][language]}</span>
                      <span className="truncate max-w-[200px]">{order.deliveryAddress}</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <span className="text-lg font-bold text-earth-900">{formatMoney(order.totalAmount)}</span>
                    <span className={`text-earth-400 transition-transform ${isExpanded ? 'rotate-180' : ''}`}>
                      ▼
                    </span>
                  </div>
                </div>

                {/* Expanded Items */}
                {isExpanded && (
                  <div className="border-t border-earth-100 bg-earth-50/30">
                    <div className="p-4 space-y-3">
                      {order.items.map((item) => (
                        <div key={item.id} className="flex items-center gap-3 p-3 rounded-xl bg-white border border-earth-100">
                          <div className="w-12 h-12 rounded-lg bg-earth-100 flex items-center justify-center text-xl shrink-0">
                            📦
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="font-medium text-earth-900 text-sm truncate">{item.productTitle}</p>
                            <p className="text-xs text-earth-500">{item.vendorName}</p>
                          </div>
                          <div className="text-left">
                            <p className="text-sm font-medium text-earth-900">{formatMoney(item.totalPrice)}</p>
                            <p className="text-xs text-earth-500">x{item.quantity} @ {formatMoney(item.unitPrice)}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                    {order.deliveredAt && (
                      <div className="px-4 pb-4">
                        <p className="text-xs text-nile-600">
                          {language === 'ar' ? 'تم التوصيل في' : 'Delivered on'}: {formatDate(order.deliveredAt)}
                        </p>
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
