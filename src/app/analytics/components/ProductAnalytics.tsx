/**
 * AROOBA MARKETPLACE — Product Analytics Component
 * Shows per-product performance metrics and category breakdown.
 */

import React, { useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
  PieChart, Pie, Cell,
} from 'recharts';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader, Badge } from '../../shared/components';
import { mockProductAnalytics, mockCategoryAnalytics, categoryNames } from '../mock-analytics-data';

type SortKey = 'views' | 'addedToCart' | 'purchases' | 'conversionRate';

export function ProductAnalytics() {
  const { language } = useAppStore();
  const [sortBy, setSortBy] = useState<SortKey>('views');

  const sorted = [...mockProductAnalytics].sort((a, b) => {
    const aVal = a[sortBy as keyof typeof a] as number;
    const bVal = b[sortBy as keyof typeof b] as number;
    return bVal - aVal;
  });

  // Category pie data
  const lang = language as 'ar' | 'en';
  const categoryPieData = mockCategoryAnalytics.map((c) => ({
    name: categoryNames[c.categoryId]?.[lang] || c.categoryId,
    value: c.views,
    color: categoryNames[c.categoryId]?.color || '#94a3b8',
  }));

  // Top 5 products for bar chart
  const barChartData = sorted.slice(0, 5).map((p) => ({
    name: language === 'ar' ? p.productTitleAr : p.productTitle,
    [language === 'ar' ? 'مشاهدات' : 'Views']: p.views,
    [language === 'ar' ? 'سلة' : 'Cart']: p.addedToCart,
    [language === 'ar' ? 'مشتريات' : 'Purchases']: p.purchases,
  }));

  const sortOptions: { key: SortKey; labelAr: string; labelEn: string }[] = [
    { key: 'views', labelAr: 'المشاهدات', labelEn: 'Views' },
    { key: 'addedToCart', labelAr: 'إضافة للسلة', labelEn: 'Cart Adds' },
    { key: 'purchases', labelAr: 'المشتريات', labelEn: 'Purchases' },
    { key: 'conversionRate', labelAr: 'معدل التحويل', labelEn: 'Conversion' },
  ];

  return (
    <div className="space-y-6">
      {/* Top Products Bar Chart & Category Pie */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="card p-5 lg:col-span-2">
          <SectionHeader
            title={language === 'ar' ? 'أداء أعلى المنتجات' : 'Top Product Performance'}
            subtitle={language === 'ar' ? 'مقارنة المشاهدات والسلة والمشتريات' : 'Views, cart, and purchase comparison'}
          />
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={barChartData} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" stroke="#e2d2b8" opacity={0.5} />
                <XAxis type="number" tick={{ fontSize: 11, fill: '#714938' }} />
                <YAxis
                  type="category"
                  dataKey="name"
                  tick={{ fontSize: 10, fill: '#714938' }}
                  width={120}
                />
                <Tooltip
                  contentStyle={{ borderRadius: '12px', border: '1px solid #e2d2b8', direction: language === 'ar' ? 'rtl' : 'ltr', fontSize: '11px' }}
                />
                <Legend />
                <Bar dataKey={language === 'ar' ? 'مشاهدات' : 'Views'} fill="#3b82f6" radius={[0, 4, 4, 0]} />
                <Bar dataKey={language === 'ar' ? 'سلة' : 'Cart'} fill="#ee7711" radius={[0, 4, 4, 0]} />
                <Bar dataKey={language === 'ar' ? 'مشتريات' : 'Purchases'} fill="#1fa76d" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Category Breakdown */}
        <div className="card p-5">
          <SectionHeader
            title={language === 'ar' ? 'تحليل الفئات' : 'Category Analysis'}
            subtitle={language === 'ar' ? 'حسب المشاهدات' : 'By views'}
          />
          <div className="h-40">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={categoryPieData}
                  cx="50%"
                  cy="50%"
                  innerRadius={40}
                  outerRadius={65}
                  paddingAngle={3}
                  dataKey="value"
                >
                  {categoryPieData.map((entry) => (
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
          <div className="space-y-2 mt-2">
            {mockCategoryAnalytics.map((cat) => (
              <div key={cat.categoryId} className="flex items-center gap-2 text-xs">
                <span
                  className="w-2.5 h-2.5 rounded-full shrink-0"
                  style={{ backgroundColor: categoryNames[cat.categoryId]?.color }}
                />
                <span className="text-earth-600 flex-1">
                  {categoryNames[cat.categoryId]?.[lang] || cat.categoryId}
                </span>
                <span className="font-medium text-earth-800 dir-ltr">
                  {cat.views.toLocaleString()}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Product Performance Table */}
      <div className="card p-5">
        <div className="flex items-center justify-between mb-4">
          <SectionHeader
            title={language === 'ar' ? 'تحليل المنتجات التفصيلي' : 'Detailed Product Analysis'}
            subtitle={language === 'ar' ? 'المشاهدات، السلة، الشراء، التحويل، والمنتجات المرتبطة' : 'Views, cart, purchases, conversion, and related products'}
          />
          <div className="flex gap-1">
            {sortOptions.map((opt) => (
              <button
                key={opt.key}
                onClick={() => setSortBy(opt.key)}
                className={`
                  px-3 py-1.5 rounded-lg text-xs font-medium transition-colors
                  ${sortBy === opt.key
                    ? 'bg-arooba-100 text-arooba-700'
                    : 'text-earth-500 hover:bg-earth-100'
                  }
                `}
              >
                {language === 'ar' ? opt.labelAr : opt.labelEn}
              </button>
            ))}
          </div>
        </div>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>{language === 'ar' ? 'المنتج' : 'Product'}</th>
                <th>{language === 'ar' ? 'الفئة' : 'Category'}</th>
                <th>{language === 'ar' ? 'المشاهدات' : 'Views'}</th>
                <th>{language === 'ar' ? 'إضافة للسلة' : 'Cart Adds'}</th>
                <th>{language === 'ar' ? 'المشتريات' : 'Purchases'}</th>
                <th>{language === 'ar' ? 'معدل التحويل' : 'Conv. Rate'}</th>
                <th>{language === 'ar' ? 'نقرات المنتجات المرتبطة' : 'Related Clicks'}</th>
              </tr>
            </thead>
            <tbody>
              {sorted.map((product) => (
                <tr key={product.productId}>
                  <td>
                    <div>
                      <p className="font-medium text-earth-800 text-xs">
                        {language === 'ar' ? product.productTitleAr : product.productTitle}
                      </p>
                    </div>
                  </td>
                  <td>
                    <span
                      className="inline-block px-2 py-0.5 rounded-full text-[10px] font-medium text-white"
                      style={{ backgroundColor: categoryNames[product.categoryId]?.color || '#94a3b8' }}
                    >
                      {categoryNames[product.categoryId]?.[lang] || product.categoryId}
                    </span>
                  </td>
                  <td className="dir-ltr font-medium">{product.views.toLocaleString()}</td>
                  <td className="dir-ltr text-arooba-600 font-medium">{product.addedToCart.toLocaleString()}</td>
                  <td className="dir-ltr text-nile-600 font-medium">{product.purchases.toLocaleString()}</td>
                  <td>
                    <Badge variant={product.conversionRate >= 5 ? 'success' : product.conversionRate >= 4 ? 'info' : 'warning'}>
                      {product.conversionRate}%
                    </Badge>
                  </td>
                  <td className="dir-ltr text-blue-600 font-medium">{product.relatedProductClicks.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
