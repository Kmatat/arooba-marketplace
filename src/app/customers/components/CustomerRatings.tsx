/**
 * ============================================================
 * AROOBA MARKETPLACE — Customer Ratings & Reviews Tab
 * ============================================================
 *
 * Shows all reviews submitted by a customer with ratings,
 * review text, vendor responses, and review status.
 * ============================================================
 */

import React, { useState } from 'react';
import { useAppStore } from '../../../store/app-store';
import { Badge, formatDate } from '../../shared/components';
import { mockCustomerReviews } from '../mock-customer-data';
import type { CustomerCRM, CustomerReview } from '../types';

interface CustomerRatingsProps {
  customer: CustomerCRM;
}

function StarRating({ rating }: { rating: number }) {
  return (
    <span className="inline-flex gap-0.5">
      {[1, 2, 3, 4, 5].map((star) => (
        <span
          key={star}
          className={`text-sm ${star <= rating ? 'text-yellow-500' : 'text-earth-200'}`}
        >
          ★
        </span>
      ))}
    </span>
  );
}

export function CustomerRatings({ customer }: CustomerRatingsProps) {
  const { language } = useAppStore();
  const [filterRating, setFilterRating] = useState<number | null>(null);
  const reviews = mockCustomerReviews[customer.id] || [];

  const filteredReviews = filterRating
    ? reviews.filter(r => r.rating === filterRating)
    : reviews;

  const statusConfig: Record<string, { ar: string; en: string; variant: 'success' | 'pending' | 'danger' | 'warning' }> = {
    published: { ar: 'منشور', en: 'Published', variant: 'success' },
    pending: { ar: 'قيد المراجعة', en: 'Pending', variant: 'pending' },
    flagged: { ar: 'مُبلّغ عنه', en: 'Flagged', variant: 'warning' },
    removed: { ar: 'محذوف', en: 'Removed', variant: 'danger' },
  };

  // Rating distribution
  const ratingDist = [5, 4, 3, 2, 1].map(r => ({
    stars: r,
    count: reviews.filter(rev => rev.rating === r).length,
    pct: reviews.length > 0 ? (reviews.filter(rev => rev.rating === r).length / reviews.length) * 100 : 0,
  }));

  return (
    <div className="space-y-4">
      {/* Rating Overview */}
      <div className="card p-5">
        <div className="flex flex-col sm:flex-row gap-6">
          {/* Average Rating */}
          <div className="text-center sm:text-start sm:w-48 shrink-0">
            <p className="text-5xl font-bold text-earth-900">
              {customer.averageRating > 0 ? customer.averageRating.toFixed(1) : '-'}
            </p>
            <StarRating rating={Math.round(customer.averageRating)} />
            <p className="text-sm text-earth-500 mt-1">
              {customer.totalReviews} {language === 'ar' ? 'تقييم' : 'reviews'}
            </p>
          </div>

          {/* Distribution */}
          <div className="flex-1 space-y-2">
            {ratingDist.map(({ stars, count, pct }) => (
              <div key={stars} className="flex items-center gap-3">
                <button
                  onClick={() => setFilterRating(filterRating === stars ? null : stars)}
                  className={`flex items-center gap-1 text-sm w-16 shrink-0 ${
                    filterRating === stars ? 'font-bold text-arooba-600' : 'text-earth-600'
                  }`}
                >
                  {stars} ★
                </button>
                <div className="flex-1 h-2.5 bg-earth-100 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-yellow-400 rounded-full transition-all"
                    style={{ width: `${pct}%` }}
                  />
                </div>
                <span className="text-xs text-earth-500 w-8 text-left">{count}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Reviews List */}
      {filteredReviews.length === 0 ? (
        <div className="card p-12 text-center">
          <p className="text-earth-500">
            {language === 'ar' ? 'لا توجد تقييمات' : 'No reviews found'}
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {filteredReviews.map((review) => {
            const sc = statusConfig[review.status];
            return (
              <div key={review.id} className="card p-5">
                <div className="flex flex-col sm:flex-row gap-4">
                  {/* Product Info */}
                  <div className="w-12 h-12 rounded-lg bg-earth-100 flex items-center justify-center text-xl shrink-0">
                    📦
                  </div>

                  <div className="flex-1 min-w-0">
                    {/* Header */}
                    <div className="flex items-start justify-between gap-3 flex-wrap">
                      <div>
                        <p className="font-medium text-earth-900">{review.productTitle}</p>
                        <p className="text-xs text-earth-500 mt-0.5">
                          {review.vendorName} | {language === 'ar' ? 'طلب' : 'Order'}: {review.orderNumber}
                        </p>
                      </div>
                      <Badge variant={sc.variant}>{sc[language]}</Badge>
                    </div>

                    {/* Stars & Date */}
                    <div className="flex items-center gap-3 mt-2">
                      <StarRating rating={review.rating} />
                      <span className="text-xs text-earth-400">{formatDate(review.createdAt)}</span>
                      {review.isVerifiedPurchase && (
                        <span className="text-xs text-nile-600 bg-nile-50 px-1.5 py-0.5 rounded">
                          {language === 'ar' ? 'مشتري موثق' : 'Verified Purchase'}
                        </span>
                      )}
                    </div>

                    {/* Review Text */}
                    {review.reviewText && (
                      <p className="text-sm text-earth-700 mt-3 leading-relaxed">
                        "{review.reviewText}"
                      </p>
                    )}

                    {/* Admin Reply */}
                    {review.adminReply && (
                      <div className="mt-3 p-3 rounded-lg bg-blue-50 border border-blue-100">
                        <p className="text-xs font-medium text-blue-700 mb-1">
                          {language === 'ar' ? 'رد الإدارة:' : 'Admin Reply:'}
                        </p>
                        <p className="text-sm text-blue-600">{review.adminReply}</p>
                      </div>
                    )}

                    {/* Footer */}
                    <div className="flex items-center gap-4 mt-3 text-xs text-earth-400">
                      <span>
                        👍 {review.helpfulCount} {language === 'ar' ? 'وجدوا هذا مفيداً' : 'found helpful'}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
