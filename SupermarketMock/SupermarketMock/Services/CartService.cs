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

                    var primaryPromotion = activePromotions.FirstOrDefault();

                    decimal currentPrice = PricingCalculator.CalculateFinalPrice(ci.Product, primaryPromotion);
                    decimal subtoal = PricingCalculator.CalculateItemSubTotal(ci.Product, primaryPromotion, ci.Quantity);

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
                            promotionNames = activePromotions.Select(p => p.Name).ToList()
                        }
                    };
                }).ToList(),

                TotalAmount = Math.Round(totalprice, 2)
            };
        }

        private decimal TotalPrice(Cart cart) 
        {
            decimal finalTotal = 0;

            foreach (var item in cart.CartItems)
            {
                var primaryPromotion = item.Product.ProductPromotions
                                        .Select(pp => pp.Promotion)
                                        .FirstOrDefault();

                finalTotal += PricingCalculator.CalculateItemSubTotal(
                    item.Product, primaryPromotion, item.Quantity);
            }
            return Math.Round(finalTotal, 2);
        }

        private async Task<Cart?> GetCartWithPromotionsAsync(int userId)
        {
            var now = DateTime.UtcNow;

            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        //SQL 層級直接過濾時間、並依 Priority 降冪排序（最高權重在第一個）
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
                bool addingMeetGroupSize = (quantity + 1) % groupSize == 0;
                bool subMeetGroupSize = previousQuantity % groupSize == 0;

                if ((previousQuantity < quantity) && addingMeetGroupSize)
                {
                    if (item.Product.StockQuantity >= item.Quantity+1)
                    {
                        item.Quantity +=1 ; // 自動變 3 件
                    }
                }
                else if (subMeetGroupSize && (quantity == previousQuantity - 1))
                {
                    item.Quantity -= 1;
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
                bool meetgroupSize = (currentQuantity + 1) % groupSize == 0;

                // 買二送一自動加碼：使用者原本只有 1 件或沒有，現在加到 2 件時，自動幫他加到 3 件
                if (previousQuantity < currentQuantity && meetgroupSize)
                {
                    if (item.Product.StockQuantity >= (item.Quantity+1))
                    {
                        item.Quantity += 1;  // 自動變 groupSiz
                    }
                }
            }
        }
    }
}
