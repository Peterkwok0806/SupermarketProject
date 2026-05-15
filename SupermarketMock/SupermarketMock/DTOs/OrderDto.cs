using SupermarketMock.Models;

namespace SupermarketMock.DTOs
{
    public class CreateOrderDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }

    public class OrderDto
    {
        public int id { get; set; }
        public decimal totalAmount { get; set; }
        public OrderStatus status { get; set; }
        public string fullName { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string? remark { get; set; }
        public DateTime createdAt { get; set; }
        public List<OrderItemDto> orderItems { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int productId { get; set; }
        public string productName { get; set; } = string.Empty;
        public string? productPhoto { get; set; } // 如果有商品圖片
        public int quantity { get; set; }
        public decimal unitPrice { get; set; }
    }
}
