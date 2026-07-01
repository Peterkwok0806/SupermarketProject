using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    /// <summary>
    /// 共享的定價計算邏輯，消除 CartService / OrderService / CouponService 之間的 DRY 違規。
    /// 所有促銷折扣、買 X 送 Y、N 件特價、優惠券折扣的計算都統一在此。
    /// </summary>
    public static class PricingCalculator
    {
        /// <summary>
        /// 計算單一商品在促銷活動下的「折後單價」。
        /// </summary>
        /// <param name="product">商品實體（需包含 Price）</param>
        /// <param name="promo">目前最高權重的促銷活動（可為 null）</param>
        /// <returns>折後單價</returns>
        public static decimal CalculateFinalPrice(Product product, Promotion? promo)
        {
            if (promo == null)
                return product.Price;

            return promo.Type switch
            {
                PromotionType.PercentageOff =>
                    Math.Round(product.Price * (1 - (promo.DiscountValue!.Value / 100)), 2),

                PromotionType.FixedDiscount =>
                    Math.Max(0, product.Price - promo.DiscountValue!.Value),

                // BuyXGetYFree / QuantitySpecialPrice 時，單件基礎價維持原價
                _ => product.Price
            };
        }

        /// <summary>
        /// 計算單一商品在數量與促銷活動下的「項目小計」。
        /// 處理：無活動、百分比折扣、固定折扣、買 X 送 Y、N 件特價。
        /// </summary>
        /// <param name="product">商品實體（需包含 Price）</param>
        /// <param name="promo">目前最高權重的促銷活動（可為 null）</param>
        /// <param name="quantity">購買數量</param>
        /// <returns>項目小計金額</returns>
        public static decimal CalculateItemSubTotal(Product product, Promotion? promo, int quantity)
        {
            decimal basePrice = CalculateFinalPrice(product, promo);

            // 無數量類型活動，直接 = 折後單價 × 數量
            if (promo == null ||
                (promo.Type != PromotionType.BuyXGetYFree &&
                 promo.Type != PromotionType.QuantitySpecialPrice))
            {
                return basePrice * quantity;
            }

            // 買 X 送 Y（例如：買 2 送 1）
            if (promo.Type == PromotionType.BuyXGetYFree)
            {
                int buyQty = promo.BuyQuantity!.Value;
                int freeQty = promo.FreeQuantity!.Value;
                int groupSize = buyQty + freeQty;

                int completedGroups = quantity / groupSize;
                int remainder = quantity % groupSize;

                // 實際要收費的件數 = (每組應付件數 × 組數) + 散件
                int chargeableQuantity = (buyQty * completedGroups) + remainder;
                return basePrice * chargeableQuantity;
            }

            // N 件特價（例如：3 件特價 250 元）
            if (promo.Type == PromotionType.QuantitySpecialPrice)
            {
                int specialQty = promo.BuyQuantity!.Value;
                decimal specialPrice = promo.DiscountValue!.Value;

                int specialGroups = quantity / specialQty;
                int remainder = quantity % specialQty;

                // 總價 = (特價組數 × 特價總額) + (散件 × 基礎單價)
                return (specialGroups * specialPrice) + (remainder * basePrice);
            }

            // 不應到達此處，但安全回退
            return basePrice * quantity;
        }

        /// <summary>
        /// 根據優惠券類型計算折扣金額（含最大折扣上限與不超過訂單總額）。
        /// </summary>
        /// <param name="coupon">已驗證的優惠券</param>
        /// <param name="orderSubtotal">訂單折前總金額</param>
        /// <returns>計算後的折扣金額</returns>
        public static decimal CalculateCouponDiscount(Coupon coupon, decimal orderSubtotal)
        {
            decimal discount = coupon.Type switch
            {
                CouponType.Percentage => orderSubtotal * (coupon.DiscountValue / 100m),
                CouponType.FixedAmount => coupon.DiscountValue,
                CouponType.FreeShipping => 0, // 運費折扣另行處理
                _ => 0
            };

            // 受限於最大折扣金額
            if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount.Value)
                discount = coupon.MaximumDiscountAmount.Value;

            // 折扣金額不得超過訂單總額
            discount = Math.Min(discount, orderSubtotal);

            return Math.Round(discount, 2);
        }
    }
}
