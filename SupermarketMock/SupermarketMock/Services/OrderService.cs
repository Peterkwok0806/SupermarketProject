using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
namespace SupermarketMock.Services
{
    public class OrderService : IOrderService
    {
        private readonly SupermarketContext _context;

        public OrderService(SupermarketContext context)
        {
            _context = context;
        }

        private OrderDto MapToOrderDto(Order order, Dictionary<int, Product> lockedProducts)
        {
            return new OrderDto
            {
                id = order.Id,
                totalAmount = order.TotalAmount,
                status = order.Status,
                fullName = order.FullName,
                phone = order.Phone,
                address = order.Address,
                remark = order.Remark,
                createdAt = order.CreatedAt,
                orderItems = order.OrderItems.Select(oi=>
                {
                    var currentProduct = lockedProducts[oi.ProductId];
                    return new OrderItemDto
                    {
                        productId = oi.ProductId,
                        productName = currentProduct.Name,
                        productPhoto = currentProduct.Photo,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice
                    };
                }).ToList()
            };
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                id = order.Id,
                totalAmount = order.TotalAmount,
                status = order.Status,
                fullName = order.FullName,
                phone = order.Phone,
                address = order.Address,
                remark = order.Remark,
                createdAt = order.CreatedAt,
               
                orderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    productId = oi.ProductId,
                    productName = oi.Product.Name, 
                    productPhoto = oi.Product.Photo,
                    quantity = oi.Quantity,
                    unitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        public async Task<OrderResult> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
                return new OrderResult { Success = false, Message = "購物車是空的" };

            // 2. 依商品 ID 排序，確保所有執行緒加鎖順序一致，徹底封鎖死鎖（Deadlock）
            var sortedCartItems = cartItems.OrderBy(ci => ci.ProductId).ToList();
            int[] productIds = sortedCartItems.Select(ci => ci.ProductId).ToArray();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lockedProducts = new Dictionary<int, Product>();

                // 依商品 ID 從小到大，嚴格依序鎖定
                foreach (var pid in productIds)
                {
                    // 傳入單一 int，.FromSql 會轉譯為安全的 WHERE Id = @p0，100% 成功
                    var product = await _context.Products
                        .FromSql($"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {pid}")
                        .FirstOrDefaultAsync();

                    if (product != null)
                    {
                        lockedProducts.Add(product.Id, product);
                    }
                }
                // 建立訂單主檔
                var order = new Order
                {
                    UserId = userId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    Remark = dto.Remark,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                decimal totalAmount = 0;

                foreach (var cartItem in sortedCartItems)
                {
                    if (!lockedProducts.TryGetValue(cartItem.ProductId, out var dbProduct))
                    {
                        await transaction.RollbackAsync();
                        return new OrderResult { Success = false, Message = "商品已下架或不存在" };
                    }

                    if (dbProduct.StockQuantity < cartItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new OrderResult { Success = false, Message = $"商品 {dbProduct.Name} 庫存不足！" };
                    }

                    // 扣減記憶體中的實體庫存
                    dbProduct.StockQuantity -= cartItem.Quantity;

                    // 建立訂單明細
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += cartItem.UnitPrice * cartItem.Quantity;
                }

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems); // 移除這批購物車項目

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderResult
                {
                    Success = true,
                    Message = "訂單建立成功",
                    Order = MapToOrderDto(order, lockedProducts)
                };
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                return new OrderResult { Success = false, Message = "建立訂單失敗：" + ex.Message };
            }

        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) return null;
            return MapToOrderDto(order);


        }

        public async Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToOrderDto).ToList();
        }


    }
}
