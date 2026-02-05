/**
 * AROOBA — Finance Wallets Table Component
 */

import React from 'react';
import { Badge, formatMoney } from '../../shared/components';

const vendorWallets = [
  { vendor: 'خزفيات حسن', total: 45200, pending: 12800, available: 32400, pendingDays: 8 },
  { vendor: 'سيوة للمنسوجات', total: 78500, pending: 28000, available: 50500, pendingDays: 3 },
  { vendor: 'يدوية نادية', total: 8700, pending: 3200, available: 5500, pendingDays: 11 },
  { vendor: 'جلود خان الخليلي', total: 125000, pending: 45000, available: 80000, pendingDays: 6 },
];

export function VendorWalletsTable() {
  return (
    <div className="table-container">
      <table>
        <thead>
          <tr>
            <th>المورد</th>
            <th>الرصيد الكلي</th>
            <th>معلق (ضمان)</th>
            <th>قابل للسحب</th>
            <th>أيام متبقية</th>
            <th>الحالة</th>
          </tr>
        </thead>
        <tbody>
          {vendorWallets.map((wallet) => (
            <tr key={wallet.vendor}>
              <td>
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 rounded-lg bg-arooba-100 text-arooba-600 flex items-center justify-center text-xs font-bold">
                    {wallet.vendor.charAt(0)}
                  </div>
                  <span className="font-medium text-earth-800 text-sm">{wallet.vendor}</span>
                </div>
              </td>
              <td className="font-medium text-earth-800">{formatMoney(wallet.total)}</td>
              <td className="text-amber-600 font-medium">{formatMoney(wallet.pending)}</td>
              <td className="text-nile-600 font-bold">{formatMoney(wallet.available)}</td>
              <td>
                <div className="flex items-center gap-1.5">
                  <div className="w-16 h-1.5 bg-earth-100 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-arooba-500 rounded-full"
                      style={{ width: `${((14 - wallet.pendingDays) / 14) * 100}%` }}
                    />
                  </div>
                  <span className="text-xs text-earth-500">{wallet.pendingDays} يوم</span>
                </div>
              </td>
              <td>
                <Badge variant={wallet.available >= 500 ? 'success' : 'warning'}>
                  {wallet.available >= 500 ? 'جاهز للسحب' : 'أقل من الحد'}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
