/**
 * ============================================================
 * AROOBA MARKETPLACE — Admin Configuration Store (Zustand)
 * ============================================================
 *
 * Manages dynamic platform configurations that replace hardcoded
 * constants. All configs are admin-editable and stored centrally.
 *
 * In production, this store syncs with the backend
 * PlatformConfiguration table. For the MVP, it initializes
 * from default values that mirror the original constants.
 * ============================================================
 */

import { create } from 'zustand';
import type { PlatformConfig, VendorActionRequest, AuditLogEntry, ConfigCategory } from '../app/shared/types';
import { UPLIFT_MATRIX, UPLIFT_RULES, TAX, ESCROW, VENDOR_SLAS, FRAUD_RULES, KPI_TARGETS, LOYALTY } from '../config/constants';

// ──────────────────────────────────────────────
// RESOLVED CONFIG GETTERS
// ──────────────────────────────────────────────

/** Gets a typed config value from the store, with fallback to default. */
export function getConfigValue(configs: PlatformConfig[], key: string, fallback: string): string {
  const config = configs.find(c => c.key === key && c.isActive);
  return config?.value ?? fallback;
}

export function getConfigNumber(configs: PlatformConfig[], key: string, fallback: number): number {
  const raw = getConfigValue(configs, key, String(fallback));
  const parsed = parseFloat(raw);
  return isNaN(parsed) ? fallback : parsed;
}

export function getConfigBoolean(configs: PlatformConfig[], key: string, fallback: boolean): boolean {
  const raw = getConfigValue(configs, key, String(fallback));
  return raw === 'true';
}

// ──────────────────────────────────────────────
// DEFAULT CONFIGS (seeded from constants.ts)
// ──────────────────────────────────────────────

