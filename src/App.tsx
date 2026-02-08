/**
 * ============================================================
 * AROOBA MARKETPLACE — Main Application Entry
 * ============================================================
 * 
 * Routes between all business modules based on sidebar selection.
 * In production, this would use React Router for proper URL routing.
 * For the MVP dashboard, we use Zustand state to switch views.
 * ============================================================
 */

import React from 'react';
import { DashboardLayout } from './layouts/DashboardLayout';
import { useAppStore } from './store/app-store';

// Module imports (lazy-loaded in production)
import { AdminDashboard } from './app/admin/components/AdminDashboard';
import { VendorManagement } from './app/vendors/components/VendorManagement';
import { ProductCatalog } from './app/products/components/ProductCatalog';
import { OrderManagement } from './app/orders/components/OrderManagement';
import { FinanceWaterfall } from './app/finance/components/FinanceWaterfall';
import { VendorWalletsTable } from './app/finance/components/VendorWalletsTable';
import { MonitoringChecklist } from './app/admin/components/MonitoringChecklist';
import { PlatformConfigManager } from './app/admin/components/PlatformConfigManager';
import { VendorApprovalQueue } from './app/admin/components/VendorApprovalQueue';
import { AuditTrail } from './app/admin/components/AuditTrail';
import { UserAnalyticsDashboard } from './app/analytics/components/UserAnalyticsDashboard';
import { CustomerCRM } from './app/customers/components/CustomerCRM';
import { SectionHeader } from './app/shared/components';

/**
 * Module Router — maps section IDs to their components.
 * Each section corresponds to a business module from the spec.
 */
function ModuleRouter() {
  const { activeSection } = useAppStore();

  switch (activeSection) {
    case 'dashboard':
      return <AdminDashboard />;
    case 'user-analytics':
      return <UserAnalyticsDashboard />;
    case 'vendors':
      return <VendorManagement />;
    case 'products':
      return <ProductCatalog />;
    case 'orders':
      return <OrderManagement />;
    case 'finance':
      return (
        <div className="space-y-6">
          <FinanceWaterfall />
          <VendorWalletsTable />
        </div>
      );
    case 'pricing':
      return <FinanceWaterfall />;
    case 'customers':
      return <CustomerCRM />;
    case 'logistics':
      return <LogisticsPlaceholder />;
    case 'monitoring':
      return <MonitoringChecklist />;
    case 'platform-config':
      return <PlatformConfigManager />;
    case 'approvals':
      return <VendorApprovalQueue />;
    case 'audit':
      return <AuditTrail />;
    case 'settings':
      return <SettingsPlaceholder />;
    default:
      return <AdminDashboard />;
  }
}

// ──────────────────────────────────────────────
// PLACEHOLDER MODULES (to be built out)
// ──────────────────────────────────────────────

function LogisticsPlaceholder() {
  return (
    <div className="space-y-6">
      <SectionHeader title="🚚 إدارة اللوجستيات" subtitle="مناطق الشحن وتتبع الشحنات" />
      <div className="card p-8 text-center">
        <span className="text-5xl mb-4 block">🚧</span>
        <p className="text-lg font-bold text-earth-700 mb-2">قيد التطوير</p>
        <p className="text-sm text-earth-500 max-w-md mx-auto">
          هذا القسم سيتضمن إدارة مناطق الشحن، جدول الأسعار،
          متابعة شركات التوصيل، ومطابقة فواتير الشحن.
        </p>
      </div>
    </div>
  );
}

function SettingsPlaceholder() {
  return (
    <div className="space-y-6">
      <SectionHeader title="⚙️ الإعدادات" subtitle="إعدادات المنصة والتكوين" />
      <div className="card p-8 text-center">
        <span className="text-5xl mb-4 block">⚙️</span>
        <p className="text-lg font-bold text-earth-700 mb-2">إعدادات النظام</p>
        <p className="text-sm text-earth-500">إعدادات ض.ق.م، مناطق الشحن، وسياسات الإرجاع</p>
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────
// APP ROOT
// ──────────────────────────────────────────────

export default function App() {
  return (
    <DashboardLayout>
      <ModuleRouter />
    </DashboardLayout>
  );
}
