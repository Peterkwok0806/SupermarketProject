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

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                FullName = order.FullName,
                Phone = order.Phone,
                Address = order.Address,
                Remark = order.Remark,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(oi=> new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        public async Task<OrderResult> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return new OrderResult { Success = false, Message = "購物車是空的" };

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

            // 建立訂單明細
            foreach (var cartItem in cart.CartItems)
            {
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
            _context.CartItems.RemoveRange(cart.CartItems); // 清空購物車
            await _context.SaveChangesAsync();

            return new OrderResult
            {
                Success = true,
                Message = "訂單建立成功",
                Order = MapToOrderDto(order)
            };

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
