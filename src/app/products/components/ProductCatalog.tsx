/**
 * ============================================================
 * AROOBA MARKETPLACE — Product Catalog Module
 * ============================================================
 * 
 * Manages the product listing with the 3-step product wizard logic.
 * Shows SKU, pricing breakdown, stock mode, and pickup location.
 * ============================================================
 */

import React, { useState } from 'react';
import { StatCard, Badge, SectionHeader, formatMoney } from '../../shared/components';
import { PRODUCT_CATEGORIES } from '../../../config/constants';
import type { Product } from '../../shared/types';

const mockProducts: Product[] = [
  {
    id: 'p-001', sku: 'HASAN-FATMA-CERAM-001', parentVendorId: 'v-001', subVendorId: 'sv-001',
    categoryId: 'home-decor-fragile', title: 'Hand-Painted Ceramic Vase', titleAr: 'فازة سيراميك مرسومة يدوياً',
    description: '', descriptionAr: 'فخار الفيوم التقليدي، مرسوم يدوياً بزخارف اللوتس.',
    images: [], costPrice: 80, sellingPrice: 100, cooperativeFee: 0, marketplaceUplift: 25,
    finalPrice: 130, pickupLocationId: 'loc-001', stockMode: 'made_to_order', leadTimeDays: 3,
    quantityAvailable: 20, weightKg: 1.2, dimensionL: 20, dimensionW: 20, dimensionH: 35,
    volumetricWeight: 2.8, isLocalOnly: false, status: 'active', isFeatured: true,
    createdAt: '2025-10-25', updatedAt: '2025-12-01',
  },
  {
    id: 'p-002', sku: 'SIWA-MAIN-TEXTI-001', parentVendorId: 'v-002',
    categoryId: 'home-decor-textiles', title: 'Siwa Embroidered Kilim', titleAr: 'سجادة كليم سيوة مطرزة',
    description: '', descriptionAr: 'كليم سيوي أصيل بأنماط أمازيغية تقليدية.',
    images: [], costPrice: 1200, sellingPrice: 1500, cooperativeFee: 0, marketplaceUplift: 300,
    finalPrice: 1800, pickupLocationId: 'loc-002', stockMode: 'ready_stock',
    quantityAvailable: 8, weightKg: 4.5, dimensionL: 120, dimensionW: 80, dimensionH: 15,
    volumetricWeight: 28.8, isLocalOnly: false, status: 'active', isFeatured: true,
    createdAt: '2025-10-28', updatedAt: '2025-12-01',
  },
  {
    id: 'p-003', sku: 'NADIA-MONA-FASHI-001', parentVendorId: 'v-003', subVendorId: 'sv-002',
    categoryId: 'fashion-apparel', title: 'Handmade Crochet Scarf', titleAr: 'وشاح كروشيه يدوي',
    description: '', descriptionAr: 'وشاح كروشيه رقيق بألوان ترابية.',
    images: [], costPrice: 60, sellingPrice: 85, cooperativeFee: 4.25, marketplaceUplift: 20,
    finalPrice: 112, pickupLocationId: 'loc-003', stockMode: 'made_to_order', leadTimeDays: 5,
    quantityAvailable: 50, weightKg: 0.3, dimensionL: 30, dimensionW: 20, dimensionH: 5,
    volumetricWeight: 0.6, isLocalOnly: false, status: 'active', isFeatured: false,
    createdAt: '2025-11-10', updatedAt: '2025-12-02',
  },
  {
    id: 'p-004', sku: 'KHAN-MAIN-LEATH-001', parentVendorId: 'v-004',
    categoryId: 'leather-goods', title: 'Leather Messenger Bag', titleAr: 'حقيبة ماسنجر جلد طبيعي',
    description: '', descriptionAr: 'حقيبة جلد مصنوعة يدوياً من ورشة خان الخليلي.',
    images: [], costPrice: 400, sellingPrice: 550, cooperativeFee: 0, marketplaceUplift: 110,
    finalPrice: 660, pickupLocationId: 'loc-004', stockMode: 'ready_stock',
    quantityAvailable: 15, weightKg: 0.8, dimensionL: 35, dimensionW: 10, dimensionH: 28,
    volumetricWeight: 1.96, isLocalOnly: false, status: 'active', isFeatured: true,
    createdAt: '2025-10-15', updatedAt: '2025-12-03',
  },
  {
    id: 'p-005', sku: 'NADIA-MAIN-BEAUT-001', parentVendorId: 'v-003',
    categoryId: 'beauty-personal', title: 'Natural Olive Oil Soap', titleAr: 'صابون زيت زيتون طبيعي',
    description: '', descriptionAr: 'صابون يدوي بزيت زيتون نقي.',
    images: [], costPrice: 20, sellingPrice: 30, cooperativeFee: 1.5, marketplaceUplift: 20,
    finalPrice: 55, pickupLocationId: 'loc-003', stockMode: 'ready_stock',
    quantityAvailable: 200, weightKg: 0.15, dimensionL: 8, dimensionW: 5, dimensionH: 3,
    volumetricWeight: 0.024, isLocalOnly: false, status: 'active', isFeatured: false,
    createdAt: '2025-11-15', updatedAt: '2025-12-01',
  },
];

const categoryEmoji: Record<string, string> = {
  'home-decor-fragile': '🏺', 'home-decor-textiles': '🧶',
  'fashion-apparel': '👗', 'leather-goods': '👜',
  'beauty-personal': '🧴', 'jewelry-accessories': '💍',
  'furniture-woodwork': '🪑', 'food-essentials': '🍚',
};

