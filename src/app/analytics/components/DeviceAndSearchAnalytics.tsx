/**
 * AROOBA MARKETPLACE — Device Breakdown & Top Searches Component
 */

import React from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader } from '../../shared/components';
import { mockAnalyticsSummary } from '../mock-analytics-data';

const DEVICE_COLORS: Record<string, string> = {
  mobile: '#ee7711',
  desktop: '#3b82f6',
  tablet: '#1fa76d',
};

const DEVICE_LABELS: Record<string, { ar: string; en: string }> = {
  mobile: { ar: 'موبايل', en: 'Mobile' },
  desktop: { ar: 'كمبيوتر', en: 'Desktop' },
  tablet: { ar: 'تابلت', en: 'Tablet' },
};

export function DeviceAndSearchAnalytics() {
  const { language } = useAppStore();
  const lang = language as 'ar' | 'en';
  const { deviceBreakdown, topSearches } = mockAnalyticsSummary;

  const pieData = deviceBreakdown.map((d) => ({
    name: DEVICE_LABELS[d.deviceType]?.[lang] || d.deviceType,
    value: d.count,
    color: DEVICE_COLORS[d.deviceType] || '#94a3b8',
  }));

  const maxSearchCount = topSearches.length > 0 ? topSearches[0].count : 1;

  return (
    <div className="card p-5 space-y-6">
      {/* Device Breakdown */}
      <div>
        <SectionHeader
          title={language === 'ar' ? 'توزيع الأجهزة' : 'Device Breakdown'}
          subtitle={language === 'ar' ? 'أنواع الأجهزة المستخدمة' : 'Types of devices used'}
        />
        <div className="flex items-center gap-6">
          <div className="w-32 h-32 shrink-0">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={pieData}
                  cx="50%"
                  cy="50%"
                  innerRadius={30}
                  outerRadius={55}
                  paddingAngle={3}
                  dataKey="value"
                >
                  {pieData.map((entry) => (
                    <Cell key={entry.name} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number) => [value.toLocaleString(), '']}
                  contentStyle={{ borderRadius: '8px', fontSize: '11px' }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
          <div className="flex-1 space-y-2">
            {deviceBreakdown.map((d) => (
              <div key={d.deviceType} className="flex items-center gap-2 text-xs">
                <span
                  className="w-2.5 h-2.5 rounded-full shrink-0"
                  style={{ backgroundColor: DEVICE_COLORS[d.deviceType] }}
                />
                <span className="text-earth-600 flex-1">
                  {DEVICE_LABELS[d.deviceType]?.[lang] || d.deviceType}
                </span>
                <span className="font-medium text-earth-800 dir-ltr">{d.percentage}%</span>
                <span className="text-earth-400 dir-ltr">
                  ({d.count.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')})
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Top Searches */}
      <div>
        <SectionHeader
          title={language === 'ar' ? 'أكثر عمليات البحث' : 'Top Searches'}
          subtitle={language === 'ar' ? 'الكلمات الأكثر بحثا' : 'Most searched terms'}
        />
        <div className="space-y-3">
          {topSearches.slice(0, 6).map((search, index) => (
            <div key={search.query} className="flex items-center gap-3">
              <span className="w-5 text-xs font-bold text-earth-400 text-center shrink-0">
                {index + 1}
              </span>
              <div className="flex-1">
                <div className="flex items-center justify-between mb-1">
                  <span className="text-xs font-medium text-earth-700">{search.query}</span>
                  <span className="text-xs text-earth-400 dir-ltr">{search.count.toLocaleString()}</span>
                </div>
                <div className="w-full h-1.5 bg-earth-100 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-arooba-400 rounded-full transition-all duration-500"
                    style={{ width: `${(search.count / maxSearchCount) * 100}%` }}
                  />
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
