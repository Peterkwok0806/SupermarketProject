using Microsoft.EntityFrameworkCore;
using SupermarketMock.Models;
using SupermarketMock.DTOs;


namespace SupermarketMock.Services
{
    public class CartService : ICartService
    {
        private readonly SupermarketContext _context;

        public CartService(SupermarketContext context)
        {
            _context = context;
        }

        private CartDto MapToDto(Cart cart)
        {
            decimal totalprice = 0;

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                
                CartItems = cart.CartItems.Select(ci => {
                    var activePromotions = ci.Product.ProductPromotions
                        .Select(pp => pp.Promotion)
                        .ToList();

                    decimal subtoal = 0;

                    var primaryPromotion = activePromotions.FirstOrDefault();

                    decimal currentPrice = primaryPromotion == null ? ci.Product.Price : primaryPromotion.Type switch
                    {
                        PromotionType.PercentageOff => Math.Round(ci.Product.Price * (1 - (primaryPromotion.DiscountValue!.Value / 100)), 2),
                        PromotionType.FixedDiscount => Math.Max(0, ci.Product.Price - primaryPromotion.DiscountValue!.Value),
                        _ => ci.Product.Price
                    };

                    if (primaryPromotion == null ||
                    (primaryPromotion.Type != PromotionType.BuyXGetYFree &&
                     primaryPromotion.Type != PromotionType.QuantitySpecialPrice))
                    {
                        subtoal = currentPrice * ci.Quantity;
                    }
                    else if (primaryPromotion.Type == PromotionType.BuyXGetYFree)
                    {
                        // 買 X 送 Y (例如：買 2 送 1)
                        int buyQty = primaryPromotion.BuyQuantity!.Value;
                        int freeQty = primaryPromotion.FreeQuantity!.Value;
                        int groupSize = buyQty + freeQty; // 一組總共 3 件

                        int completedGroups = ci.Quantity / groupSize; // 命中幾組
                        int remainder = ci.Quantity % groupSize;       // 剩下沒滿組的散件

                        // 實際要收費的件數 = (每組應付件數 * 組數) + 散件
                        int chargeableQuantity = (buyQty * completedGroups) + remainder;
                        subtoal = currentPrice * chargeableQuantity;
                    }
                    else if (primaryPromotion.Type == PromotionType.QuantitySpecialPrice)
                    {
                        // N 件特價 (例如：2 件特價 25 元)
                        int specialQty = primaryPromotion.BuyQuantity!.Value;
                        decimal specialPrice = primaryPromotion.DiscountValue!.Value;

                        int specialGroups = ci.Quantity / specialQty; // 命中幾組特價
                        int remainder = ci.Quantity % specialQty;     // 剩下沒湊滿的散件

                        // 總價 = (特價組數 * 特價總額) + (散件 * 基礎單價)
                        subtoal = (specialGroups * specialPrice) + (remainder * currentPrice);
                    }

                    totalprice += subtoal;

                    return new CartItemDto
                    {
                        ProductId = ci.ProductId,
                        UnitPrice = currentPrice,
                        Quantity = ci.Quantity,
                        Subtotal = subtoal,
                        Product = new ProductDto
                        {
                            id = ci.Product.Id,
                            snowflakeId = ci.Product.SnowflakeId.ToString(),
                            name = ci.Product.Name,
                            price = currentPrice,
                            photo = ci.Product.Photo,
                            isOnSale = activePromotions.Any(),
                            originalPrice = activePromotions.Any() ? ci.Product.Price : null,
                            promotionNames = activePromotions.Select(p => p.Name).ToList() // 輸出多個標籤
                        }
                    };
                }).ToList(),

                TotalAmount = Math.Round(totalprice, 2)


            };
        }

        private decimal TotalPrice(Cart cart) 
        {
            decimal finalTotal = 0;
            var now = DateTime.UtcNow;

            foreach (var item in cart.CartItems)
            {
                // 該商品目前最優先 (Priority 最高) 的活動
                var primaryPromotion = item.Product.ProductPromotions
                                        .Select(pp => pp.Promotion)
                                        .FirstOrDefault();

                // 計算這款商品的「基礎折後單價」（處理單品打折、現折）
                decimal basePrice = primaryPromotion == null ? item.Product.Price : primaryPromotion.Type switch
                {
                    PromotionType.PercentageOff =>
                        Math.Round(item.Product.Price * (1 - (primaryPromotion.DiscountValue!.Value / 100)), 2),

                    PromotionType.FixedDiscount =>
                        Math.Max(0, item.Product.Price - primaryPromotion.DiscountValue!.Value),

                    _ => item.Product.Price // 買二送一、兩件特價時，單件基礎價維持原價
                };

                // 如果沒有命中數量類型活動，該品項總價就是：基礎折後價 * 數量
                if (primaryPromotion == null ||
                    (primaryPromotion.Type != PromotionType.BuyXGetYFree &&
                     primaryPromotion.Type != PromotionType.QuantitySpecialPrice))
                {
                    finalTotal += basePrice * item.Quantity;
                    continue;
                }

                if (primaryPromotion.Type == PromotionType.BuyXGetYFree)
                {
                    // 買 X 送 Y (例如：買 2 送 1)
                    int buyQty = primaryPromotion.BuyQuantity!.Value;
                    int freeQty = primaryPromotion.FreeQuantity!.Value;
                    int groupSize = buyQty + freeQty; // 一組總共 3 件

                    int completedGroups = item.Quantity / groupSize; // 命中幾組
                    int remainder = item.Quantity % groupSize;       // 剩下沒滿組的散件

                    // 實際要收費的件數 = (每組應付件數 * 組數) + 散件
                    int chargeableQuantity = (buyQty * completedGroups) + remainder;
                    finalTotal += basePrice * chargeableQuantity;
                }
                else if (primaryPromotion.Type == PromotionType.QuantitySpecialPrice)
                {
                    // N 件特價 (例如：2 件特價 25 元)
                    int specialQty = primaryPromotion.BuyQuantity!.Value;
                    decimal specialPrice = primaryPromotion.DiscountValue!.Value;

                    int specialGroups = item.Quantity / specialQty; // 命中幾組特價
                    int remainder = item.Quantity % specialQty;     // 剩下沒湊滿的散件

                    // 總價 = (特價組數 * 特價總額) + (散件 * 基礎單價)
                    finalTotal += (specialGroups * specialPrice) + (remainder * basePrice);
                }
                
            }
            return Math.Round(finalTotal, 2);

        }

