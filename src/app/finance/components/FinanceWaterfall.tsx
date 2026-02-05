/**
 * ============================================================
 * AROOBA MARKETPLACE — Finance Waterfall (Pricing Calculator)
 * ============================================================
 */

import React, { useState } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { StatCard, SectionHeader, formatMoney } from '../../shared/components';
import { calculatePrice, type PricingInput } from '../../../lib/pricing-engine';
import { UPLIFT_MATRIX } from '../../../config/constants';

const BUCKET_COLORS = {
  A: '#1fa76d',
  B: '#79daac',
  C: '#ee7711',
  D: '#f6b86d',
};

export function FinanceWaterfall() {
  const [calcInput, setCalcInput] = useState<PricingInput>({
    vendorBasePrice: 500,
    categoryId: 'home-decor-fragile',
    isVendorVatRegistered: true,
    isNonLegalizedVendor: false,
  });

  const result = calculatePrice(calcInput);

  const waterfallData = [
    { name: 'إيراد المورد (A)', value: result.bucketA_vendorRevenue, color: BUCKET_COLORS.A },
    { name: 'ض.ق.م المورد (B)', value: result.bucketB_vendorVat, color: BUCKET_COLORS.B },
    { name: 'إيراد أروبة (C)', value: result.bucketC_aroobaRevenue, color: BUCKET_COLORS.C },
    { name: 'ض.ق.م أروبة (D)', value: result.bucketD_aroobaVat, color: BUCKET_COLORS.D },
  ];

  return (
    <div className="card p-6">
      <SectionHeader
        title="🧮 حاسبة التسعير التفاعلية"
        subtitle="شاهد كيف يتم تقسيم السعر — نموذج الخمس دلو (Waterfall)"
      />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Inputs */}
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-earth-700 mb-1">سعر المورد الأساسي (ج.م)</label>
            <input
              type="number"
              value={calcInput.vendorBasePrice}
              onChange={(e) => setCalcInput({ ...calcInput, vendorBasePrice: Number(e.target.value) || 0 })}
              className="input dir-ltr text-left"
              min={1}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-earth-700 mb-1">فئة المنتج</label>
            <select
              value={calcInput.categoryId}
              onChange={(e) => setCalcInput({ ...calcInput, categoryId: e.target.value as keyof typeof UPLIFT_MATRIX })}
              className="input"
            >
              {Object.entries(UPLIFT_MATRIX).map(([id, config]) => (
                <option key={id} value={id}>
                  {id.replace(/-/g, ' ')} — هامش {(config.default * 100).toFixed(0)}%
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={calcInput.isVendorVatRegistered}
                onChange={(e) => setCalcInput({ ...calcInput, isVendorVatRegistered: e.target.checked })}
                className="w-4 h-4 rounded border-earth-300 text-arooba-500 focus:ring-arooba-500"
              />
              <span className="text-sm text-earth-700">مسجل ضريبياً (ض.ق.م 14%)</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={calcInput.isNonLegalizedVendor}
                onChange={(e) => setCalcInput({ ...calcInput, isNonLegalizedVendor: e.target.checked })}
                className="w-4 h-4 rounded border-earth-300 text-arooba-500 focus:ring-arooba-500"
              />
              <span className="text-sm text-earth-700">مورد غير مسجل (رسوم تعاونية 5%)</span>
            </label>
          </div>

          {/* Price Breakdown */}
          <div className="mt-6 p-4 rounded-xl bg-earth-50 border border-earth-200">
            <div className="text-center mb-4">
              <p className="text-xs text-earth-500">السعر النهائي للعميل</p>
              <p className="text-3xl font-bold text-arooba-600 font-display">{formatMoney(result.finalPrice)}</p>
            </div>

            <div className="space-y-1.5 text-sm">
              <Row label="سعر المورد" value={formatMoney(result.vendorBasePrice)} />
              {result.cooperativeFee > 0 && <Row label="رسوم التعاونية (5%)" value={`+${formatMoney(result.cooperativeFee)}`} color="text-amber-600" />}
              <Row label="هامش أروبة" value={`+${formatMoney(result.marketplaceUplift)}`} color="text-arooba-600" />
              <Row label="رسم لوجستي" value={`+${formatMoney(result.logisticsSurcharge)}`} />
              {result.vendorVat > 0 && <Row label="ض.ق.م المورد (14%)" value={`+${formatMoney(result.vendorVat)}`} color="text-nile-600" />}
              <Row label="ض.ق.م أروبة (14%)" value={`+${formatMoney(result.aroobaVat)}`} color="text-arooba-500" />
              <div className="border-t border-earth-200 pt-2 mt-2 flex justify-between font-bold">
                <span className="text-earth-800">الإجمالي</span>
                <span className="text-arooba-700">{formatMoney(result.finalPrice)}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Chart */}
        <div>
          <p className="text-sm font-semibold text-earth-600 mb-3">توزيع الإيرادات (Waterfall)</p>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={waterfallData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2d2b8" opacity={0.5} />
                <XAxis dataKey="name" tick={{ fontSize: 10, fill: '#714938' }} />
                <YAxis tick={{ fontSize: 11, fill: '#714938' }} />
                <Tooltip
                  formatter={(value: number) => [formatMoney(value), '']}
                  contentStyle={{ borderRadius: '12px', border: '1px solid #e2d2b8', direction: 'rtl' }}
                />
                <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                  {waterfallData.map((entry, i) => (
                    <Cell key={i} fill={entry.color} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Legend */}
          <div className="mt-4 grid grid-cols-2 gap-2">
            {waterfallData.map((b) => (
              <div key={b.name} className="flex items-center gap-2 text-xs">
                <span className="w-3 h-3 rounded-sm shrink-0" style={{ backgroundColor: b.color }} />
                <span className="text-earth-600">{b.name}</span>
                <span className="font-bold text-earth-800 mr-auto">{formatMoney(b.value)}</span>
              </div>
            ))}
          </div>

          {/* Margin highlight */}
          <div className="mt-6 p-4 rounded-xl bg-arooba-50 border border-arooba-200">
            <p className="text-xs font-semibold text-arooba-700 mb-2">📊 هامش أروبة</p>
            <div className="flex items-end gap-3">
              <p className="text-2xl font-bold text-arooba-600">{result.aroobaMarginPercent.toFixed(1)}%</p>
              <p className="text-xs text-earth-500 mb-1">= {formatMoney(result.aroobaGrossMargin)} من كل عملية بيع</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function Row({ label, value, color }: { label: string; value: string; color?: string }) {
  return (
    <div className="flex justify-between">
      <span className="text-earth-600">{label}</span>
      <span className={`font-medium ${color || 'text-earth-800'}`}>{value}</span>
    </div>
  );
}
