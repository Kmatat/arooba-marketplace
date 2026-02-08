/**
 * AROOBA MARKETPLACE — Activity Trend Chart Component
 * Shows daily views, cart adds, purchases, and sessions over time.
 */

import React from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader } from '../../shared/components';
import { mockAnalyticsSummary } from '../mock-analytics-data';

export function ActivityTrendChart() {
  const { language } = useAppStore();
  const data = mockAnalyticsSummary.dailyTrend.map((d) => ({
    ...d,
    label: new Date(d.date).toLocaleDateString(language === 'ar' ? 'ar-EG' : 'en-US', {
      month: 'short',
      day: 'numeric',
    }),
  }));

  return (
    <div className="card p-5">
      <SectionHeader
        title={language === 'ar' ? 'اتجاه النشاط اليومي' : 'Daily Activity Trend'}
        subtitle={language === 'ar' ? 'المشاهدات، السلة، والمشتريات على مدار الأيام' : 'Views, cart adds, and purchases over time'}
      />
      <div className="h-72">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data}>
            <defs>
              <linearGradient id="viewsGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.2} />
                <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="cartGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#ee7711" stopOpacity={0.2} />
                <stop offset="95%" stopColor="#ee7711" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="purchaseGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#1fa76d" stopOpacity={0.2} />
                <stop offset="95%" stopColor="#1fa76d" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="#e2d2b8" opacity={0.5} />
            <XAxis dataKey="label" tick={{ fontSize: 11, fill: '#714938' }} />
            <YAxis tick={{ fontSize: 11, fill: '#714938' }} />
            <Tooltip
              contentStyle={{
                borderRadius: '12px',
                border: '1px solid #e2d2b8',
                direction: language === 'ar' ? 'rtl' : 'ltr',
                fontSize: '12px',
              }}
              formatter={(value: number, name: string) => {
                const labels: Record<string, string> = language === 'ar'
                  ? { views: 'مشاهدات', cartAdds: 'إضافة للسلة', purchases: 'مشتريات' }
                  : { views: 'Views', cartAdds: 'Cart Adds', purchases: 'Purchases' };
                return [value.toLocaleString(), labels[name] || name];
              }}
            />
            <Legend
              formatter={(value: string) => {
                const labels: Record<string, string> = language === 'ar'
                  ? { views: 'مشاهدات', cartAdds: 'إضافة للسلة', purchases: 'مشتريات' }
                  : { views: 'Views', cartAdds: 'Cart Adds', purchases: 'Purchases' };
                return labels[value] || value;
              }}
            />
            <Area type="monotone" dataKey="views" stroke="#3b82f6" strokeWidth={2} fill="url(#viewsGrad)" />
            <Area type="monotone" dataKey="cartAdds" stroke="#ee7711" strokeWidth={2} fill="url(#cartGrad)" />
            <Area type="monotone" dataKey="purchases" stroke="#1fa76d" strokeWidth={2} fill="url(#purchaseGrad)" />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
