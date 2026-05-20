using Microsoft.EntityFrameworkCore;
using SupermarketMock.Models;
using SupermarketMock.DTOs;
using System.Linq;

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
            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.CartItems.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    UnitPrice = ci.UnitPrice,
                    Quantity = ci.Quantity,
                    Product = new ProductDto
                    {
                        id = ci.Product.Id,
                        name = ci.Product.Name,
                        price = ci.Product.Price,
                        photo = ci.Product.Photo
                    }
                }).ToList()
            };
        }

        private decimal TotalPrice(Cart cart) 
        {
            return cart.CartItems.Sum(item => item.UnitPrice * item.Quantity);
        }

        public async Task<CartOperationResult> GetCartByUserIdAsync(int userId)
        {
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return null;

            return new CartOperationResult
            {
                Success = true,
                Message = "已找到購物車",
                totalAmount = TotalPrice(cart),
                Cart = MapToDto(cart)
            };

        }

        

        public async Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity = 1)
        {
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

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
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
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
                totalAmount = TotalPrice(cart),
                Cart = MapToDto(cart)
            };
        }

        public async Task<CartOperationResult> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await _context.Carts
             .Include(c => c.CartItems)
             .ThenInclude(ci => ci.Product)
             .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return new CartOperationResult { Success = false, Message = "購物車不存在" };

            // 檢查商品是否存在
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null)
                return new CartOperationResult { Success = false, Message = "購物車中無此商品" };

            if (quantity < 1)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new CartOperationResult
            {
                Success = true,
                totalAmount = TotalPrice(cart),
                Cart = MapToDto(cart)
            };
        }

        public async Task<CartOperationResult> RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
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
              totalAmount = TotalPrice(cart), 
              Cart = MapToDto(cart) 
            };
         }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart != null)
            {
                cart.CartItems.Clear();
                await _context.SaveChangesAsync();
            }
        }

        


    }
}
