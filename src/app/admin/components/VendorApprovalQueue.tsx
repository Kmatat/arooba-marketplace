/**
 * ============================================================
 * AROOBA MARKETPLACE — Vendor Approval Queue
 * ============================================================
 *
 * Admin UI for reviewing and managing vendor action requests.
 * Implements the approval cycle workflow:
 * Vendor submits → Admin reviews → Approve/Reject → Audit logged
 *
 * BUSINESS CONTEXT:
 * In an ecommerce marketplace, vendors cannot freely modify
 * critical data (prices, bank details, products). Every significant
 * vendor action goes through an admin approval cycle to ensure
 * marketplace integrity and compliance.
 * ============================================================
 */

import React, { useState } from 'react';
import { SectionHeader, Badge, StatCard, formatDate } from '../../shared/components';
import { useAdminConfigStore } from '../../../store/admin-config-store';
import type { ApprovalStatus, VendorActionType, VendorActionRequest } from '../../shared/types';

// ──────────────────────────────────────────────
// LABELS & CONFIGURATION
// ──────────────────────────────────────────────

const actionTypeLabels: Record<VendorActionType, { label: string; labelAr: string }> = {
  product_listing: { label: 'Product Listing', labelAr: 'إدراج منتج' },
  price_change: { label: 'Price Change', labelAr: 'تغيير سعر' },
  profile_update: { label: 'Profile Update', labelAr: 'تحديث ملف شخصي' },
  sub_vendor_addition: { label: 'Sub-Vendor Addition', labelAr: 'إضافة مورد فرعي' },
  bank_details_change: { label: 'Bank Details Change', labelAr: 'تغيير بيانات بنكية' },
  category_change: { label: 'Category Change', labelAr: 'تغيير فئة' },
  bulk_stock_update: { label: 'Bulk Stock Update', labelAr: 'تحديث مخزون جماعي' },
  promotion_request: { label: 'Promotion Request', labelAr: 'طلب ترويج' },
  refund_request: { label: 'Refund Request', labelAr: 'طلب استرداد' },
  account_deactivation: { label: 'Account Deactivation', labelAr: 'إلغاء حساب' },
};

const statusConfig: Record<ApprovalStatus, { labelAr: string; variant: 'pending' | 'success' | 'danger' | 'neutral' | 'warning' }> = {
  pending: { labelAr: 'قيد الانتظار', variant: 'pending' },
  approved: { labelAr: 'تمت الموافقة', variant: 'success' },
  rejected: { labelAr: 'مرفوض', variant: 'danger' },
  cancelled: { labelAr: 'ملغي', variant: 'neutral' },
  expired: { labelAr: 'منتهي الصلاحية', variant: 'warning' },
};

const priorityConfig: Record<number, { labelAr: string; color: string }> = {
  1: { labelAr: 'منخفض', color: 'text-earth-400' },
  2: { labelAr: 'متوسط', color: 'text-blue-500' },
  3: { labelAr: 'عالي', color: 'text-amber-500' },
  4: { labelAr: 'عاجل', color: 'text-red-500' },
};

// ──────────────────────────────────────────────
// REQUEST DETAIL MODAL
// ──────────────────────────────────────────────

