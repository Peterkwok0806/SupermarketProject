namespace SupermarketMock.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public ProductDto Product { get; set; } = null!;
    }
    
}
