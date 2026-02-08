/**
 * ============================================================
 * AROOBA MARKETPLACE — Platform Configuration Manager
 * ============================================================
 *
 * Admin UI for managing all dynamic platform configurations.
 * Replaces hardcoded constants with admin-editable values.
 *
 * BUSINESS CONTEXT:
 * Instead of requiring code deployments to change uplift rates,
 * VAT thresholds, or SLA parameters, admins can now modify
 * these values directly from the dashboard. All changes are
 * audit-logged with before/after values.
 *
 * Organized by category tabs matching ConfigCategory.
 * ============================================================
 */

import React, { useState } from 'react';
import { SectionHeader, Badge, StatCard } from '../../shared/components';
import { useAdminConfigStore } from '../../../store/admin-config-store';
import type { ConfigCategory, PlatformConfig } from '../../shared/types';

const CATEGORY_TABS: { id: ConfigCategory; label: string; labelAr: string }[] = [
  { id: 'uplift', label: 'Uplift Rules', labelAr: 'قواعد الهامش' },
  { id: 'tax', label: 'Tax & VAT', labelAr: 'الضرائب وض.ق.م' },
  { id: 'escrow', label: 'Escrow & Payouts', labelAr: 'الضمان والمدفوعات' },
  { id: 'vendor_sla', label: 'Vendor SLAs', labelAr: 'اتفاقيات مستوى الخدمة' },
  { id: 'fraud_prevention', label: 'Fraud Rules', labelAr: 'قواعد مكافحة الاحتيال' },
  { id: 'loyalty', label: 'Loyalty Program', labelAr: 'برنامج الولاء' },
  { id: 'kpi_targets', label: 'KPI Targets', labelAr: 'أهداف الأداء' },
];

function formatDisplayValue(config: PlatformConfig): string {
  if (config.valueType === 'percentage') {
    return `${(parseFloat(config.value) * 100).toFixed(1)}%`;
  }
  if (config.valueType === 'boolean') {
    return config.value === 'true' ? 'Yes' : 'No';
  }
  if (config.valueType === 'json') {
    return 'JSON Object';
  }
  return config.value;
}