export function ProductCatalog() {
  const [view, setView] = useState<'grid' | 'list'>('grid');
  const [catFilter, setCatFilter] = useState<string>('all');

  const filtered = catFilter === 'all' ? mockProducts : mockProducts.filter(p => p.categoryId === catFilter);
  const totalSKUs = mockProducts.length;
  const activeProducts = mockProducts.filter(p => p.status === 'active').length;
  const featuredCount = mockProducts.filter(p => p.isFeatured).length;

  return (
    <div className="space-y-6">
      {/* KPIs */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="إجمالي المنتجات" value={totalSKUs} accent="orange" icon={<span className="text-2xl">📦</span>} />
        <StatCard label="منتجات نشطة" value={activeProducts} accent="green" icon={<span className="text-2xl">✅</span>} />
        <StatCard label="مميزة" value={featuredCount} accent="blue" icon={<span className="text-2xl">⭐</span>} />
        <StatCard label="فئات" value={PRODUCT_CATEGORIES.length} accent="orange" icon={<span className="text-2xl">🏷️</span>} />
      </div>

      {/* Filters */}
      <div className="card p-4">
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex gap-1 flex-wrap">
            <button
              onClick={() => setCatFilter('all')}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                catFilter === 'all' ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
              }`}
            >
              الكل
            </button>
            {PRODUCT_CATEGORIES.map((cat) => (
              <button
                key={cat.id}
                onClick={() => setCatFilter(cat.id)}
                className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                  catFilter === cat.id ? 'bg-arooba-500 text-white' : 'bg-earth-100 text-earth-600 hover:bg-earth-200'
                }`}
              >
                {cat.icon} {cat.nameAr}
              </button>
            ))}
          </div>
          <div className="mr-auto flex gap-1">
            <button onClick={() => setView('grid')} className={`p-2 rounded-lg ${view === 'grid' ? 'bg-arooba-100 text-arooba-600' : 'text-earth-400'}`}>▦</button>
            <button onClick={() => setView('list')} className={`p-2 rounded-lg ${view === 'list' ? 'bg-arooba-100 text-arooba-600' : 'text-earth-400'}`}>☰</button>
          </div>
        </div>
      </div>

      {/* Product Grid */}
      {view === 'grid' ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {filtered.map((product) => (
            <div key={product.id} className="card-hover overflow-hidden animate-fade-in">
              {/* Image Placeholder */}
              <div className="h-40 bg-gradient-to-br from-earth-100 to-earth-200 flex items-center justify-center relative">
                <span className="text-5xl">{categoryEmoji[product.categoryId] || '📦'}</span>
                {product.isFeatured && (
                  <span className="absolute top-3 left-3 bg-arooba-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-full">⭐ مميز</span>
                )}
                <Badge
                  variant={product.stockMode === 'ready_stock' ? 'success' : 'warning'}
                  dot={false}
                >
                  {product.stockMode === 'ready_stock' ? 'جاهز' : `${product.leadTimeDays} أيام`}
                </Badge>
              </div>

              {/* Content */}
              <div className="p-4">
                <p className="font-bold text-earth-800 text-sm mb-1">{product.titleAr}</p>
                <p className="text-xs text-earth-400 font-mono mb-3 dir-ltr">{product.sku}</p>

                {/* Pricing Breakdown */}
                <div className="space-y-1 text-xs mb-3">
                  <div className="flex justify-between text-earth-500">
                    <span>سعر المورد</span>
                    <span>{formatMoney(product.sellingPrice)}</span>
                  </div>
                  {product.cooperativeFee > 0 && (
                    <div className="flex justify-between text-amber-600">
                      <span>رسوم تعاونية</span>
                      <span>+{formatMoney(product.cooperativeFee)}</span>
                    </div>
                  )}
                  <div className="flex justify-between text-arooba-600">
                    <span>هامش أروبة</span>
                    <span>+{formatMoney(product.marketplaceUplift)}</span>
                  </div>
                  <div className="flex justify-between font-bold text-earth-900 border-t border-earth-100 pt-1">
                    <span>السعر النهائي</span>
                    <span className="text-arooba-600">{formatMoney(product.finalPrice)}</span>
                  </div>
                </div>

                {/* Meta */}
                <div className="flex items-center justify-between text-xs text-earth-400">
                  <span>المخزون: {product.quantityAvailable}</span>
                  <span>{product.weightKg} كجم</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        /* List View */
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>المنتج</th>
                <th>SKU</th>
                <th>الفئة</th>
                <th>سعر المورد</th>
                <th>السعر النهائي</th>
                <th>المخزون</th>
                <th>الحالة</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((p) => (
                <tr key={p.id}>
                  <td>
                    <div className="flex items-center gap-2">
                      <span className="text-xl">{categoryEmoji[p.categoryId] || '📦'}</span>
                      <div>
                        <p className="font-medium text-sm text-earth-800">{p.titleAr}</p>
                        <p className="text-xs text-earth-400">{p.descriptionAr?.slice(0, 40)}...</p>
                      </div>
                    </div>
                  </td>
                  <td className="font-mono text-xs dir-ltr text-earth-500">{p.sku}</td>
                  <td>
                    <Badge variant="neutral" dot={false}>
                      {PRODUCT_CATEGORIES.find(c => c.id === p.categoryId)?.nameAr}
                    </Badge>
                  </td>
                  <td className="text-earth-600">{formatMoney(p.sellingPrice)}</td>
                  <td className="font-bold text-arooba-600">{formatMoney(p.finalPrice)}</td>
                  <td>{p.quantityAvailable}</td>
                  <td>
                    <Badge variant={p.stockMode === 'ready_stock' ? 'success' : 'warning'}>
                      {p.stockMode === 'ready_stock' ? 'جاهز' : 'حسب الطلب'}
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
}
