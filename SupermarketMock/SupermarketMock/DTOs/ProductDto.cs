namespace SupermarketMock.DTOs
{
    public class ProductDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string photo { get; set; } = string.Empty;
    }
}
