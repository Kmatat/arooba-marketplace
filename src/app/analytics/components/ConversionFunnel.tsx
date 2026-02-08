/**
 * AROOBA MARKETPLACE — Conversion Funnel Component
 * Visualizes the purchase funnel: Views -> Cart -> Checkout -> Purchase
 */

import React from 'react';
import { useAppStore } from '../../../store/app-store';
import { SectionHeader } from '../../shared/components';
import { mockConversionFunnel } from '../mock-analytics-data';

export function ConversionFunnel() {
  const { language } = useAppStore();
  const funnel = mockConversionFunnel;

  const stages = [
    {
      labelAr: 'مشاهدة المنتج',
      labelEn: 'Product Views',
      count: funnel.productViews,
      rate: 100,
      color: 'bg-blue-500',
      lightColor: 'bg-blue-100',
      textColor: 'text-blue-700',
    },
    {
      labelAr: 'إضافة للسلة',
      labelEn: 'Added to Cart',
      count: funnel.addedToCart,
      rate: funnel.viewToCartRate,
      color: 'bg-arooba-500',
      lightColor: 'bg-arooba-100',
      textColor: 'text-arooba-700',
    },
    {
      labelAr: 'بدء الدفع',
      labelEn: 'Checkout Started',
      count: funnel.checkoutsStarted,
      rate: funnel.cartToCheckoutRate,
      color: 'bg-amber-500',
      lightColor: 'bg-amber-100',
      textColor: 'text-amber-700',
    },
    {
      labelAr: 'إتمام الشراء',
      labelEn: 'Purchase Completed',
      count: funnel.purchasesCompleted,
      rate: funnel.checkoutToCompletionRate,
      color: 'bg-nile-500',
      lightColor: 'bg-nile-100',
      textColor: 'text-nile-700',
    },
  ];

  const maxCount = stages[0].count;

  return (
    <div className="card p-5">
      <SectionHeader
        title={language === 'ar' ? 'قمع التحويل' : 'Conversion Funnel'}
        subtitle={language === 'ar' ? 'رحلة المستخدم من المشاهدة حتى الشراء' : 'User journey from view to purchase'}
      />

      <div className="space-y-4">
        {stages.map((stage, index) => {
          const widthPercent = Math.max(15, (stage.count / maxCount) * 100);
          return (
            <div key={stage.labelEn}>
              <div className="flex items-center justify-between mb-1.5">
                <span className="text-sm font-medium text-earth-700">
                  {language === 'ar' ? stage.labelAr : stage.labelEn}
                </span>
                <div className="flex items-center gap-3">
                  <span className="text-sm font-bold text-earth-900 dir-ltr">
                    {stage.count.toLocaleString(language === 'ar' ? 'ar-EG' : 'en-US')}
                  </span>
                  {index > 0 && (
                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${stage.lightColor} ${stage.textColor}`}>
                      {stage.rate}%
                    </span>
                  )}
                </div>
              </div>
              <div className="w-full h-8 bg-earth-50 rounded-lg overflow-hidden">
                <div
                  className={`h-full ${stage.color} rounded-lg transition-all duration-700 ease-out flex items-center justify-end px-3`}
                  style={{ width: `${widthPercent}%` }}
                >
                  {widthPercent > 25 && (
                    <span className="text-[10px] font-bold text-white">
                      {((stage.count / maxCount) * 100).toFixed(1)}%
                    </span>
                  )}
                </div>
              </div>
              {index < stages.length - 1 && (
                <div className="flex justify-center my-1">
                  <svg width="12" height="12" viewBox="0 0 12 12" className="text-earth-300">
                    <path d="M6 2v8M3 7l3 3 3-3" stroke="currentColor" strokeWidth="1.5" fill="none" strokeLinecap="round" strokeLinejoin="round"/>
                  </svg>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Overall Conversion */}
      <div className="mt-6 pt-4 border-t border-earth-100 flex items-center justify-between">
        <span className="text-sm font-medium text-earth-600">
          {language === 'ar' ? 'معدل التحويل الكلي' : 'Overall Conversion Rate'}
        </span>
        <span className="text-2xl font-bold text-nile-600">{funnel.overallConversionRate}%</span>
      </div>
    </div>
  );
}