function ConfigRow({ config, onSave }: { config: PlatformConfig; onSave: (id: string, value: string) => void }) {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(config.value);
  const [error, setError] = useState('');

  const handleSave = () => {
    // Validate
    if (config.valueType === 'number' || config.valueType === 'percentage') {
      const num = parseFloat(editValue);
      if (isNaN(num)) {
        setError('يجب إدخال رقم صالح');
        return;
      }
      if (config.minValue !== undefined && num < config.minValue) {
        setError(`الحد الأدنى: ${config.minValue}`);
        return;
      }
      if (config.maxValue !== undefined && num > config.maxValue) {
        setError(`الحد الأقصى: ${config.maxValue}`);
        return;
      }
    }
    setError('');
    onSave(config.id, editValue);
    setIsEditing(false);
  };

  const handleCancel = () => {
    setEditValue(config.value);
    setError('');
    setIsEditing(false);
  };

  return (
    <div className="flex items-start gap-4 p-4 border-b border-earth-100 last:border-b-0 hover:bg-earth-50/50 transition-colors">
      {/* Config Info */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <p className="font-medium text-earth-800 text-sm">{config.labelAr}</p>
          {config.requiresApproval && (
            <Badge variant="warning" dot={false}>يتطلب موافقة</Badge>
          )}
        </div>
        <p className="text-xs text-earth-400 dir-ltr">{config.label}</p>
        {config.descriptionAr && (
          <p className="text-xs text-earth-500 mt-1">{config.descriptionAr}</p>
        )}
        <p className="text-[10px] text-earth-300 dir-ltr mt-1">Key: {config.key}</p>
      </div>

      {/* Value Display / Edit */}
      <div className="flex items-center gap-2 shrink-0">
        {isEditing ? (
          <div className="flex flex-col items-end gap-1">
            <div className="flex items-center gap-2">
              {config.valueType === 'boolean' ? (
                <select
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  className="input text-sm py-1 px-2 w-24"
                >
                  <option value="true">Yes</option>
                  <option value="false">No</option>
                </select>
              ) : config.valueType === 'json' ? (
                <textarea
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  className="input text-xs py-1 px-2 w-56 h-20 font-mono dir-ltr"
                />
              ) : (
                <input
                  type="text"
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  className="input text-sm py-1 px-2 w-28 dir-ltr text-center"
                  autoFocus
                />
              )}
              <button onClick={handleSave} className="px-2 py-1 rounded-lg bg-nile-500 text-white text-xs font-medium hover:bg-nile-600 transition-colors">
                حفظ
              </button>
              <button onClick={handleCancel} className="px-2 py-1 rounded-lg bg-earth-200 text-earth-600 text-xs font-medium hover:bg-earth-300 transition-colors">
                إلغاء
              </button>
            </div>
            {error && <p className="text-xs text-red-500">{error}</p>}
            {config.minValue !== undefined && config.maxValue !== undefined && (
              <p className="text-[10px] text-earth-400 dir-ltr">
                Range: {config.minValue} - {config.maxValue}
              </p>
            )}
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <div className="text-left min-w-[80px]">
              <p className="font-bold text-earth-900 text-sm dir-ltr">{formatDisplayValue(config)}</p>
              {config.defaultValue && config.value !== config.defaultValue && (
                <p className="text-[10px] text-amber-600 dir-ltr">Default: {config.defaultValue}</p>
              )}
            </div>
            <button
              onClick={() => setIsEditing(true)}
              className="px-2 py-1 rounded-lg bg-arooba-100 text-arooba-700 text-xs font-medium hover:bg-arooba-200 transition-colors"
            >
              تعديل
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export function PlatformConfigManager() {
  const [activeTab, setActiveTab] = useState<ConfigCategory>('uplift');
  const { configs, updateConfig, getConfigsByCategory } = useAdminConfigStore();

  const categoryConfigs = getConfigsByCategory(activeTab)
    .filter(c => !c.key.startsWith('uplift.category.'))
    .sort((a, b) => a.sortOrder - b.sortOrder);

  const categoryUpliftConfigs = getConfigsByCategory('uplift')
    .filter(c => c.key.startsWith('uplift.category.'))
    .sort((a, b) => a.sortOrder - b.sortOrder);

  const totalConfigs = configs.length;
  const activeConfigs = configs.filter(c => c.isActive).length;
  const approvalRequired = configs.filter(c => c.requiresApproval).length;
  const modifiedFromDefault = configs.filter(c => c.defaultValue && c.value !== c.defaultValue).length;

  return (
    <div className="space-y-6">
      <SectionHeader
        title="إعدادات المنصة"
        subtitle="جميع الإعدادات ديناميكية وقابلة للتعديل من هنا — لا حاجة لتعديل الكود"
      />

      {/* KPI Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="إجمالي الإعدادات" value={totalConfigs} accent="orange" icon={<span className="text-2xl">&#9881;</span>} />
        <StatCard label="إعدادات نشطة" value={activeConfigs} accent="green" icon={<span className="text-2xl">&#10003;</span>} />
        <StatCard label="تتطلب موافقة" value={approvalRequired} accent="blue" icon={<span className="text-2xl">&#128274;</span>} />
        <StatCard label="معدّلة عن الافتراضي" value={modifiedFromDefault} accent="red" icon={<span className="text-2xl">&#9888;</span>} />
      </div>

      {/* Category Tabs */}
      <div className="card p-4">
        <div className="flex flex-wrap gap-2">
          {CATEGORY_TABS.map((tab) => {
            const count = getConfigsByCategory(tab.id).filter(c => !c.key.startsWith('uplift.category.')).length
              + (tab.id === 'uplift' ? categoryUpliftConfigs.length : 0);
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                  activeTab === tab.id
                    ? 'bg-arooba-500 text-white'
                    : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                }`}
              >
                {tab.labelAr} ({count})
              </button>
            );
          })}
        </div>
      </div>

      {/* Config List */}
      <div className="card overflow-hidden">
        <div className="px-4 py-3 bg-earth-50 border-b border-earth-100">
          <p className="text-sm font-bold text-earth-700">
            {CATEGORY_TABS.find(t => t.id === activeTab)?.labelAr}
          </p>
          <p className="text-xs text-earth-500">
            {CATEGORY_TABS.find(t => t.id === activeTab)?.label}
          </p>
        </div>

        {categoryConfigs.length === 0 && activeTab !== 'uplift' ? (
          <div className="p-8 text-center text-earth-400 text-sm">
            لا توجد إعدادات في هذه الفئة
          </div>
        ) : (
          <div>
            {categoryConfigs.map((config) => (
              <ConfigRow key={config.id} config={config} onSave={updateConfig} />
            ))}
          </div>
        )}

        {/* Category Uplift Matrix (special section under uplift tab) */}
        {activeTab === 'uplift' && categoryUpliftConfigs.length > 0 && (
          <>
            <div className="px-4 py-3 bg-arooba-50 border-t border-b border-arooba-100">
              <p className="text-sm font-bold text-arooba-700">مصفوفة الهامش حسب الفئة</p>
              <p className="text-xs text-earth-500 dir-ltr">Category-specific uplift matrix (JSON)</p>
            </div>
            {categoryUpliftConfigs.map((config) => (
              <ConfigRow key={config.id} config={config} onSave={updateConfig} />
            ))}
          </>
        )}
      </div>

      {/* Info Banner */}
      <div className="card p-4 bg-blue-50 border-blue-100">
        <div className="flex items-start gap-3">
          <span className="text-xl shrink-0">&#9432;</span>
          <div>
            <p className="text-sm font-medium text-blue-800">ملاحظة أمان</p>
            <p className="text-xs text-blue-600 mt-1">
              جميع التغييرات يتم تسجيلها تلقائياً في سجل المراجعة.
              الإعدادات المعلّمة بـ "يتطلب موافقة" تحتاج موافقة المدير الأعلى قبل التفعيل.
              القيم الافتراضية محفوظة كشبكة أمان في حالة الحاجة للاستعادة.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
