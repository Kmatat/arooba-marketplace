/**
 * ============================================================
 * AROOBA MARKETPLACE — Audit Trail Viewer
 * ============================================================
 *
 * Admin UI for viewing the complete platform audit log.
 * Records every significant action: config changes, vendor
 * approvals/rejections, product status transitions, and more.
 *
 * BUSINESS CONTEXT:
 * Full audit trail is essential for:
 * - Regulatory compliance (Egyptian e-commerce regulations)
 * - Dispute resolution (vendor claims, customer complaints)
 * - Internal accountability (who changed what and when)
 * - Security monitoring (unauthorized access detection)
 * ============================================================
 */

import React, { useState } from 'react';
import { SectionHeader, Badge, StatCard, formatDate } from '../../shared/components';
import { useAdminConfigStore } from '../../../store/admin-config-store';
import type { AuditAction, AuditLogEntry } from '../../shared/types';

// ──────────────────────────────────────────────
// LABELS & CONFIGURATION
// ──────────────────────────────────────────────

const actionConfig: Record<AuditAction, { labelAr: string; variant: 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'pending' }> = {
  create: { labelAr: 'إنشاء', variant: 'success' },
  update: { labelAr: 'تحديث', variant: 'info' },
  delete: { labelAr: 'حذف', variant: 'danger' },
  status_change: { labelAr: 'تغيير حالة', variant: 'warning' },
  approve: { labelAr: 'موافقة', variant: 'success' },
  reject: { labelAr: 'رفض', variant: 'danger' },
  config_change: { labelAr: 'تغيير إعداد', variant: 'pending' },
  login: { labelAr: 'تسجيل دخول', variant: 'neutral' },
  export: { labelAr: 'تصدير', variant: 'info' },
  bulk_operation: { labelAr: 'عملية جماعية', variant: 'warning' },
};

const entityTypeLabels: Record<string, string> = {
  Product: 'منتج',
  Vendor: 'مورد',
  ParentVendor: 'مورد',
  SubVendor: 'مورد فرعي',
  Order: 'طلب',
  PlatformConfig: 'إعداد المنصة',
  VendorActionRequest: 'طلب مورد',
  Customer: 'عميل',
};

// ──────────────────────────────────────────────
// LOG ENTRY ROW
// ──────────────────────────────────────────────

