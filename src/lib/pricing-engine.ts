/**
 * ============================================================
 * AROOBA MARKETPLACE — Pricing Engine
 * ============================================================
 *
 * This is the MOST critical piece of business logic.
 * It calculates the final price a customer sees, the vendor payout,
 * and the 5-bucket waterfall split for every transaction.
 *
 * BUSINESS CONTEXT (for non-developers):
 *
 * Think of it like a layered cake:
 *
 * Layer 1: Vendor sets their base price (e.g., 500 EGP for a carpet)
 * Layer 2: If vendor is non-legalized, cooperative adds 5% (25 EGP)
 * Layer 3: Arooba adds marketplace uplift (20% = 100 EGP)
 * Layer 4: VAT is calculated on applicable portions
 * Layer 5: Shipping fee is added separately
 *
 * The customer sees ONE final price. Behind the scenes,
 * the system knows exactly who gets what.
 *
 * CHALLENGE:
 * The "Minimum Uplift Rule" ensures we don't lose money on cheap items.
 * A 20 EGP soap at 20% uplift = only 4 EGP commission — that doesn't
 * even cover the bank transfer fee. So we enforce a 15 EGP minimum.
 *
 * DYNAMIC CONFIGURATION:
 * All pricing parameters are now sourced from the admin configuration
 * store. The hardcoded constants serve only as fallback defaults.
 * Admin can modify uplift rates, VAT, cooperative fees, and thresholds
 * without code changes.
 * ============================================================
 */

import { TAX, UPLIFT_RULES, UPLIFT_MATRIX, SHIPPING } from '../config/constants';

// ──────────────────────────────────────────────
// DYNAMIC CONFIG INTERFACE
// ──────────────────────────────────────────────

/**
 * Dynamic pricing configuration sourced from admin store.
 * When provided, these values override the hardcoded constants.
 * When omitted, the engine falls back to constants.ts defaults.
 */
export interface DynamicPricingConfig {
  vatRate: number;
  mvpFlatRate: number;
  minimumFixedUplift: number;
  lowPriceThreshold: number;
  lowPriceFixedMarkup: number;
  cooperativeFee: number;
  logisticsSurcharge: number;
  categoryUplifts?: Record<string, { min: number; max: number; default: number; risk: string }>;
}

/** Returns the default config from hardcoded constants. */
export function getDefaultPricingConfig(): DynamicPricingConfig {
  return {
    vatRate: TAX.vatRate,
    mvpFlatRate: UPLIFT_RULES.mvpFlatRate,
    minimumFixedUplift: UPLIFT_RULES.minimumFixedUplift,
    lowPriceThreshold: UPLIFT_RULES.lowPriceThreshold,
    lowPriceFixedMarkup: UPLIFT_RULES.lowPriceFixedMarkup,
    cooperativeFee: UPLIFT_RULES.cooperativeFee,
    logisticsSurcharge: UPLIFT_RULES.logisticsSurcharge,
    categoryUplifts: { ...UPLIFT_MATRIX },
  };
}

// ──────────────────────────────────────────────
// TYPE DEFINITIONS
// ──────────────────────────────────────────────

export interface PricingInput {
  vendorBasePrice: number;
  categoryId: string;
  isVendorVatRegistered: boolean;
  isNonLegalizedVendor: boolean;
  parentUpliftType?: 'fixed' | 'percentage';
  parentUpliftValue?: number;
  customUpliftOverride?: number;
}

export interface PricingResult {
  // What the customer sees
  finalPrice: number;

  // Breakdown
  vendorBasePrice: number;
  cooperativeFee: number;
  parentVendorUplift: number;
  marketplaceUplift: number;
  logisticsSurcharge: number;

  // VAT
  vendorVat: number;
  aroobaVat: number;

  // The 5-Bucket Waterfall
  bucketA_vendorRevenue: number;
  bucketB_vendorVat: number;
  bucketC_aroobaRevenue: number;
  bucketD_aroobaVat: number;

  // Margins
  aroobaGrossMargin: number;
  aroobaMarginPercent: number;

  // Config source indicator (for transparency in admin UI)
  configSource: 'dynamic' | 'default';
}

export interface ShippingFeeInput {
  actualWeightKg: number;
  dimensionL: number;
  dimensionW: number;
  dimensionH: number;
  fromZoneId: string;
  toZoneId: string;
  baseRate: number;
  perKgRate: number;
}

export interface ShippingFeeResult {
  actualWeight: number;
  volumetricWeight: number;
  chargeableWeight: number;
  baseFee: number;
  excessWeightFee: number;
  totalFee: number;
  subsidizedCustomerFee: number;
  aroobaSubsidy: number;
}

