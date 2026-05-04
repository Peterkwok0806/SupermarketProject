namespace SupermarketMock.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Photo { get; set; } = string.Empty;
    }
}
