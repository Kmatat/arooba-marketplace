/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer Performance & Loyalty Tab
 * ============================================================
 *
 * RFM scores, spending trends, category breakdown,
 * loyalty tier progress, and referral program stats.
 * ============================================================
 */

import React from 'react';
import { useAppStore } from '../../../store/app-store';
import { ProgressBar, formatMoney } from '../../shared/components';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { mockCustomerPerformance } from '../mock-customer-data';
import type { CustomerCRM, CustomerTier } from '../types';

interface CustomerPerformanceProps {
  customer: CustomerCRM;
}

const CATEGORY_COLORS = ['#ee7711', '#1fa76d', '#3b82f6', '#d79352', '#8b5cf6', '#ef4444'];

export function CustomerPerformanceTab({ customer }: CustomerPerformanceProps) {
  const { language } = useAppStore();
  const perf = mockCustomerPerformance[customer.id];

  if (!perf) {
    return (
      <div className="card p-12 text-center">
        <p className="text-earth-500">
          {language === 'ar' ? 'لا تتوفر بيانات أداء كافية لهذا العميل' : 'Not enough performance data for this customer'}
        </p>
      </div>
    );
  }

  const tierLabels: Record<CustomerTier, { ar: string; en: string }> = {
    bronze: { ar: 'برونزي', en: 'Bronze' },
    silver: { ar: 'فضي', en: 'Silver' },
    gold: { ar: 'ذهبي', en: 'Gold' },
    platinum: { ar: 'بلاتيني', en: 'Platinum' },
  };

  const scoreColor = (score: number): string => {
    if (score >= 80) return 'text-nile-600';
    if (score >= 60) return 'text-arooba-600';
    if (score >= 40) return 'text-amber-600';
    return 'text-red-600';
  };

  const scoreBarColor = (score: number): 'green' | 'orange' | 'red' | 'blue' => {
    if (score >= 80) return 'green';
    if (score >= 60) return 'orange';
    if (score >= 40) return 'orange';
    return 'red';
  };

  return (
    <div className="space-y-4">
      {/* Engagement Score & RFM */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Engagement Score */}
        <div className="card p-5">
          <h3 className="text-sm font-semibold text-earth-700 mb-4">
            {language === 'ar' ? 'درجة التفاعل' : 'Engagement Score'}
          </h3>
          <div className="flex items-center gap-6">
            <div className="relative w-28 h-28 shrink-0">
              <svg className="w-28 h-28 transform -rotate-90" viewBox="0 0 100 100">
                <circle cx="50" cy="50" r="40" fill="none" stroke="#f3f0ec" strokeWidth="8" />
                <circle
                  cx="50" cy="50" r="40" fill="none"
                  stroke={perf.engagementScore >= 80 ? '#1fa76d' : perf.engagementScore >= 60 ? '#ee7711' : '#ef4444'}
                  strokeWidth="8" strokeLinecap="round"
                  strokeDasharray={`${perf.engagementScore * 2.51} 251`}
                />
              </svg>
              <div className="absolute inset-0 flex items-center justify-center">
                <span className={`text-2xl font-bold ${scoreColor(perf.engagementScore)}`}>
                  {perf.engagementScore}
                </span>
              </div>
            </div>
            <div className="flex-1 space-y-3">
              <ProgressBar
                label={language === 'ar' ? 'الحداثة (Recency)' : 'Recency'}
                value={perf.recencyScore}
                color={scoreBarColor(perf.recencyScore)}
              />
              <ProgressBar
                label={language === 'ar' ? 'التكرار (Frequency)' : 'Frequency'}
                value={perf.frequencyScore}
                color={scoreBarColor(perf.frequencyScore)}
              />
              <ProgressBar
                label={language === 'ar' ? 'القيمة (Monetary)' : 'Monetary'}
                value={perf.monetaryScore}
                color={scoreBarColor(perf.monetaryScore)}
              />
            </div>
          </div>
        </div>

        {/* Tier Progress */}
        <div className="card p-5">
          <h3 className="text-sm font-semibold text-earth-700 mb-4">
            {language === 'ar' ? 'تقدم مستوى الولاء' : 'Loyalty Tier Progress'}
          </h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between text-sm">
              <span className="text-earth-600">
                {language === 'ar' ? 'المستوى الحالي:' : 'Current Tier:'}{' '}
                <span className="font-bold text-earth-900">{tierLabels[customer.tier][language]}</span>
              </span>
              {perf.pointsToNextTier > 0 && (
                <span className="text-earth-500">
                  {language === 'ar' ? 'التالي:' : 'Next:'}{' '}
                  <span className="font-medium">{tierLabels[perf.nextTier][language]}</span>
                </span>
              )}
            </div>
            <ProgressBar
              value={perf.tierProgress}
              color={perf.tierProgress === 100 ? 'green' : 'orange'}
            />
            {perf.pointsToNextTier > 0 && (
              <p className="text-xs text-earth-500">
                {language === 'ar'
                  ? `${perf.pointsToNextTier.toLocaleString()} نقطة متبقية للترقية`
                  : `${perf.pointsToNextTier.toLocaleString()} points to upgrade`
                }
              </p>
            )}

            <div className="grid grid-cols-3 gap-3 pt-3 border-t border-earth-100">
              <div className="text-center">
                <p className="text-xs text-earth-500">{language === 'ar' ? 'نقاط مكتسبة' : 'Earned'}</p>
                <p className="text-lg font-bold text-nile-600">{perf.pointsEarned.toLocaleString()}</p>
              </div>
              <div className="text-center">
                <p className="text-xs text-earth-500">{language === 'ar' ? 'نقاط مستبدلة' : 'Redeemed'}</p>
                <p className="text-lg font-bold text-arooba-600">{perf.pointsRedeemed.toLocaleString()}</p>
              </div>
              <div className="text-center">
                <p className="text-xs text-earth-500">{language === 'ar' ? 'الرصيد' : 'Balance'}</p>
                <p className="text-lg font-bold text-earth-900">{perf.pointsBalance.toLocaleString()}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Monthly Spending Chart & Category Breakdown */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Monthly Spending */}
        <div className="card p-5">
          <h3 className="text-sm font-semibold text-earth-700 mb-4">
            {language === 'ar' ? 'الإنفاق الشهري' : 'Monthly Spending'}
          </h3>
          <div className="h-52">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={perf.monthlySpending}>
                <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip
                  formatter={(value: number) => [formatMoney(value), language === 'ar' ? 'الإنفاق' : 'Spending']}
                  contentStyle={{ fontSize: 12, borderRadius: 8 }}
                />
                <Bar dataKey="amount" fill="#ee7711" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Category Breakdown */}
        <div className="card p-5">
          <h3 className="text-sm font-semibold text-earth-700 mb-4">
            {language === 'ar' ? 'توزيع الفئات' : 'Category Breakdown'}
          </h3>
          <div className="flex items-center gap-6">
            <div className="w-40 h-40 shrink-0">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={perf.categoryBreakdown}
                    dataKey="amount"
                    nameKey="category"
                    cx="50%"
                    cy="50%"
                    innerRadius={30}
                    outerRadius={60}
                    paddingAngle={2}
                  >
                    {perf.categoryBreakdown.map((_, idx) => (
                      <Cell key={idx} fill={CATEGORY_COLORS[idx % CATEGORY_COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip
                    formatter={(value: number) => formatMoney(value)}
                    contentStyle={{ fontSize: 12, borderRadius: 8 }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <div className="flex-1 space-y-2">
              {perf.categoryBreakdown.map((cat, idx) => (
                <div key={cat.category} className="flex items-center gap-2 text-sm">
                  <span
                    className="w-3 h-3 rounded-full shrink-0"
                    style={{ backgroundColor: CATEGORY_COLORS[idx % CATEGORY_COLORS.length] }}
                  />
                  <span className="flex-1 text-earth-600 truncate">{cat.category}</span>
                  <span className="font-medium text-earth-900">{cat.percentage}%</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Referral & Return Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="card p-4">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'إحالات مُرسلة' : 'Referrals Sent'}</p>
          <p className="text-2xl font-bold text-earth-900">{perf.referralsSent}</p>
          <p className="text-xs text-nile-600 mt-1">
            {perf.referralsConverted} {language === 'ar' ? 'ناجحة' : 'converted'}
          </p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'أرباح الإحالة' : 'Referral Earnings'}</p>
          <p className="text-2xl font-bold text-nile-600">{formatMoney(perf.referralEarnings)}</p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'معدل الإرجاع' : 'Return Rate'}</p>
          <p className="text-2xl font-bold text-earth-900">{perf.returnRate}%</p>
          <p className="text-xs text-earth-400 mt-1">{perf.totalReturns} {language === 'ar' ? 'مرتجعات' : 'returns'}</p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-earth-500 mb-1">{language === 'ar' ? 'متوسط حجم السلة' : 'Avg Cart Size'}</p>
          <p className="text-2xl font-bold text-earth-900">{perf.averageCartSize}</p>
          <p className="text-xs text-earth-400 mt-1">
            {language === 'ar' ? 'معدل التخلي:' : 'Abandon:'} {perf.cartAbandonmentRate}%
          </p>
        </div>
      </div>
    </div>
  );
}