function buildDefaultConfigs(): PlatformConfig[] {
  const now = new Date().toISOString();
  let id = 0;
  const make = (
    key: string,
    value: string,
    category: ConfigCategory,
    label: string,
    labelAr: string,
    valueType: PlatformConfig['valueType'] = 'number',
    opts: Partial<PlatformConfig> = {},
  ): PlatformConfig => ({
    id: `cfg-${String(++id).padStart(3, '0')}`,
    key,
    value,
    category,
    label,
    labelAr,
    valueType,
    isActive: true,
    requiresApproval: false,
    sortOrder: id,
    updatedAt: now,
    ...opts,
  });

  return [
    // Tax
    make('tax.vatRate', String(TAX.vatRate), 'tax', 'VAT Rate', 'نسبة ض.ق.م', 'percentage', {
      description: 'Egyptian standard VAT rate applied to transactions',
      descriptionAr: 'نسبة ضريبة القيمة المضافة المصرية',
      minValue: 0, maxValue: 1, defaultValue: '0.14', requiresApproval: true,
    }),

    // Uplift Rules
    make('uplift.mvpFlatRate', String(UPLIFT_RULES.mvpFlatRate), 'uplift', 'MVP Flat Uplift Rate', 'نسبة الهامش الثابتة', 'percentage', {
      description: 'Default flat uplift rate for MVP phase',
      descriptionAr: 'نسبة الهامش الثابتة الافتراضية لمرحلة MVP',
      minValue: 0, maxValue: 1, defaultValue: '0.20', requiresApproval: true,
    }),
    make('uplift.fragileOverride', String(UPLIFT_RULES.fragileOverride), 'uplift', 'Fragile Items Override', 'هامش المنتجات الهشة', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.25',
    }),
    make('uplift.minimumFixedUplift', String(UPLIFT_RULES.minimumFixedUplift), 'uplift', 'Minimum Fixed Uplift (EGP)', 'الحد الأدنى للهامش الثابت', 'number', {
      description: 'Minimum uplift in EGP to protect against cheap items',
      descriptionAr: 'الحد الأدنى للهامش بالجنيه لحماية المنتجات الرخيصة',
      minValue: 0, maxValue: 100, defaultValue: '15',
    }),
    make('uplift.lowPriceThreshold', String(UPLIFT_RULES.lowPriceThreshold), 'uplift', 'Low Price Threshold (EGP)', 'عتبة السعر المنخفض', 'number', {
      minValue: 0, maxValue: 500, defaultValue: '100',
    }),
    make('uplift.lowPriceFixedMarkup', String(UPLIFT_RULES.lowPriceFixedMarkup), 'uplift', 'Low Price Fixed Markup (EGP)', 'هامش ثابت للسعر المنخفض', 'number', {
      minValue: 0, maxValue: 200, defaultValue: '20',
    }),
    make('uplift.cooperativeFee', String(UPLIFT_RULES.cooperativeFee), 'uplift', 'Cooperative Fee Rate', 'نسبة رسوم التعاونية', 'percentage', {
      description: 'Fee percentage for non-legalized vendors operating through cooperative',
      descriptionAr: 'نسبة الرسوم للموردين غير المسجلين عبر التعاونية',
      minValue: 0, maxValue: 0.5, defaultValue: '0.05', requiresApproval: true,
    }),
    make('uplift.logisticsSurcharge', String(UPLIFT_RULES.logisticsSurcharge), 'uplift', 'Logistics Surcharge (EGP)', 'رسوم اللوجستيات الإضافية', 'number', {
      minValue: 0, maxValue: 100, defaultValue: '10',
    }),

    // Category-specific uplifts
    ...Object.entries(UPLIFT_MATRIX).map(([catId, config]) =>
      make(`uplift.category.${catId}`, JSON.stringify(config), 'uplift',
        `Category Uplift: ${catId}`, `هامش الفئة: ${catId}`, 'json', {
        description: `Uplift range for ${catId}: min=${config.min}, max=${config.max}, default=${config.default}`,
        defaultValue: JSON.stringify(config),
      })
    ),

    // Escrow
    make('escrow.holdDays', String(ESCROW.holdDays), 'escrow', 'Escrow Hold Period (Days)', 'فترة الضمان (أيام)', 'number', {
      description: 'Days before delivered order funds become available to vendor',
      descriptionAr: 'عدد الأيام قبل إتاحة أموال الطلب المسلم للمورد',
      minValue: 1, maxValue: 60, defaultValue: '14', requiresApproval: true,
    }),
    make('escrow.minimumPayoutThreshold', String(ESCROW.minimumPayoutThreshold), 'escrow', 'Minimum Payout (EGP)', 'الحد الأدنى للسحب', 'number', {
      minValue: 0, maxValue: 5000, defaultValue: '500',
    }),
    make('escrow.codDepositCycle', String(ESCROW.codDepositCycle), 'escrow', 'COD Deposit Cycle (Hours)', 'دورة إيداع الدفع عند الاستلام', 'number', {
      minValue: 12, maxValue: 168, defaultValue: '48',
    }),
    make('escrow.codDiscrepancyThreshold', String(ESCROW.codDiscrepancyThreshold), 'escrow', 'COD Discrepancy Threshold', 'عتبة التناقض في COD', 'percentage', {
      minValue: 0, maxValue: 0.1, defaultValue: '0.01',
    }),

    // Vendor SLAs
    make('sla.acceptanceWindowHours', String(VENDOR_SLAS.acceptanceWindowHours), 'vendor_sla', 'Order Acceptance Window (Hours)', 'نافذة قبول الطلب (ساعات)', 'number', {
      description: 'Hours a vendor has to accept an order before auto-cancel',
      descriptionAr: 'ساعات المهلة لقبول الطلب قبل الإلغاء التلقائي',
      minValue: 1, maxValue: 72, defaultValue: '24',
    }),
    make('sla.maxReliabilityStrikes', String(VENDOR_SLAS.maxReliabilityStrikes), 'vendor_sla', 'Max Reliability Strikes', 'الحد الأقصى لإنذارات الموثوقية', 'number', {
      minValue: 1, maxValue: 10, defaultValue: '3',
    }),
    make('sla.wastedTripFee', String(VENDOR_SLAS.wastedTripFee), 'vendor_sla', 'Wasted Trip Fee (EGP)', 'رسوم الرحلة الضائعة', 'number', {
      minValue: 0, maxValue: 100, defaultValue: '20',
    }),
    make('sla.minimumRating', String(VENDOR_SLAS.minimumRating), 'vendor_sla', 'Minimum Vendor Rating', 'الحد الأدنى لتقييم المورد', 'number', {
      minValue: 1, maxValue: 5, defaultValue: '4.0',
    }),
    make('sla.maxReturnRate', String(VENDOR_SLAS.maxReturnRate), 'vendor_sla', 'Max Return Rate', 'الحد الأقصى لمعدل الإرجاع', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.12',
    }),

    // Fraud Prevention
    make('fraud.maxCodCancelsBeforeBlock', String(FRAUD_RULES.maxCodCancelsBeforeBlock), 'fraud_prevention', 'Max COD Cancels Before Block', 'الحد الأقصى لإلغاء COD', 'number', {
      minValue: 1, maxValue: 10, defaultValue: '3',
    }),
    make('fraud.priceDeviationFlag', String(FRAUD_RULES.priceDeviationFlag), 'fraud_prevention', 'Price Deviation Flag Threshold', 'عتبة تنبيه انحراف السعر', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.20', requiresApproval: true,
    }),

    // Loyalty
    make('loyalty.pointsPerEgp', String(LOYALTY.pointsPerEgp), 'loyalty', 'Points Per EGP', 'نقاط لكل جنيه', 'number', {
      minValue: 0, maxValue: 10, defaultValue: '1',
    }),
    make('loyalty.referralGiveAmount', String(LOYALTY.referralGiveAmount), 'loyalty', 'Referral Give Amount (EGP)', 'مبلغ الإحالة (يعطي)', 'number', {
      minValue: 0, maxValue: 500, defaultValue: '50',
    }),
    make('loyalty.referralGetAmount', String(LOYALTY.referralGetAmount), 'loyalty', 'Referral Get Amount (EGP)', 'مبلغ الإحالة (يحصل)', 'number', {
      minValue: 0, maxValue: 500, defaultValue: '50',
    }),

    // KPI Targets
    make('kpi.addToCartRate', String(KPI_TARGETS.addToCartRate), 'kpi_targets', 'Add to Cart Rate Target', 'هدف معدل الإضافة للسلة', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.08',
    }),
    make('kpi.checkoutCompletion', String(KPI_TARGETS.checkoutCompletion), 'kpi_targets', 'Checkout Completion Target', 'هدف إتمام الشراء', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.55',
    }),
    make('kpi.codRatioMax', String(KPI_TARGETS.codRatioMax), 'kpi_targets', 'Max COD Ratio', 'الحد الأقصى لنسبة COD', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.65',
    }),
    make('kpi.orderAcceptanceRate', String(KPI_TARGETS.orderAcceptanceRate), 'kpi_targets', 'Order Acceptance Rate Target', 'هدف معدل قبول الطلبات', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.95',
    }),
    make('kpi.deliveryFirstAttempt', String(KPI_TARGETS.deliveryFirstAttempt), 'kpi_targets', 'First Attempt Delivery Rate', 'معدل التسليم من المحاولة الأولى', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.85',
    }),
    make('kpi.refundRateMax', String(KPI_TARGETS.refundRateMax), 'kpi_targets', 'Max Refund Rate', 'الحد الأقصى لمعدل الاسترداد', 'percentage', {
      minValue: 0, maxValue: 1, defaultValue: '0.12',
    }),
  ];
}

