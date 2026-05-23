namespace SupermarketMock.Models
{
    public enum OrderStatus
    {
        Pending,     // 待處理
        Paid,        // 已付款
        Processing,  // 處理中
        Shipped,     // 已出貨
        Completed,   // 已完成
        Cancelled    // 已取消
    }
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public long SnowflakeId { get; set; }

        public decimal TotalAmount { get; set; }

        // 使用 Enum
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Remark { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }     // 購買當時的單價

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
