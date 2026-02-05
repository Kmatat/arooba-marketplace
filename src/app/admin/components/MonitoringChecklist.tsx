/**
 * ============================================================
 * AROOBA MARKETPLACE — Platform Monitoring Checklist
 * ============================================================
 * 
 * Interactive version of the E-Commerce Platform Monitoring
 * & UX Progress Checklist. Tracks all operational KPIs.
 * ============================================================
 */

import React, { useState } from 'react';
import { Badge, ProgressBar, SectionHeader } from '../../shared/components';

interface CheckItem {
  id: string;
  check: string;
  metric: string;
  target: string;
  current?: string;
  status: 'on_target' | 'warning' | 'action_required' | 'not_measured';
}

const CHECKLIST_SECTIONS = [
  {
    title: '1️⃣ تجربة المستخدم (UX)',
    items: [
      { id: 'ux-1', check: 'سرعة تحميل الصفحة الرئيسية', metric: 'متوسط (ثانية)', target: '≤ 2 ثانية', current: '1.8 ثانية', status: 'on_target' as const },
      { id: 'ux-2', check: 'سرعة استجابة البحث', metric: 'متوسط (ثانية)', target: '≤ 1 ثانية', current: '0.9 ثانية', status: 'on_target' as const },
      { id: 'ux-3', check: 'خطوات الدفع', metric: 'عدد الخطوات', target: '≤ 4 خطوات', current: '4 خطوات', status: 'on_target' as const },
      { id: 'ux-4', check: 'وضوح المحتوى العربي', metric: '% منتجات بالعربي', target: '≥ 95%', current: '88%', status: 'warning' as const },
      { id: 'ux-5', check: 'روابط/صور معطلة', metric: 'العدد', target: '0', current: '3', status: 'action_required' as const },
    ],
  },
  {
    title: '2️⃣ أداء المبيعات',
    items: [
      { id: 'sales-1', check: 'GMV اليومي', metric: 'ج.م', target: 'اتجاه صاعد ↑', current: '↑ 23%', status: 'on_target' as const },
      { id: 'sales-2', check: 'متوسط قيمة الطلب', metric: 'ج.م', target: 'مستقر أو ↑', current: '285 ج.م', status: 'on_target' as const },
      { id: 'sales-3', check: 'نسبة العملاء المتكررين', metric: '%', target: '≥ 30%', current: '24%', status: 'warning' as const },
      { id: 'sales-4', check: 'نسبة الإرجاع', metric: '% من الطلبات', target: '≤ 12%', current: '8%', status: 'on_target' as const },
      { id: 'sales-5', check: 'نسبة COD مقابل الرقمي', metric: '%', target: 'COD ≤ 65%', current: '62%', status: 'on_target' as const },
    ],
  },
  {
    title: '3️⃣ تدفق الطلبات',
    items: [
      { id: 'funnel-1', check: 'مشاهدة → سلة', metric: 'معدل التحويل', target: '≥ 8%', current: '9.2%', status: 'on_target' as const },
      { id: 'funnel-2', check: 'سلة → دفع', metric: 'معدل التحويل', target: '≥ 70%', current: '65%', status: 'warning' as const },
      { id: 'funnel-3', check: 'قبول المورد', metric: 'استجابة المورد', target: '≥ 95%', current: '92%', status: 'warning' as const },
      { id: 'funnel-4', check: 'إلغاء الطلبات', metric: 'الإجمالي', target: '≤ 10%', current: '7%', status: 'on_target' as const },
    ],
  },
  {
    title: '4️⃣ أداء التوصيل',
    items: [
      { id: 'del-1', check: 'القاهرة الكبرى', metric: 'SLA', target: '1-2 يوم', current: '1.4 يوم', status: 'on_target' as const },
      { id: 'del-2', check: 'الإسكندرية', metric: 'SLA', target: '1-2 يوم', current: '1.8 يوم', status: 'on_target' as const },
      { id: 'del-3', check: 'الدلتا', metric: 'SLA', target: '2-3 يوم', current: '2.6 يوم', status: 'on_target' as const },
      { id: 'del-4', check: 'صعيد مصر', metric: 'SLA', target: '3-5 يوم', current: '4.1 يوم', status: 'on_target' as const },
    ],
  },
  {
    title: '5️⃣ SLA الموردين',
    items: [
      { id: 'sla-1', check: 'وقت قبول الطلب', metric: 'ساعات', target: '≤ 4 ساعات', current: '3.2 ساعة', status: 'on_target' as const },
      { id: 'sla-2', check: 'وقت التجهيز للشحن', metric: 'ساعات', target: '≤ 24 ساعة', current: '18 ساعة', status: 'on_target' as const },
      { id: 'sla-3', check: 'نجاح التوصيل', metric: '%', target: '≥ 90%', current: '87%', status: 'warning' as const },
      { id: 'sla-4', check: 'تقييم العملاء', metric: 'من 5', target: '≥ 4.0', current: '4.3', status: 'on_target' as const },
    ],
  },
  {
    title: '7️⃣ المالية والتسوية',
    items: [
      { id: 'fin-1', check: 'دقة المحفظة', metric: 'المطابقة', target: '100%', current: '100%', status: 'on_target' as const },
      { id: 'fin-2', check: 'تأخير المدفوعات', metric: 'العدد', target: '0', current: '0', status: 'on_target' as const },
      { id: 'fin-3', check: 'دقة حساب ض.ق.م', metric: 'الدقة', target: '100%', current: '100%', status: 'on_target' as const },
      { id: 'fin-4', check: 'نزاعات العمولة', metric: 'العدد', target: 'منخفض', current: '2', status: 'on_target' as const },
    ],
  },
];