// ──────────────────────────────────────────────
// CORE PRICING CALCULATION
// ──────────────────────────────────────────────

/**
 * Calculates the complete price breakdown for a product.
 * This function implements the entire Additive Uplift Model.
 *
 * Accepts an optional dynamic config parameter. When provided,
 * uses admin-configured values. Falls back to hardcoded constants
 * for backward compatibility.
 *
 * @example
 * // Using default constants
 * const result = calculatePrice({
 *   vendorBasePrice: 300,
 *   categoryId: 'jewelry-accessories',
 *   isVendorVatRegistered: true,
 *   isNonLegalizedVendor: false,
 * });
 *
 * @example
 * // Using dynamic admin config
 * const config = adminStore.getState().getDynamicPricingConfig();
 * const result = calculatePrice(input, config);
 */
export function calculatePrice(input: PricingInput, dynamicConfig?: DynamicPricingConfig): PricingResult {
  const { vendorBasePrice, categoryId, isVendorVatRegistered, isNonLegalizedVendor } = input;

  // Resolve config: dynamic admin values or hardcoded fallback
  const config = dynamicConfig ?? getDefaultPricingConfig();
  const configSource: PricingResult['configSource'] = dynamicConfig ? 'dynamic' : 'default';

  // Step 1: Calculate Cooperative Fee (only for non-legalized vendors)
  const cooperativeFee = isNonLegalizedVendor
    ? vendorBasePrice * config.cooperativeFee
    : 0;

  const priceAfterCoop = vendorBasePrice + cooperativeFee;

  // Step 2: Calculate Parent Vendor Uplift (if sub-vendor product)
  let parentVendorUplift = 0;
  if (input.parentUpliftType && input.parentUpliftValue) {
    parentVendorUplift = input.parentUpliftType === 'fixed'
      ? input.parentUpliftValue
      : priceAfterCoop * input.parentUpliftValue;
  }

  // Step 3: Calculate Marketplace Uplift
  const categoryConfig = config.categoryUplifts?.[categoryId] ??
    (UPLIFT_MATRIX as Record<string, { min: number; max: number; default: number; risk: string }>)[categoryId];
  const upliftRate = input.customUpliftOverride ?? categoryConfig?.default ?? config.mvpFlatRate;

  let marketplaceUplift = priceAfterCoop * upliftRate;

  // Step 3b: Apply Minimum Uplift Rule
  if (marketplaceUplift < config.minimumFixedUplift) {
    marketplaceUplift = config.minimumFixedUplift;
  }

  // Step 3c: For items under the low-price threshold, use fixed markup
  if (vendorBasePrice < config.lowPriceThreshold) {
    marketplaceUplift = Math.max(marketplaceUplift, config.lowPriceFixedMarkup);
  }

  // Step 4: Add Logistics Surcharge (SmartCom Buffer)
  const logisticsSurcharge = config.logisticsSurcharge;

  // Step 5: Calculate Bucket A — Vendor Revenue
  const bucketA = vendorBasePrice + parentVendorUplift;

  // Step 6: Calculate Bucket B — Vendor VAT
  const bucketB = isVendorVatRegistered ? bucketA * config.vatRate : 0;

  // Step 7: Calculate Bucket C — Arooba Revenue
  const bucketC = cooperativeFee + marketplaceUplift + logisticsSurcharge;

  // Step 8: Calculate Bucket D — Arooba VAT (always applies)
  const bucketD = bucketC * config.vatRate;

  // Step 9: Final Price (excluding delivery — that's Bucket E)
  const finalPrice = bucketA + bucketB + bucketC + bucketD;

  // Step 10: Calculate margins
  const aroobaGrossMargin = bucketC; // Before VAT, before costs
  const aroobaMarginPercent = (aroobaGrossMargin / finalPrice) * 100;

  return {
    finalPrice: roundPrice(finalPrice),
    vendorBasePrice,
    cooperativeFee: roundPrice(cooperativeFee),
    parentVendorUplift: roundPrice(parentVendorUplift),
    marketplaceUplift: roundPrice(marketplaceUplift),
    logisticsSurcharge,
    vendorVat: roundPrice(bucketB),
    aroobaVat: roundPrice(bucketD),
    bucketA_vendorRevenue: roundPrice(bucketA),
    bucketB_vendorVat: roundPrice(bucketB),
    bucketC_aroobaRevenue: roundPrice(bucketC),
    bucketD_aroobaVat: roundPrice(bucketD),
    aroobaGrossMargin: roundPrice(aroobaGrossMargin),
    aroobaMarginPercent: roundPrice(aroobaMarginPercent),
    configSource,
  };
}

