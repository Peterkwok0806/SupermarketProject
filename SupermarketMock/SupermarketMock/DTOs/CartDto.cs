using SupermarketMock.Models;

namespace SupermarketMock.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new();
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public ProductDto Product { get; set; } = null!;
    }

    public class UpdateCartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartSummaryDto
    {
        public decimal totalAmount { get; set; }
    }

}
