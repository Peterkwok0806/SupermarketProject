// ===== Enums (matching backend) =====

export enum CouponType {
  Percentage = 'Percentage',
  FixedAmount = 'FixedAmount',
  FreeShipping = 'FreeShipping'
}

export enum CouponScope {
  Global = 'Global',
  Product = 'Product',
  Category = 'Category'
}

// ===== Customer-Facing DTOs =====

export interface CouponListDto {
  id: number;
  code: string;
  description?: string;
  type: CouponType;
  discountValue: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  usageLimit?: number;
  usedCount: number;
  usageLimitPerUser?: number;
  scope: CouponScope;
  startDate: string;
  endDate: string;
  isActive: boolean;
  createdAt: string;
  productIds?: number[];
  categoryIds?: number[];
}

export interface ValidateCouponRequestDto {
  code: string;
  orderSubtotal: number;
  cartProductIds?: number[];
  cartCategoryIds?: number[];
}

export interface CouponValidationResultDto {
  isValid: boolean;
  errorMessage?: string;
  couponId?: number;
  code?: string;
  type?: CouponType;
  discountAmount: number;
  description?: string;
}

export interface ApplyCouponRequestDto {
  code: string;
  orderId: number;
}

export interface CouponUsageDto {
  id: number;
  couponCode: string;
  couponDescription?: string;
  couponType: CouponType;
  discountApplied: number;
  usedAt: string;
  orderId: number;
}

// ===== Admin DTOs =====

export interface CouponStatsDto {
  totalCoupons: number;
  activeCoupons: number;
  expiredCoupons: number;
  totalRedemptions: number;
  totalDiscountGiven: number;
}

export interface CreateCouponDto {
  code: string;
  description?: string;
  type: CouponType;
  discountValue: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  usageLimit?: number;
  usageLimitPerUser?: number;
  scope: CouponScope;
  startDate: string;
  endDate: string;
  isActive: boolean;
  productIds?: number[];
  categoryIds?: number[];
}

export interface UpdateCouponDto extends CreateCouponDto {
  id: number;
  isActive: boolean;
}

// ===== Helper =====

export function getCouponTypeLabel(type: CouponType): string {
  switch (type) {
    case CouponType.Percentage: return 'Percentage';
    case CouponType.FixedAmount: return 'Fixed Amount';
    case CouponType.FreeShipping: return 'Free Shipping';
    default: return 'Unknown';
  }
}

export function getCouponScopeLabel(scope: CouponScope): string {
  switch (scope) {
    case CouponScope.Global: return 'All Products';
    case CouponScope.Product: return 'Specific Products';
    case CouponScope.Category: return 'Specific Categories';
    default: return 'Unknown';
  }
}

export function formatDiscountDisplay(type: CouponType, discountValue: number, maximumDiscountAmount?: number): string {
  switch (type) {
    case CouponType.Percentage:
      let text = `${discountValue}% Off`;
      if (maximumDiscountAmount) {
        text += ` (max HK$${maximumDiscountAmount.toFixed(2)})`;
      }
      return text;
    case CouponType.FixedAmount:
      return `HK$${discountValue.toFixed(2)} Off`;
    case CouponType.FreeShipping:
      return 'Free Shipping';
    default:
      return '';
  }
}