function LogEntryRow({ entry, isExpanded, onToggle }: {
  entry: AuditLogEntry;
  isExpanded: boolean;
  onToggle: () => void;
}) {
  const config = actionConfig[entry.action];

  const renderJsonDiff = (label: string, json?: string) => {
    if (!json) return null;
    try {
      const parsed = JSON.parse(json);
      return (
        <div>
          <p className="text-xs font-semibold text-earth-400 mb-1">{label}</p>
          <div className="p-2 rounded-lg bg-earth-50 font-mono text-xs dir-ltr">
            {Object.entries(parsed).map(([k, v]) => (
              <div key={k}>
                <span className="text-earth-400">{k}: </span>
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
    <div className="border-b border-earth-100 last:border-b-0">
      <div
        onClick={onToggle}
        className="flex items-center gap-3 px-4 py-3 cursor-pointer hover:bg-earth-50/50 transition-colors"
      >
        {/* Timestamp */}
        <div className="w-28 shrink-0">
          <p className="text-xs text-earth-700">{formatDate(entry.createdAt)}</p>
          <p className="text-[10px] text-earth-400 dir-ltr">
            {new Date(entry.createdAt).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}
          </p>
        </div>

        {/* Action Badge */}
        <Badge variant={config.variant} dot={false}>
          {config.labelAr}
        </Badge>

        {/* Entity Type */}
        <Badge variant="neutral" dot={false}>
          {entityTypeLabels[entry.entityType] || entry.entityType}
        </Badge>

        {/* Description */}
        <div className="flex-1 min-w-0">
          <p className="text-sm text-earth-700 truncate">{entry.descriptionAr || entry.description}</p>
        </div>

        {/* User */}
        <div className="shrink-0 text-left">
          <p className="text-xs font-medium text-earth-600">{entry.userName}</p>
          <p className="text-[10px] text-earth-400">{entry.userRole}</p>
        </div>

        {/* Expand */}
        <span className={`text-earth-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`}>
          &#9660;
        </span>
      </div>

      {isExpanded && (
        <div className="px-4 pb-4 pt-0 animate-slide-down">
          <div className="p-3 rounded-xl bg-earth-50 space-y-3">
            {/* Meta Info */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 text-xs">
              <div>
                <p className="text-earth-400">معرّف المستخدم</p>
                <p className="text-earth-700 dir-ltr">{entry.userId}</p>
              </div>
              <div>
                <p className="text-earth-400">الكيان</p>
                <p className="text-earth-700 dir-ltr">{entry.entityType} {entry.entityId ? `#${entry.entityId}` : ''}</p>
              </div>
              {entry.ipAddress && (
                <div>
                  <p className="text-earth-400">عنوان IP</p>
                  <p className="text-earth-700 dir-ltr">{entry.ipAddress}</p>
                </div>
              )}
              {entry.vendorActionRequestId && (
                <div>
                  <p className="text-earth-400">طلب المورد</p>
                  <p className="text-earth-700 dir-ltr">{entry.vendorActionRequestId}</p>
                </div>
              )}
            </div>

            {/* Full Description */}
            <div>
              <p className="text-xs font-semibold text-earth-400 mb-1">الوصف الكامل</p>
              <p className="text-sm text-earth-700">{entry.description}</p>
              {entry.descriptionAr && (
                <p className="text-sm text-earth-600 mt-1">{entry.descriptionAr}</p>
              )}
            </div>

            {/* Value Diff */}
            {(entry.oldValues || entry.newValues) && (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {renderJsonDiff('القيم السابقة', entry.oldValues)}
                {renderJsonDiff('القيم الجديدة', entry.newValues)}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

// ──────────────────────────────────────────────
// MAIN COMPONENT
// ──────────────────────────────────────────────

export function AuditTrail() {
  const { auditLogs } = useAdminConfigStore();
  const [actionFilter, setActionFilter] = useState<AuditAction | 'all'>('all');
  const [entityFilter, setEntityFilter] = useState<string>('all');
  const [expandedEntry, setExpandedEntry] = useState<string | null>(null);

  const filtered = auditLogs.filter((log) => {
    if (actionFilter !== 'all' && log.action !== actionFilter) return false;
    if (entityFilter !== 'all' && log.entityType !== entityFilter) return false;
    return true;
  });

  // Stats
  const todayLogs = auditLogs.filter(l => {
    const today = new Date().toISOString().split('T')[0];
    return l.createdAt.startsWith(today);
  }).length;
  const configChanges = auditLogs.filter(l => l.action === 'config_change').length;
  const approvalActions = auditLogs.filter(l => l.action === 'approve' || l.action === 'reject').length;
  const uniqueEntityTypes = [...new Set(auditLogs.map(l => l.entityType))];

  return (
    <div className="space-y-6">
      <SectionHeader
        title="سجل المراجعة"
        subtitle="تتبع كامل لجميع الإجراءات على المنصة — لا يمكن حذف أو تعديل السجلات"
      />

      {/* KPI Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="إجمالي السجلات" value={auditLogs.length} accent="orange" icon={<span className="text-2xl">&#128220;</span>} />
        <StatCard label="اليوم" value={todayLogs} accent="blue" icon={<span className="text-2xl">&#128197;</span>} />
        <StatCard label="تغييرات الإعدادات" value={configChanges} accent="green" icon={<span className="text-2xl">&#9881;</span>} />
        <StatCard label="إجراءات الموافقة" value={approvalActions} accent="orange" icon={<span className="text-2xl">&#10003;</span>} />
      </div>

      {/* Filters */}
      <div className="card p-4">
        <div className="flex flex-wrap gap-4">
          {/* Action Type Filter */}
          <div className="flex flex-wrap gap-1">
            <button
              onClick={() => setActionFilter('all')}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                actionFilter === 'all' ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
              }`}
            >
              كل الإجراءات
            </button>
            {(Object.keys(actionConfig) as AuditAction[]).map((action) => {
              const count = auditLogs.filter(l => l.action === action).length;
              if (count === 0) return null;
              return (
                <button
                  key={action}
                  onClick={() => setActionFilter(action)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                    actionFilter === action ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                  }`}
                >
                  {actionConfig[action].labelAr} ({count})
                </button>
              );
            })}
          </div>

          {/* Entity Type Filter */}
          <select
            value={entityFilter}
            onChange={(e) => setEntityFilter(e.target.value)}
            className="input text-xs py-1.5"
          >
            <option value="all">كل الكيانات</option>
            {uniqueEntityTypes.map((type) => (
              <option key={type} value={type}>
                {entityTypeLabels[type] || type}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Log Entries */}
      <div className="card overflow-hidden">
        <div className="px-4 py-3 bg-earth-50 border-b border-earth-100 flex items-center justify-between">
          <p className="text-sm font-bold text-earth-700">
            السجلات ({filtered.length})
          </p>
          <p className="text-xs text-earth-400">
            مرتبة من الأحدث إلى الأقدم
          </p>
        </div>

        {filtered.length === 0 ? (
          <div className="p-8 text-center text-earth-400 text-sm">
            لا توجد سجلات تطابق الفلتر المحدد
          </div>
        ) : (
          filtered.map((entry) => (
            <LogEntryRow
              key={entry.id}
              entry={entry}
              isExpanded={expandedEntry === entry.id}
              onToggle={() => setExpandedEntry(expandedEntry === entry.id ? null : entry.id)}
            />
          ))
        )}
      </div>

      {/* Security Notice */}
      <div className="card p-4 bg-amber-50 border-amber-100">
        <div className="flex items-start gap-3">
          <span className="text-xl shrink-0">&#128274;</span>
          <div>
            <p className="text-sm font-medium text-amber-800">سياسة سجل المراجعة</p>
            <p className="text-xs text-amber-600 mt-1">
              سجلات المراجعة غير قابلة للتعديل أو الحذف (Immutable Audit Trail).
              يتم تسجيل كل إجراء تلقائياً مع: هوية المستخدم، عنوان IP، القيم السابقة والجديدة.
              يتم الاحتفاظ بالسجلات وفقاً لسياسة الاحتفاظ بالبيانات.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
