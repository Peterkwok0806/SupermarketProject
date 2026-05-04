namespace SupermarketMock.Models
{
    public class CartItem
    {

        public int CartId { get; set; }

        public Cart Cart { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