// ──────────────────────────────────────────────
// MOCK VENDOR ACTION REQUESTS
// ──────────────────────────────────────────────

const mockActionRequests: VendorActionRequest[] = [
  {
    id: 'req-001', vendorId: 'v-001', vendorName: 'Hassan Ceramics', vendorNameAr: 'خزفيات حسن',
    actionType: 'product_listing', status: 'pending', entityType: 'Product', entityId: 'p-new-001',
    proposedValues: JSON.stringify({ title: 'Handmade Clay Bowl', titleAr: 'وعاء طين يدوي', sellingPrice: 120, categoryId: 'home-decor-fragile' }),
    justification: 'New artisan product from Fayoum workshop', priority: 2,
    createdBy: 'v-001', createdAt: '2026-02-07T10:00:00Z', updatedAt: '2026-02-07T10:00:00Z',
  },
  {
    id: 'req-002', vendorId: 'v-003', vendorName: 'Nadia Handcraft', vendorNameAr: 'يدوية نادية',
    actionType: 'price_change', status: 'pending', entityType: 'Product', entityId: 'p-003',
    currentValues: JSON.stringify({ sellingPrice: 85 }),
    proposedValues: JSON.stringify({ sellingPrice: 110 }),
    justification: 'Raw material costs increased by 25%', priority: 3,
    createdBy: 'v-003', createdAt: '2026-02-06T14:30:00Z', updatedAt: '2026-02-06T14:30:00Z',
  },
  {
    id: 'req-003', vendorId: 'v-004', vendorName: 'Khan El-Khalili Leather', vendorNameAr: 'جلود خان الخليلي',
    actionType: 'sub_vendor_addition', status: 'pending', entityType: 'SubVendor',
    proposedValues: JSON.stringify({ internalName: 'Ahmed Tanning', internalNameAr: 'أحمد للدباغة', upliftType: 'percentage', upliftValue: 0.10 }),
    justification: 'Expanding supply chain with specialized tanning artisan', priority: 2,
    createdBy: 'v-004', createdAt: '2026-02-05T09:00:00Z', updatedAt: '2026-02-05T09:00:00Z',
  },
  {
    id: 'req-004', vendorId: 'v-002', vendorName: 'Siwa Textiles Co.', vendorNameAr: 'سيوة للمنسوجات',
    actionType: 'bank_details_change', status: 'approved', entityType: 'ParentVendor', entityId: 'v-002',
    currentValues: JSON.stringify({ bankName: 'Banque Misr', bankAccountNumber: '****4321' }),
    proposedValues: JSON.stringify({ bankName: 'CIB', bankAccountNumber: '****8765' }),
    justification: 'Switching to bank with lower transfer fees',
    reviewedBy: 'admin-001', reviewedAt: '2026-02-04T16:00:00Z', reviewNotes: 'Verified new bank details via phone call',
    priority: 3, createdBy: 'v-002', createdAt: '2026-02-03T11:00:00Z', updatedAt: '2026-02-04T16:00:00Z',
  },
  {
    id: 'req-005', vendorId: 'v-005', vendorName: 'Aswan Spices', vendorNameAr: 'بهارات أسوان',
    actionType: 'profile_update', status: 'rejected', entityType: 'ParentVendor', entityId: 'v-005',
    currentValues: JSON.stringify({ businessName: 'Aswan Spices' }),
    proposedValues: JSON.stringify({ businessName: 'Aswan Premium Spices & Herbs' }),
    justification: 'Rebranding for better market positioning',
    reviewedBy: 'admin-001', reviewedAt: '2026-02-02T13:00:00Z', reviewNotes: 'Vendor is still in pending status. Complete registration first.',
    priority: 1, createdBy: 'v-005', createdAt: '2026-02-01T08:00:00Z', updatedAt: '2026-02-02T13:00:00Z',
  },
  {
    id: 'req-006', vendorId: 'v-001', vendorName: 'Hassan Ceramics', vendorNameAr: 'خزفيات حسن',
    actionType: 'bulk_stock_update', status: 'pending', entityType: 'Product',
    proposedValues: JSON.stringify({ products: [{ id: 'p-001', quantityAvailable: 50 }] }),
    justification: 'Restocking after winter season', priority: 1,
    createdBy: 'v-001', createdAt: '2026-02-07T12:00:00Z', updatedAt: '2026-02-07T12:00:00Z',
  },
];

