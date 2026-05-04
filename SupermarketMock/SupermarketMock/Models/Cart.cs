namespace SupermarketMock.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    }
}