const statusConfig = {
  on_target: { label: '🟢 ضمن الهدف', variant: 'success' as const },
  warning: { label: '🟡 تحذير', variant: 'warning' as const },
  action_required: { label: '🔴 يتطلب إجراء', variant: 'danger' as const },
  not_measured: { label: '⬜ غير مقاس', variant: 'neutral' as const },
};

export function MonitoringChecklist() {
  const [expandedSection, setExpandedSection] = useState<string | null>(CHECKLIST_SECTIONS[0].title);

  // Summary counts
  const allItems = CHECKLIST_SECTIONS.flatMap(s => s.items);
  const onTarget = allItems.filter(i => i.status === 'on_target').length;
  const warnings = allItems.filter(i => i.status === 'warning').length;
  const actions = allItems.filter(i => i.status === 'action_required').length;

  return (
    <div className="space-y-6">
      <SectionHeader
        title="📋 قائمة مراقبة المنصة"
        subtitle="متابعة شاملة لصحة المنصة — محسّنة للسوق المصري"
      />

      {/* Summary */}
      <div className="grid grid-cols-3 gap-4">
        <div className="card p-4 text-center border-r-4 border-r-nile-500">
          <p className="text-2xl font-bold text-nile-600">{onTarget}</p>
          <p className="text-xs text-earth-500">ضمن الهدف 🟢</p>
        </div>
        <div className="card p-4 text-center border-r-4 border-r-amber-500">
          <p className="text-2xl font-bold text-amber-600">{warnings}</p>
          <p className="text-xs text-earth-500">تحذير 🟡</p>
        </div>
        <div className="card p-4 text-center border-r-4 border-r-red-500">
          <p className="text-2xl font-bold text-red-600">{actions}</p>
          <p className="text-xs text-earth-500">يتطلب إجراء 🔴</p>
        </div>
      </div>

      {/* Overall Progress */}
      <div className="card p-5">
        <ProgressBar
          value={Math.round((onTarget / allItems.length) * 100)}
          label="التقدم العام"
          color={onTarget / allItems.length >= 0.8 ? 'green' : 'orange'}
        />
      </div>

      {/* Checklist Sections */}
      <div className="space-y-3">
        {CHECKLIST_SECTIONS.map((section) => {
          const isExpanded = expandedSection === section.title;
          const sectionOnTarget = section.items.filter(i => i.status === 'on_target').length;

          return (
            <div key={section.title} className="card overflow-hidden">
              <button
                onClick={() => setExpandedSection(isExpanded ? null : section.title)}
                className="w-full p-4 flex items-center justify-between hover:bg-earth-50/50 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <span className="font-bold text-earth-800">{section.title}</span>
                  <span className="text-xs text-earth-400">
                    {sectionOnTarget}/{section.items.length} ضمن الهدف
                  </span>
                </div>
                <span className={`text-earth-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`}>▼</span>
              </button>

              {isExpanded && (
                <div className="border-t border-earth-100">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="bg-earth-50">
                        <th className="px-4 py-2 text-start text-xs font-semibold text-earth-500">البند</th>
                        <th className="px-4 py-2 text-start text-xs font-semibold text-earth-500">المقياس</th>
                        <th className="px-4 py-2 text-start text-xs font-semibold text-earth-500">الهدف</th>
                        <th className="px-4 py-2 text-start text-xs font-semibold text-earth-500">الفعلي</th>
                        <th className="px-4 py-2 text-start text-xs font-semibold text-earth-500">الحالة</th>
                      </tr>
                    </thead>
                    <tbody>
                      {section.items.map((item) => (
                        <tr key={item.id} className="border-t border-earth-50">
                          <td className="px-4 py-2.5 font-medium text-earth-700">{item.check}</td>
                          <td className="px-4 py-2.5 text-earth-500 text-xs">{item.metric}</td>
                          <td className="px-4 py-2.5 text-earth-600 text-xs font-medium">{item.target}</td>
                          <td className="px-4 py-2.5 font-bold text-earth-800 text-xs">{item.current || '—'}</td>
                          <td className="px-4 py-2.5">
                            <Badge variant={statusConfig[item.status].variant}>
                              {statusConfig[item.status].label}
                            </Badge>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