// ──────────────────────────────────────────────
// MOCK AUDIT LOG
// ──────────────────────────────────────────────

const mockAuditLogs: AuditLogEntry[] = [
  {
    id: 'log-001', userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
    action: 'config_change', entityType: 'PlatformConfig', entityId: 'cfg-001',
    description: 'Updated VAT rate from 0.14 to 0.14 (verified)',
    descriptionAr: 'تحديث نسبة ض.ق.م من 0.14 إلى 0.14 (تم التحقق)',
    oldValues: JSON.stringify({ vatRate: 0.14 }), newValues: JSON.stringify({ vatRate: 0.14 }),
    ipAddress: '192.168.1.100', createdAt: '2026-02-07T14:00:00Z',
  },
  {
    id: 'log-002', userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
    action: 'approve', entityType: 'VendorActionRequest', entityId: 'req-004',
    description: 'Approved bank details change for Siwa Textiles Co.',
    descriptionAr: 'تمت الموافقة على تغيير بيانات البنك لسيوة للمنسوجات',
    ipAddress: '192.168.1.100', vendorActionRequestId: 'req-004', createdAt: '2026-02-04T16:00:00Z',
  },
  {
    id: 'log-003', userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
    action: 'reject', entityType: 'VendorActionRequest', entityId: 'req-005',
    description: 'Rejected profile update for Aswan Spices - pending registration',
    descriptionAr: 'رفض تحديث الملف الشخصي لبهارات أسوان - تسجيل معلق',
    ipAddress: '192.168.1.100', vendorActionRequestId: 'req-005', createdAt: '2026-02-02T13:00:00Z',
  },
  {
    id: 'log-004', userId: 'v-001', userName: 'خزفيات حسن', userRole: 'parent_vendor',
    action: 'create', entityType: 'Product', entityId: 'p-001',
    description: 'Created product: Hand-Painted Ceramic Vase',
    descriptionAr: 'إنشاء منتج: فازة سيراميك مرسومة يدوياً',
    ipAddress: '10.0.0.50', createdAt: '2025-10-25T10:00:00Z',
  },
  {
    id: 'log-005', userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
    action: 'status_change', entityType: 'Product', entityId: 'p-001',
    description: 'Approved product: Hand-Painted Ceramic Vase (Draft → Active)',
    descriptionAr: 'تمت الموافقة على المنتج: فازة سيراميك مرسومة يدوياً',
    oldValues: JSON.stringify({ status: 'pending_review' }), newValues: JSON.stringify({ status: 'active' }),
    ipAddress: '192.168.1.100', createdAt: '2025-10-26T09:00:00Z',
  },
  {
    id: 'log-006', userId: 'v-003', userName: 'يدوية نادية', userRole: 'parent_vendor',
    action: 'update', entityType: 'Product', entityId: 'p-003',
    description: 'Updated stock quantity for Handmade Crochet Scarf',
    descriptionAr: 'تحديث كمية المخزون لوشاح كروشيه يدوي',
    oldValues: JSON.stringify({ quantityAvailable: 30 }), newValues: JSON.stringify({ quantityAvailable: 50 }),
    ipAddress: '10.0.0.75', createdAt: '2025-12-02T10:00:00Z',
  },
  {
    id: 'log-007', userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
    action: 'status_change', entityType: 'Vendor', entityId: 'v-003',
    description: 'Activated vendor: Nadia Handcraft (Pending → Active)',
    descriptionAr: 'تفعيل مورد: يدوية نادية',
    oldValues: JSON.stringify({ status: 'pending' }), newValues: JSON.stringify({ status: 'active' }),
    ipAddress: '192.168.1.100', createdAt: '2025-11-02T08:00:00Z',
  },
  {
    id: 'log-008', userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
    action: 'config_change', entityType: 'PlatformConfig', entityId: 'cfg-002',
    description: 'Updated MVP flat uplift rate from 0.18 to 0.20',
    descriptionAr: 'تحديث نسبة الهامش الثابتة من 0.18 إلى 0.20',
    oldValues: JSON.stringify({ mvpFlatRate: 0.18 }), newValues: JSON.stringify({ mvpFlatRate: 0.20 }),
    ipAddress: '192.168.1.100', createdAt: '2025-10-15T14:00:00Z',
  },
];

