/**
 * ============================================================
 * AROOBA MARKETPLACE — Shared UI Components
 * ============================================================
 * 
 * Reusable components used across all modules.
 * Follows the Arooba design system with Egyptian-inspired palette.
 * ============================================================
 */

import React from 'react';

// ──────────────────────────────────────────────
// STAT CARD — for dashboard KPIs
// ──────────────────────────────────────────────

interface StatCardProps {
  label: string;
  value: string | number;
  trend?: number;        // Positive = growth, negative = decline
  trendLabel?: string;
  icon?: React.ReactNode;
  accent?: 'orange' | 'green' | 'blue' | 'red';
}

export function StatCard({ label, value, trend, trendLabel, icon, accent = 'orange' }: StatCardProps) {
  const accentColors = {
    orange: 'border-l-arooba-500 bg-arooba-50/30',
    green: 'border-l-nile-500 bg-nile-50/30',
    blue: 'border-l-blue-500 bg-blue-50/30',
    red: 'border-l-red-500 bg-red-50/30',
  };

  const iconBg = {
    orange: 'bg-arooba-100 text-arooba-600',
    green: 'bg-nile-100 text-nile-600',
    blue: 'bg-blue-100 text-blue-600',
    red: 'bg-red-100 text-red-600',
  };

  return (
    <div className={`card p-5 border-r-4 ${accentColors[accent]} animate-fade-in`}>
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm text-earth-500 mb-1">{label}</p>
          <p className="text-2xl font-bold text-earth-900 font-display">{value}</p>
          {trend !== undefined && (
            <p className={`text-xs font-medium mt-2 ${trend >= 0 ? 'text-nile-600' : 'text-red-500'}`}>
              {trend >= 0 ? '↑' : '↓'} {Math.abs(trend)}% {trendLabel || ''}
            </p>
          )}
        </div>
        {icon && (
          <div className={`p-3 rounded-xl ${iconBg[accent]}`}>
            {icon}
          </div>
        )}
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────
// STATUS BADGE
// ──────────────────────────────────────────────

type BadgeVariant = 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'pending';

interface BadgeProps {
  variant: BadgeVariant;
  children: React.ReactNode;
  dot?: boolean;
}

export function Badge({ variant, children, dot = true }: BadgeProps) {
  const styles: Record<BadgeVariant, string> = {
    success: 'bg-nile-100 text-nile-700',
    warning: 'bg-amber-100 text-amber-700',
    danger: 'bg-red-100 text-red-700',
    info: 'bg-blue-100 text-blue-700',
    neutral: 'bg-earth-100 text-earth-600',
    pending: 'bg-arooba-100 text-arooba-700',
  };

  const dotColors: Record<BadgeVariant, string> = {
    success: 'bg-nile-500',
    warning: 'bg-amber-500',
    danger: 'bg-red-500',
    info: 'bg-blue-500',
    neutral: 'bg-earth-400',
    pending: 'bg-arooba-500',
  };

  return (
    <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-medium ${styles[variant]}`}>
      {dot && <span className={`w-1.5 h-1.5 rounded-full ${dotColors[variant]}`} />}
      {children}
    </span>
  );
}

// ──────────────────────────────────────────────
// PROGRESS BAR
// ──────────────────────────────────────────────

interface ProgressBarProps {
  value: number;         // 0 to 100
  label?: string;
  color?: 'orange' | 'green' | 'blue' | 'red';
  showValue?: boolean;
}

export function ProgressBar({ value, label, color = 'orange', showValue = true }: ProgressBarProps) {
  const barColors = {
    orange: 'bg-arooba-500',
    green: 'bg-nile-500',
    blue: 'bg-blue-500',
    red: 'bg-red-500',
  };

  return (
    <div className="w-full">
      {(label || showValue) && (
        <div className="flex justify-between text-xs text-earth-500 mb-1.5">
          {label && <span>{label}</span>}
          {showValue && <span className="dir-ltr">{value}%</span>}
        </div>
      )}
      <div className="w-full h-2 bg-earth-100 rounded-full overflow-hidden">
        <div
          className={`h-full rounded-full transition-all duration-500 ${barColors[color]}`}
          style={{ width: `${Math.min(100, Math.max(0, value))}%` }}
        />
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────
// EMPTY STATE
// ──────────────────────────────────────────────

interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      {icon && <div className="text-earth-300 mb-4">{icon}</div>}
      <h3 className="text-lg font-semibold text-earth-700 mb-2">{title}</h3>
      {description && <p className="text-sm text-earth-500 max-w-sm mb-6">{description}</p>}
      {action}
    </div>
  );
}

// ──────────────────────────────────────────────
// DATA TABLE (Reusable)
// ──────────────────────────────────────────────

interface Column<T> {
  key: string;
  header: string;
  render: (item: T) => React.ReactNode;
  width?: string;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyExtractor: (item: T) => string;
  onRowClick?: (item: T) => void;
  emptyMessage?: string;
}

export function DataTable<T>({ columns, data, keyExtractor, onRowClick, emptyMessage }: DataTableProps<T>) {
  if (data.length === 0) {
    return (
      <EmptyState
        title={emptyMessage || 'لا توجد بيانات'}
        description="لم يتم العثور على أي عناصر بعد"
      />
    );
  }

  return (
    <div className="table-container">
      <table>
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={col.key} style={col.width ? { width: col.width } : undefined}>
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((item) => (
            <tr
              key={keyExtractor(item)}
              onClick={() => onRowClick?.(item)}
              className={onRowClick ? 'cursor-pointer' : ''}
            >
              {columns.map((col) => (
                <td key={col.key}>{col.render(item)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ──────────────────────────────────────────────
// SECTION HEADER
// ──────────────────────────────────────────────

interface SectionHeaderProps {
  title: string;
  subtitle?: string;
  action?: React.ReactNode;
}

export function SectionHeader({ title, subtitle, action }: SectionHeaderProps) {
  return (
    <div className="flex items-start justify-between mb-6">
      <div>
        <h2 className="text-xl font-bold text-earth-900 font-display">{title}</h2>
        {subtitle && <p className="text-sm text-earth-500 mt-1">{subtitle}</p>}
      </div>
      {action}
    </div>
  );
}

// ──────────────────────────────────────────────
// MONEY FORMATTER
// ──────────────────────────────────────────────

export function formatMoney(amount: number, compact = false): string {
  if (compact) {
    if (amount >= 1000000) return `${(amount / 1000000).toFixed(1)}M`;
    if (amount >= 1000) return `${(amount / 1000).toFixed(1)}K`;
  }
  return new Intl.NumberFormat('ar-EG', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount) + ' ج.م';
}

export function formatPercent(value: number): string {
  return `${(value * 100).toFixed(1)}%`;
}

export function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('ar-EG', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}
