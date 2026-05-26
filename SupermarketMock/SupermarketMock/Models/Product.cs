
    namespace SupermarketMock.Models
    {

        public class Product
        {
            public int Id { get; set; }

            public long SnowflakeId { get; set; }

            public string Name { get; set; } = string.Empty;

            public string? Description { get; set; }

            public decimal Price { get; set; }

            public int CategoryId { get; set; }                    
            public ProductCategory Category { get; set; } = null!;    

            public string? SubCategory { get; set; }

            public string Photo { get; set; } = string.Empty;

            public int StockQuantity { get; set; }

            public bool IsAvailable { get; set; } = true;

            public DateTime? ExpiryDate { get; set; }

            public string? Brand { get; set; }

            public decimal? Weight { get; set; }
            public string? Unit { get; set; } = "piece";

            public int? Rating { get; set; }
            public int? ReviewCount { get; set; }
            public ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();

        }
        



    }