// ──────────────────────────────────────────────
// STORE DEFINITION
// ──────────────────────────────────────────────

interface AdminConfigState {
  // Platform configurations
  configs: PlatformConfig[];
  loadConfigs: () => void;
  updateConfig: (id: string, value: string) => void;
  addConfig: (config: PlatformConfig) => void;
  getConfigsByCategory: (category: ConfigCategory) => PlatformConfig[];

  // Vendor action requests
  actionRequests: VendorActionRequest[];
  loadActionRequests: () => void;
  approveRequest: (id: string, notes: string) => void;
  rejectRequest: (id: string, notes: string) => void;
  getPendingCount: () => number;

  // Audit log
  auditLogs: AuditLogEntry[];
  loadAuditLogs: () => void;
  addAuditEntry: (entry: Omit<AuditLogEntry, 'id' | 'createdAt'>) => void;

  // Dynamic pricing config accessors
  getUpliftRules: () => {
    mvpFlatRate: number;
    fragileOverride: number;
    minimumFixedUplift: number;
    lowPriceThreshold: number;
    lowPriceFixedMarkup: number;
    cooperativeFee: number;
    logisticsSurcharge: number;
  };
  getVatRate: () => number;
  getEscrowConfig: () => {
    holdDays: number;
    minimumPayoutThreshold: number;
    codDepositCycle: number;
    codDiscrepancyThreshold: number;
  };
}

