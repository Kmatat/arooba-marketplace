/**
 * ============================================================
 * AROOBA MARKETPLACE — Order Management Module
 * ============================================================
 * 
 * Manages the full order lifecycle:
 * Pending → Accepted → Ready to Ship → In Transit → Delivered → Returned
 * 
 * BUSINESS CONTEXT:
 * Orders can be "split" across multiple vendors/locations.
 * A single customer order might create 2-3 separate shipments
 * from different pickup points. Each shipment has its own
 * tracking number and delivery fee.
 * 
 * KEY CHALLENGES:
 * 1. Split shipments need transparent fee presentation
 * 2. COD handling — courier must deposit within 48 hours
 * 3. "Rejected - Shipping Refusal" is a major COD market issue
 * ============================================================
 */

import React, { useState } from 'react';
import { StatCard, Badge, SectionHeader, formatMoney, formatDate } from '../../shared/components';
import type { Order, OrderStatus } from '../../shared/types';

// Sample orders data
const sampleOrders: Order[] = [
  {
    id: 'ORD-20251203-001', customerId: 'c-001', customerName: 'أحمد محمد',
    items: [{
      id: 'oi-001', productId: 'p-001', productTitle: 'فازة سيراميك مرسومة يدوياً',
      productImage: '', vendorId: 'v-001', vendorName: 'خزفيات حسن', quantity: 2,
      unitPrice: 130, totalPrice: 260, pickupLocationId: 'loc-001',
      bucketA_vendorRevenue: 200, bucketB_vendorVat: 28, bucketC_aroobaRevenue: 50,
      bucketD_aroobaVat: 7, bucketE_logisticsFee: 45,
    }],
    subtotal: 260, totalDeliveryFee: 45, totalAmount: 305, paymentMethod: 'cod',
    deliveryAddress: 'شارع التحرير، وسط البلد', deliveryCity: 'القاهرة', deliveryZoneId: 'cairo',
    shipments: [{
      id: 'SH-001-A', orderId: 'ORD-20251203-001', pickupLocationId: 'loc-001',
      trackingNumber: 'SC-2025-78901', courierProvider: 'SmartCom', deliveryFee: 45,
      codAmountDue: 305, status: 'in_transit', estimatedDeliveryDate: '2025-12-06',
    }],
    status: 'in_transit', createdAt: '2025-12-03T14:30:00Z', updatedAt: '2025-12-04T09:00:00Z',
  },
  {
    id: 'ORD-20251202-015', customerId: 'c-002', customerName: 'سارة عبدالله',
    items: [
      {
        id: 'oi-002', productId: 'p-004', productTitle: 'حقيبة ماسنجر جلد طبيعي',
        productImage: '', vendorId: 'v-004', vendorName: 'جلود خان الخليلي', quantity: 1,
        unitPrice: 660, totalPrice: 660, pickupLocationId: 'loc-004',
        bucketA_vendorRevenue: 550, bucketB_vendorVat: 77, bucketC_aroobaRevenue: 110,
        bucketD_aroobaVat: 15.4, bucketE_logisticsFee: 35,
      },
      {
        id: 'oi-003', productId: 'p-005', productTitle: 'صابون زيت زيتون طبيعي',
        productImage: '', vendorId: 'v-003', vendorName: 'يدوية نادية', quantity: 3,
        unitPrice: 55, totalPrice: 165, pickupLocationId: 'loc-003',
        bucketA_vendorRevenue: 90, bucketB_vendorVat: 0, bucketC_aroobaRevenue: 64.5,
        bucketD_aroobaVat: 9.03, bucketE_logisticsFee: 40,
      },
    ],
    subtotal: 825, totalDeliveryFee: 75, totalAmount: 900, paymentMethod: 'fawry',
    deliveryAddress: 'كورنيش النيل، المعادي', deliveryCity: 'القاهرة', deliveryZoneId: 'cairo',
    shipments: [
      { id: 'SH-015-A', orderId: 'ORD-20251202-015', pickupLocationId: 'loc-004', trackingNumber: 'SC-2025-78910', courierProvider: 'SmartCom', deliveryFee: 35, codAmountDue: 0, status: 'delivered', estimatedDeliveryDate: '2025-12-04', actualDeliveryDate: '2025-12-04' },
      { id: 'SH-015-B', orderId: 'ORD-20251202-015', pickupLocationId: 'loc-003', trackingNumber: 'SC-2025-78911', courierProvider: 'SmartCom', deliveryFee: 40, codAmountDue: 0, status: 'delivered', estimatedDeliveryDate: '2025-12-04', actualDeliveryDate: '2025-12-04' },
    ],
    status: 'delivered', createdAt: '2025-12-02T11:00:00Z', updatedAt: '2025-12-04T16:00:00Z',
  },
  {
    id: 'ORD-20251201-042', customerId: 'c-003', customerName: 'محمد حسين',
    items: [{
      id: 'oi-004', productId: 'p-002', productTitle: 'سجادة كليم سيوة مطرزة',
      productImage: '', vendorId: 'v-002', vendorName: 'سيوة للمنسوجات', quantity: 1,
      unitPrice: 1800, totalPrice: 1800, pickupLocationId: 'loc-002',
      bucketA_vendorRevenue: 1500, bucketB_vendorVat: 210, bucketC_aroobaRevenue: 300,
      bucketD_aroobaVat: 42, bucketE_logisticsFee: 85,
    }],
    subtotal: 1800, totalDeliveryFee: 85, totalAmount: 1885, paymentMethod: 'cod',
    deliveryAddress: 'شارع الهرم، الجيزة', deliveryCity: 'الجيزة', deliveryZoneId: 'cairo',
    shipments: [{
      id: 'SH-042-A', orderId: 'ORD-20251201-042', pickupLocationId: 'loc-002',
      courierProvider: 'SmartCom', deliveryFee: 85, codAmountDue: 1885, status: 'pending',
      estimatedDeliveryDate: '2025-12-05',
    }],
    status: 'pending', createdAt: '2025-12-01T09:00:00Z', updatedAt: '2025-12-01T09:00:00Z',
  },
];