        private async Task<Cart?> GetCartWithPromotionsAsync(int userId)
        {
            var now = DateTime.UtcNow;

            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        // 💡 核心優化：在 SQL 層級直接過濾時間、並依 Priority 降冪排序（最高權重在第一個）
                        .ThenInclude(p => p.ProductPromotions
                            .Where(pp => (pp.OverrideStartDate ?? pp.Promotion.StartDate) <= now
                                      && (pp.OverrideEndDate ?? pp.Promotion.EndDate) >= now)
                            .OrderByDescending(pp => pp.Priority))
                        .ThenInclude(pp => pp.Promotion)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartOperationResult> GetCartByUserIdAsync(int userId)
        {
            var cart = await GetCartWithPromotionsAsync(userId);

            if (cart == null)
            {
                return new CartOperationResult
                {
                    Success = false,
                    Message = "找不到該使用者的購物車"
                };
            }

            return new CartOperationResult
            {
                Success = true,
                Message = "已找到購物車",
                Cart = MapToDto(cart)
            };

        }

        

        public async Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity)
        {
            var cart = await GetCartWithPromotionsAsync(userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // 檢查商品是否存在
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return new CartOperationResult { Success = false, Message = "商品不存在" };

            // 檢查是否已存在該商品
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                int previousQuantity = existingItem.Quantity;
                int targetQuantity = previousQuantity + quantity;

                existingItem.Quantity = targetQuantity;
                existingItem.UpdatedAt = DateTime.UtcNow;

                ApplyCartItemPromotionAndPricing(existingItem, previousQuantity, targetQuantity);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return new CartOperationResult
            {
                Success = true,
                Message = "已加入購物車",
                Cart = MapToDto(cart)
            };
        }

        public async Task<CartOperationResult> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await GetCartWithPromotionsAsync(userId);
            if (cart == null)
                return new CartOperationResult { Success = false, Message = "購物車不存在" };

            // 檢查商品是否存在
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null)
                return new CartOperationResult { Success = false, Message = "購物車中無此商品" };

            // 記錄原本的舊數量，用來判斷使用者是點了 [+] 還是 [-]
            int previousQuantity = item.Quantity;

            if (quantity < 1)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                item.UpdatedAt = DateTime.UtcNow;
            }

            var activepromotion = item.Product.ProductPromotions.Select(pp => pp.Promotion).ToList();
            var BuyXGetYFreeactivepromotion = activepromotion.FirstOrDefault(p => p.Type == PromotionType.BuyXGetYFree);

            if (BuyXGetYFreeactivepromotion!=null)
            {

                int buyQty = BuyXGetYFreeactivepromotion.BuyQuantity!.Value;
                int freeQty = BuyXGetYFreeactivepromotion.FreeQuantity!.Value;
                int groupSize = buyQty + freeQty;

                if ((previousQuantity < quantity) && (quantity == groupSize - 1))
                {
                    if (item.Product.StockQuantity >= groupSize)
                    {
                        item.Quantity = groupSize; // 自動變 3 件
                    }
                }
                else if ((previousQuantity == groupSize) && (quantity == groupSize - 1))
                {
                    item.Quantity = buyQty - 1;
                }
            }

            await _context.SaveChangesAsync();

            return new CartOperationResult
            {
                Success = true,
                Cart = MapToDto(cart)
            };
        }

        public async Task<CartOperationResult> RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await GetCartWithPromotionsAsync(userId);
            if (cart == null)
                return new CartOperationResult { Success = false, Message = "購物車不存在" };

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                cart.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return new CartOperationResult 
            { Success = true, 
              Cart = MapToDto(cart) 
            };
         }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetCartWithPromotionsAsync(userId);
            if (cart != null)
            {
                cart.CartItems.Clear();
                await _context.SaveChangesAsync();
            }
        }

        private void ApplyCartItemPromotionAndPricing(CartItem item, int previousQuantity, int currentQuantity)
        {
            if (item.Product?.ProductPromotions == null) return;

            var activePromotions = item.Product.ProductPromotions.Select(pp => pp.Promotion).ToList();
            var buyXGetYFreePromo = activePromotions.FirstOrDefault(p => p.Type == PromotionType.BuyXGetYFree);

            if (buyXGetYFreePromo != null)
            {
                int buyQty = buyXGetYFreePromo.BuyQuantity!.Value;   // 例如: 2
                int freeQty = buyXGetYFreePromo.FreeQuantity!.Value; // 例如: 1
                int groupSize = buyQty + freeQty;                    // 例如: 3

                // 買二送一自動加碼：使用者原本只有 1 件或沒有，現在加到 2 件時，自動幫他加到 3 件
                if (previousQuantity < currentQuantity && currentQuantity == groupSize - 1)
                {
                    if (item.Product.StockQuantity >= groupSize)
                    {
                        item.Quantity = groupSize; // 自動變 3 件
                    }
                }
            }
        }
    }
}