export const useAdminConfigStore = create<AdminConfigState>((set, get) => ({
  // ── Configs ──
  configs: buildDefaultConfigs(),

  loadConfigs: () => {
    // In production: fetch from GET /api/adminconfig
    set({ configs: buildDefaultConfigs() });
  },

  updateConfig: (id, value) => {
    set((state) => {
      const updated = state.configs.map(c =>
        c.id === id ? { ...c, value, updatedAt: new Date().toISOString(), lastModifiedBy: 'admin-001' } : c
      );
      // Log the change
      const config = state.configs.find(c => c.id === id);
      if (config) {
        const newLog: AuditLogEntry = {
          id: `log-${Date.now()}`,
          userId: 'admin-001',
          userName: 'كريم مطاط',
          userRole: 'admin_super',
          action: 'config_change',
          entityType: 'PlatformConfig',
          entityId: id,
          description: `Updated ${config.label}: ${config.value} → ${value}`,
          descriptionAr: `تحديث ${config.labelAr}: ${config.value} → ${value}`,
          oldValues: JSON.stringify({ [config.key]: config.value }),
          newValues: JSON.stringify({ [config.key]: value }),
          createdAt: new Date().toISOString(),
        };
        return { configs: updated, auditLogs: [newLog, ...state.auditLogs] };
      }
      return { configs: updated };
    });
  },

  addConfig: (config) => {
    set((state) => ({ configs: [...state.configs, config] }));
  },

  getConfigsByCategory: (category) => {
    return get().configs.filter(c => c.category === category);
  },

  // ── Vendor Action Requests ──
  actionRequests: mockActionRequests,

  loadActionRequests: () => {
    // In production: fetch from GET /api/approvals
    set({ actionRequests: mockActionRequests });
  },

  approveRequest: (id, notes) => {
    set((state) => {
      const now = new Date().toISOString();
      const requests = state.actionRequests.map(r =>
        r.id === id ? {
          ...r, status: 'approved' as const, reviewedBy: 'admin-001',
          reviewedAt: now, reviewNotes: notes, updatedAt: now,
        } : r
      );
      const request = state.actionRequests.find(r => r.id === id);
      const newLog: AuditLogEntry = {
        id: `log-${Date.now()}`,
        userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
        action: 'approve', entityType: 'VendorActionRequest', entityId: id,
        description: `Approved ${request?.actionType} for ${request?.vendorName}`,
        descriptionAr: `تمت الموافقة على ${request?.actionType} لـ ${request?.vendorNameAr}`,
        vendorActionRequestId: id, createdAt: now,
      };
      return { actionRequests: requests, auditLogs: [newLog, ...state.auditLogs] };
    });
  },

  rejectRequest: (id, notes) => {
    set((state) => {
      const now = new Date().toISOString();
      const requests = state.actionRequests.map(r =>
        r.id === id ? {
          ...r, status: 'rejected' as const, reviewedBy: 'admin-001',
          reviewedAt: now, reviewNotes: notes, updatedAt: now,
        } : r
      );
      const request = state.actionRequests.find(r => r.id === id);
      const newLog: AuditLogEntry = {
        id: `log-${Date.now()}`,
        userId: 'admin-001', userName: 'كريم مطاط', userRole: 'admin_super',
        action: 'reject', entityType: 'VendorActionRequest', entityId: id,
        description: `Rejected ${request?.actionType} for ${request?.vendorName}: ${notes}`,
        descriptionAr: `تم رفض ${request?.actionType} لـ ${request?.vendorNameAr}: ${notes}`,
        vendorActionRequestId: id, createdAt: now,
      };
      return { actionRequests: requests, auditLogs: [newLog, ...state.auditLogs] };
    });
  },

  getPendingCount: () => {
    return get().actionRequests.filter(r => r.status === 'pending').length;
  },

  // ── Audit Log ──
  auditLogs: mockAuditLogs,

  loadAuditLogs: () => {
    // In production: fetch from GET /api/auditlogs
    set({ auditLogs: mockAuditLogs });
  },

  addAuditEntry: (entry) => {
    set((state) => ({
      auditLogs: [{
        ...entry,
        id: `log-${Date.now()}`,
        createdAt: new Date().toISOString(),
      }, ...state.auditLogs],
    }));
  },

  // ── Dynamic Config Accessors ──
  getUpliftRules: () => {
    const configs = get().configs;
    return {
      mvpFlatRate: getConfigNumber(configs, 'uplift.mvpFlatRate', UPLIFT_RULES.mvpFlatRate),
      fragileOverride: getConfigNumber(configs, 'uplift.fragileOverride', UPLIFT_RULES.fragileOverride),
      minimumFixedUplift: getConfigNumber(configs, 'uplift.minimumFixedUplift', UPLIFT_RULES.minimumFixedUplift),
      lowPriceThreshold: getConfigNumber(configs, 'uplift.lowPriceThreshold', UPLIFT_RULES.lowPriceThreshold),
      lowPriceFixedMarkup: getConfigNumber(configs, 'uplift.lowPriceFixedMarkup', UPLIFT_RULES.lowPriceFixedMarkup),
      cooperativeFee: getConfigNumber(configs, 'uplift.cooperativeFee', UPLIFT_RULES.cooperativeFee),
      logisticsSurcharge: getConfigNumber(configs, 'uplift.logisticsSurcharge', UPLIFT_RULES.logisticsSurcharge),
    };
  },

  getVatRate: () => {
    return getConfigNumber(get().configs, 'tax.vatRate', TAX.vatRate);
  },

  getEscrowConfig: () => {
    const configs = get().configs;
    return {
      holdDays: getConfigNumber(configs, 'escrow.holdDays', ESCROW.holdDays),
      minimumPayoutThreshold: getConfigNumber(configs, 'escrow.minimumPayoutThreshold', ESCROW.minimumPayoutThreshold),
      codDepositCycle: getConfigNumber(configs, 'escrow.codDepositCycle', ESCROW.codDepositCycle),
      codDiscrepancyThreshold: getConfigNumber(configs, 'escrow.codDiscrepancyThreshold', ESCROW.codDiscrepancyThreshold),
    };
  },
}));