const STATUS_FLOW: OrderStatus[] = ['pending', 'accepted', 'ready_to_ship', 'in_transit', 'delivered'];

const statusConfig: Record<string, { label: string; variant: 'pending' | 'info' | 'warning' | 'success' | 'danger' | 'neutral' }> = {
  pending: { label: 'قيد الانتظار', variant: 'pending' },
  accepted: { label: 'مقبول', variant: 'info' },
  ready_to_ship: { label: 'جاهز للشحن', variant: 'warning' },
  in_transit: { label: 'في الطريق', variant: 'info' },
  delivered: { label: 'تم التسليم', variant: 'success' },
  returned: { label: 'مُرتجع', variant: 'danger' },
  cancelled: { label: 'ملغي', variant: 'danger' },
  rejected_shipping: { label: 'رفض استلام', variant: 'danger' },
};

const paymentLabels: Record<string, string> = {
  cod: 'نقد عند الاستلام',
  fawry: 'فوري',
  card: 'بطاقة',
  wallet: 'محفظة',
};

export function OrderManagement() {
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [expandedOrder, setExpandedOrder] = useState<string | null>(null);

  const filteredOrders = statusFilter === 'all'
    ? sampleOrders
    : sampleOrders.filter((o) => o.status === statusFilter);

  const totalGmv = sampleOrders.reduce((sum, o) => sum + o.totalAmount, 0);
  const codOrders = sampleOrders.filter((o) => o.paymentMethod === 'cod').length;
  const deliveredOrders = sampleOrders.filter((o) => o.status === 'delivered').length;

  return (
    <div className="space-y-6">
      {/* KPIs */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="إجمالي الطلبات" value={sampleOrders.length} accent="orange" icon={<span className="text-2xl">🛒</span>} />
        <StatCard label="إجمالي القيمة" value={formatMoney(totalGmv)} accent="green" icon={<span className="text-2xl">💰</span>} />
        <StatCard label="طلبات COD" value={`${codOrders} من ${sampleOrders.length}`} accent="blue" icon={<span className="text-2xl">💵</span>} />
        <StatCard label="تم التسليم" value={deliveredOrders} accent="green" icon={<span className="text-2xl">✅</span>} />
      </div>

      {/* Status Filter Tabs */}
      <div className="card p-4">
        <div className="flex flex-wrap gap-2">
          <button
            onClick={() => setStatusFilter('all')}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
              statusFilter === 'all' ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
            }`}
          >
            الكل ({sampleOrders.length})
          </button>
          {Object.entries(statusConfig).slice(0, 6).map(([key, config]) => {
            const count = sampleOrders.filter((o) => o.status === key).length;
            if (count === 0) return null;
            return (
              <button
                key={key}
                onClick={() => setStatusFilter(key)}
                className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                  statusFilter === key ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                }`}
              >
                {config.label} ({count})
              </button>
            );
          })}
        </div>
      </div>

      {/* Orders List */}
      <div className="space-y-3">
        {filteredOrders.map((order) => {
          const isExpanded = expandedOrder === order.id;
          const config = statusConfig[order.status] || statusConfig.pending;
          const isSplit = order.shipments.length > 1;

          return (
            <div key={order.id} className="card overflow-hidden animate-fade-in">
              {/* Order Header */}
              <div
                className="p-4 cursor-pointer hover:bg-earth-50/50 transition-colors"
                onClick={() => setExpandedOrder(isExpanded ? null : order.id)}
              >
                <div className="flex flex-wrap items-center gap-4">
                  {/* Order ID & Status */}
                  <div className="flex-1 min-w-[200px]">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="font-mono text-sm font-bold text-earth-800 dir-ltr">{order.id}</span>
                      <Badge variant={config.variant}>{config.label}</Badge>
                      {isSplit && <Badge variant="info" dot={false}>📦 طلب مقسم ({order.shipments.length})</Badge>}
                    </div>
                    <p className="text-xs text-earth-500">{order.customerName} • {order.deliveryCity}</p>
                  </div>

                  {/* Payment Method */}
                  <div className="text-center">
                    <p className="text-xs text-earth-400">الدفع</p>
                    <Badge variant={order.paymentMethod === 'cod' ? 'warning' : 'success'} dot={false}>
                      {paymentLabels[order.paymentMethod]}
                    </Badge>
                  </div>

                  {/* Items Count */}
                  <div className="text-center">
                    <p className="text-xs text-earth-400">المنتجات</p>
                    <p className="font-medium text-earth-700">{order.items.length}</p>
                  </div>

                  {/* Total */}
                  <div className="text-center min-w-[100px]">
                    <p className="text-xs text-earth-400">الإجمالي</p>
                    <p className="font-bold text-earth-900">{formatMoney(order.totalAmount)}</p>
                  </div>

                  {/* Date */}
                  <div className="text-center">
                    <p className="text-xs text-earth-400">التاريخ</p>
                    <p className="text-xs text-earth-600">{formatDate(order.createdAt)}</p>
                  </div>

                  {/* Expand Icon */}
                  <span className={`text-earth-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`}>
                    ▼
                  </span>
                </div>
              </div>

              {/* Expanded Details */}
              {isExpanded && (
                <div className="border-t border-earth-100 animate-slide-down">
                  {/* Status Timeline */}
                  <div className="px-4 py-3 bg-earth-50/50">
                    <p className="text-xs font-semibold text-earth-500 mb-2">مسار الطلب</p>
                    <div className="flex items-center gap-1">
                      {STATUS_FLOW.map((step, i) => {
                        const stepIdx = STATUS_FLOW.indexOf(order.status as OrderStatus);
                        const isComplete = i <= stepIdx;
                        const isCurrent = i === stepIdx;
                        const stepConfig = statusConfig[step];
                        return (
                          <React.Fragment key={step}>
                            <div className={`flex items-center gap-1 px-2 py-1 rounded-lg text-xs ${
                              isCurrent ? 'bg-arooba-100 text-arooba-700 font-bold' :
                              isComplete ? 'bg-nile-100 text-nile-700' :
                              'bg-earth-100 text-earth-400'
                            }`}>
                              {isComplete && !isCurrent ? '✓' : ''} {stepConfig?.label}
                            </div>
                            {i < STATUS_FLOW.length - 1 && (
                              <div className={`w-6 h-0.5 ${isComplete ? 'bg-nile-400' : 'bg-earth-200'}`} />
                            )}
                          </React.Fragment>
                        );
                      })}
                    </div>
                  </div>

                  {/* Items */}
                  <div className="p-4">
                    <p className="text-xs font-semibold text-earth-500 mb-3">تفاصيل المنتجات</p>
                    {order.items.map((item) => (
                      <div key={item.id} className="flex items-center gap-3 py-2 border-b border-earth-50 last:border-b-0">
                        <div className="w-10 h-10 rounded-lg bg-earth-100 flex items-center justify-center text-lg">🏺</div>
                        <div className="flex-1">
                          <p className="text-sm font-medium text-earth-800">{item.productTitle}</p>
                          <p className="text-xs text-earth-400">{item.vendorName} • الكمية: {item.quantity}</p>
                        </div>
                        <p className="font-medium text-earth-800">{formatMoney(item.totalPrice)}</p>
                      </div>
                    ))}
                  </div>

                  {/* Shipments (if split) */}
                  {isSplit && (
                    <div className="px-4 pb-4">
                      <p className="text-xs font-semibold text-earth-500 mb-3">📦 الشحنات المنفصلة</p>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                        {order.shipments.map((shipment, idx) => (
                          <div key={shipment.id} className="p-3 rounded-xl bg-earth-50 border border-earth-100">
                            <div className="flex items-center justify-between mb-2">
                              <span className="text-xs font-bold text-earth-600">شحنة {idx + 1}</span>
                              <Badge variant={statusConfig[shipment.status]?.variant || 'neutral'}>
                                {statusConfig[shipment.status]?.label}
                              </Badge>
                            </div>
                            {shipment.trackingNumber && (
                              <p className="text-xs text-earth-500 dir-ltr mb-1">📍 {shipment.trackingNumber}</p>
                            )}
                            <p className="text-xs text-earth-500">رسوم التوصيل: {formatMoney(shipment.deliveryFee)}</p>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Financial Summary */}
                  <div className="px-4 pb-4">
                    <div className="p-3 rounded-xl bg-arooba-50/50 border border-arooba-100">
                      <p className="text-xs font-semibold text-arooba-700 mb-2">💰 الملخص المالي</p>
                      <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 text-xs">
                        <div>
                          <p className="text-earth-500">المنتجات</p>
                          <p className="font-bold text-earth-800">{formatMoney(order.subtotal)}</p>
                        </div>
                        <div>
                          <p className="text-earth-500">التوصيل</p>
                          <p className="font-bold text-earth-800">{formatMoney(order.totalDeliveryFee)}</p>
                        </div>
                        <div>
                          <p className="text-earth-500">الإجمالي</p>
                          <p className="font-bold text-arooba-700">{formatMoney(order.totalAmount)}</p>
                        </div>
                        <div>
                          <p className="text-earth-500">طريقة الدفع</p>
                          <p className="font-bold text-earth-800">{paymentLabels[order.paymentMethod]}</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