function RequestDetailPanel({
  request,
  onApprove,
  onReject,
  onClose,
}: {
  request: VendorActionRequest;
  onApprove: (id: string, notes: string) => void;
  onReject: (id: string, notes: string) => void;
  onClose: () => void;
}) {
  const [notes, setNotes] = useState('');
  const [actionError, setActionError] = useState('');

  const handleApprove = () => {
    onApprove(request.id, notes);
    onClose();
  };

  const handleReject = () => {
    if (!notes.trim()) {
      setActionError('يجب إدخال سبب الرفض');
      return;
    }
    onReject(request.id, notes);
    onClose();
  };

  const renderJsonValues = (label: string, json?: string) => {
    if (!json) return null;
    try {
      const parsed = JSON.parse(json);
      return (
        <div className="mt-3">
          <p className="text-xs font-semibold text-earth-500 mb-1">{label}</p>
          <div className="p-2 rounded-lg bg-earth-50 font-mono text-xs dir-ltr overflow-x-auto">
            {Object.entries(parsed).map(([k, v]) => (
              <div key={k} className="flex gap-2">
                <span className="text-earth-400">{k}:</span>
                <span className="text-earth-700">{typeof v === 'object' ? JSON.stringify(v) : String(v)}</span>
              </div>
            ))}
          </div>
        </div>
      );
    } catch {
      return null;
    }
  };

  return (
    <div className="card overflow-hidden animate-slide-down">
      <div className="p-4 bg-earth-50 border-b border-earth-100 flex items-center justify-between">
        <div>
          <p className="font-bold text-earth-800">{request.vendorNameAr}</p>
          <p className="text-xs text-earth-500 dir-ltr">{request.vendorName}</p>
        </div>
        <button onClick={onClose} className="p-1 rounded-lg hover:bg-earth-200 text-earth-400">
          &#10005;
        </button>
      </div>

      <div className="p-4 space-y-4">
        {/* Request Info */}
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <div>
            <p className="text-xs text-earth-400">نوع الطلب</p>
            <Badge variant="info" dot={false}>
              {actionTypeLabels[request.actionType].labelAr}
            </Badge>
          </div>
          <div>
            <p className="text-xs text-earth-400">الحالة</p>
            <Badge variant={statusConfig[request.status].variant}>
              {statusConfig[request.status].labelAr}
            </Badge>
          </div>
          <div>
            <p className="text-xs text-earth-400">الأولوية</p>
            <span className={`text-sm font-bold ${priorityConfig[request.priority]?.color}`}>
              {priorityConfig[request.priority]?.labelAr}
            </span>
          </div>
          <div>
            <p className="text-xs text-earth-400">تاريخ الطلب</p>
            <p className="text-sm text-earth-700">{formatDate(request.createdAt)}</p>
          </div>
        </div>

        {/* Entity Info */}
        <div className="p-3 rounded-xl bg-earth-50">
          <p className="text-xs text-earth-400 mb-1">الكيان المتأثر</p>
          <p className="text-sm font-medium text-earth-700 dir-ltr">
            {request.entityType} {request.entityId ? `(${request.entityId})` : ''}
          </p>
        </div>

        {/* Justification */}
        {request.justification && (
          <div>
            <p className="text-xs font-semibold text-earth-500 mb-1">مبرر المورد</p>
            <p className="text-sm text-earth-700 p-3 rounded-xl bg-amber-50 border border-amber-100">
              {request.justification}
            </p>
          </div>
        )}

        {/* Current vs Proposed Values */}
        {renderJsonValues('القيم الحالية', request.currentValues)}
        {renderJsonValues('القيم المقترحة', request.proposedValues)}

        {/* Review Section (if already reviewed) */}
        {request.reviewedBy && (
          <div className="p-3 rounded-xl bg-nile-50 border border-nile-100">
            <p className="text-xs font-semibold text-nile-700 mb-2">تمت المراجعة</p>
            <div className="grid grid-cols-2 gap-2 text-xs">
              <div>
                <p className="text-earth-400">بواسطة</p>
                <p className="text-earth-700">{request.reviewedBy}</p>
              </div>
              <div>
                <p className="text-earth-400">التاريخ</p>
                <p className="text-earth-700">{request.reviewedAt ? formatDate(request.reviewedAt) : '—'}</p>
              </div>
            </div>
            {request.reviewNotes && (
              <div className="mt-2">
                <p className="text-earth-400">ملاحظات</p>
                <p className="text-earth-700">{request.reviewNotes}</p>
              </div>
            )}
          </div>
        )}

        {/* Action Buttons (only for pending) */}
        {request.status === 'pending' && (
          <div className="border-t border-earth-100 pt-4 space-y-3">
            <div>
              <label className="text-xs font-semibold text-earth-500 block mb-1">ملاحظات المراجعة</label>
              <textarea
                value={notes}
                onChange={(e) => { setNotes(e.target.value); setActionError(''); }}
                placeholder="أضف ملاحظات المراجعة (مطلوب للرفض)..."
                className="input w-full text-sm"
                rows={2}
              />
              {actionError && <p className="text-xs text-red-500 mt-1">{actionError}</p>}
            </div>
            <div className="flex gap-2">
              <button
                onClick={handleApprove}
                className="flex-1 px-4 py-2 rounded-xl bg-nile-500 text-white text-sm font-medium hover:bg-nile-600 transition-colors"
              >
                &#10003; موافقة
              </button>
              <button
                onClick={handleReject}
                className="flex-1 px-4 py-2 rounded-xl bg-red-500 text-white text-sm font-medium hover:bg-red-600 transition-colors"
              >
                &#10005; رفض
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────
// MAIN COMPONENT
// ──────────────────────────────────────────────

export function VendorApprovalQueue() {
  const { actionRequests, approveRequest, rejectRequest, getPendingCount } = useAdminConfigStore();
  const [statusFilter, setStatusFilter] = useState<ApprovalStatus | 'all'>('all');
  const [typeFilter, setTypeFilter] = useState<VendorActionType | 'all'>('all');
  const [selectedRequest, setSelectedRequest] = useState<string | null>(null);

  const filtered = actionRequests.filter((r) => {
    if (statusFilter !== 'all' && r.status !== statusFilter) return false;
    if (typeFilter !== 'all' && r.actionType !== typeFilter) return false;
    return true;
  });

  const pendingCount = getPendingCount();
  const approvedCount = actionRequests.filter(r => r.status === 'approved').length;
  const rejectedCount = actionRequests.filter(r => r.status === 'rejected').length;
  const highPriorityPending = actionRequests.filter(r => r.status === 'pending' && r.priority >= 3).length;

  const selected = selectedRequest ? actionRequests.find(r => r.id === selectedRequest) : null;

  return (
    <div className="space-y-6">
      <SectionHeader
        title="طابور الموافقات"
        subtitle="مراجعة واعتماد طلبات الموردين — كل إجراء يتم تسجيله في سجل المراجعة"
      />

      {/* KPI Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="قيد الانتظار" value={pendingCount} accent="orange" icon={<span className="text-2xl">&#9203;</span>} />
        <StatCard label="تمت الموافقة" value={approvedCount} accent="green" icon={<span className="text-2xl">&#10003;</span>} />
        <StatCard label="مرفوض" value={rejectedCount} accent="red" icon={<span className="text-2xl">&#10005;</span>} />
        <StatCard label="أولوية عالية" value={highPriorityPending} accent="blue" icon={<span className="text-2xl">&#9888;</span>} />
      </div>

      {/* Filters */}
      <div className="card p-4">
        <div className="flex flex-wrap gap-4">
          {/* Status Filter */}
          <div className="flex flex-wrap gap-1">
            <button
              onClick={() => setStatusFilter('all')}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                statusFilter === 'all' ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
              }`}
            >
              الكل ({actionRequests.length})
            </button>
            {(Object.keys(statusConfig) as ApprovalStatus[]).map((status) => {
              const count = actionRequests.filter(r => r.status === status).length;
              if (count === 0) return null;
              return (
                <button
                  key={status}
                  onClick={() => setStatusFilter(status)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                    statusFilter === status ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                  }`}
                >
                  {statusConfig[status].labelAr} ({count})
                </button>
              );
            })}
          </div>

          {/* Action Type Filter */}
          <select
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value as VendorActionType | 'all')}
            className="input text-xs py-1.5"
          >
            <option value="all">كل الأنواع</option>
            {(Object.keys(actionTypeLabels) as VendorActionType[]).map((type) => (
              <option key={type} value={type}>{actionTypeLabels[type].labelAr}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Content: List + Detail */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Request List */}
        <div className="space-y-2">
          {filtered.length === 0 ? (
            <div className="card p-8 text-center text-earth-400 text-sm">
              لا توجد طلبات تطابق الفلتر المحدد
            </div>
          ) : (
            filtered.map((request) => (
              <div
                key={request.id}
                onClick={() => setSelectedRequest(request.id)}
                className={`card p-4 cursor-pointer transition-all duration-200 ${
                  selectedRequest === request.id
                    ? 'ring-2 ring-arooba-400 bg-arooba-50/30'
                    : 'hover:bg-earth-50/50'
                }`}
              >
                <div className="flex items-start justify-between mb-2">
                  <div className="flex items-center gap-2">
                    <Badge variant={statusConfig[request.status].variant}>
                      {statusConfig[request.status].labelAr}
                    </Badge>
                    <Badge variant="info" dot={false}>
                      {actionTypeLabels[request.actionType].labelAr}
                    </Badge>
                  </div>
                  <span className={`text-xs font-bold ${priorityConfig[request.priority]?.color}`}>
                    P{request.priority}
                  </span>
                </div>

                <p className="font-medium text-earth-800 text-sm">{request.vendorNameAr}</p>
                <p className="text-xs text-earth-400 dir-ltr">{request.vendorName}</p>

                {request.justification && (
                  <p className="text-xs text-earth-500 mt-2 line-clamp-2">
                    {request.justification}
                  </p>
                )}

                <div className="flex items-center justify-between mt-3 text-xs text-earth-400">
                  <span>{formatDate(request.createdAt)}</span>
                  <span className="dir-ltr">{request.entityType}</span>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Detail Panel */}
        <div>
          {selected ? (
            <RequestDetailPanel
              request={selected}
              onApprove={approveRequest}
              onReject={rejectRequest}
              onClose={() => setSelectedRequest(null)}
            />
          ) : (
            <div className="card p-8 text-center text-earth-400">
              <p className="text-3xl mb-3">&#128269;</p>
              <p className="text-sm">اختر طلبًا من القائمة لعرض التفاصيل</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