// ──────────────────────────────────────────────
// SHIPPING FEE CALCULATION
// ──────────────────────────────────────────────

/**
 * Calculates shipping fee using the Zone + Weight model.
 *
 * BUSINESS LOGIC:
 * 1. Calculate volumetric weight: (L x W x H) / 5000
 * 2. Chargeable weight = MAX(actual, volumetric)
 * 3. Fee = Base Rate + (excess weight x per-kg rate)
 * 4. Apply SmartCom Buffer to lower customer-facing fee
 */
export function calculateShippingFee(input: ShippingFeeInput, dynamicConfig?: DynamicPricingConfig): ShippingFeeResult {
  const { actualWeightKg, dimensionL, dimensionW, dimensionH, baseRate, perKgRate } = input;
  const config = dynamicConfig ?? getDefaultPricingConfig();

  // Volumetric weight = (L x W x H) / 5000
  const volumetricWeight = (dimensionL * dimensionW * dimensionH) / SHIPPING.volumetricDivisor;

  // Chargeable = whichever is higher
  const chargeableWeight = Math.max(actualWeightKg, volumetricWeight);

  // Base fee + excess weight charge (first 1kg included in base)
  const excessWeight = Math.max(0, chargeableWeight - 1);
  const excessWeightFee = excessWeight * perKgRate;
  const totalFee = baseRate + excessWeightFee;

  // SmartCom Buffer: subsidize part of the fee
  const subsidizedCustomerFee = Math.max(
    totalFee - config.logisticsSurcharge,
    totalFee * 0.75 // Never subsidize more than 25%
  );
  const aroobaSubsidy = totalFee - subsidizedCustomerFee;

  return {
    actualWeight: actualWeightKg,
    volumetricWeight: roundPrice(volumetricWeight),
    chargeableWeight: roundPrice(chargeableWeight),
    baseFee: baseRate,
    excessWeightFee: roundPrice(excessWeightFee),
    totalFee: roundPrice(totalFee),
    subsidizedCustomerFee: roundPrice(subsidizedCustomerFee),
    aroobaSubsidy: roundPrice(aroobaSubsidy),
  };
}

// ──────────────────────────────────────────────
// ESCROW CALCULATION
// ──────────────────────────────────────────────

/**
 * Determines when funds move from "pending" to "available".
 *
 * BUSINESS RULE:
 * After delivery confirmation, funds sit in escrow for a configurable
 * number of days (default 14). If no return is initiated, they become
 * withdrawable.
 */
export function calculateEscrowRelease(deliveryDate: Date, holdDays = 14): {
  releaseDate: Date;
  daysRemaining: number;
  isReleasable: boolean;
} {
  const releaseDate = new Date(deliveryDate);
  releaseDate.setDate(releaseDate.getDate() + holdDays);

  const now = new Date();
  const daysRemaining = Math.max(
    0,
    Math.ceil((releaseDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
  );

  return {
    releaseDate,
    daysRemaining,
    isReleasable: daysRemaining === 0,
  };
}

// ──────────────────────────────────────────────
// PRICE FLAG CHECK (Marketplace Integrity)
// ──────────────────────────────────────────────

/**
 * Any product priced beyond the configured threshold from market
 * benchmarks is flagged. The threshold is now admin-configurable
 * (default +-20%). This prevents price-gouging and protects customers.
 */
export function checkPriceDeviation(
  productPrice: number,
  categoryAvgPrice: number,
  threshold = 0.20
): { isFlagged: boolean; deviation: number; direction: 'above' | 'below' | 'normal' } {
  const deviation = (productPrice - categoryAvgPrice) / categoryAvgPrice;
  const isFlagged = Math.abs(deviation) > threshold;

  let direction: 'above' | 'below' | 'normal' = 'normal';
  if (deviation > threshold) direction = 'above';
  if (deviation < -threshold) direction = 'below';

  return { isFlagged, deviation: roundPrice(deviation * 100), direction };
}

// ──────────────────────────────────────────────
// HELPERS
// ──────────────────────────────────────────────

function roundPrice(value: number): number {
  return Math.round(value * 100) / 100;
}

/**
 * Round up to nearest 5 EGP for customer-friendly pricing.
 * e.g., 46.5 EGP → 50 EGP
 */
export function roundToFriendlyPrice(price: number): number {
  return Math.ceil(price / 5) * 5;
}
