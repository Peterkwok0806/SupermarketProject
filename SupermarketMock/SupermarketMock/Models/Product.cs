
    namespace SupermarketMock.Models
    {
        // Put the enum here, before the class
        public enum ProductCategory
        {
            Vegetables,     // 蔬菜
            Fruits,         // 水果
            Meat,           // 肉類
            Seafood,        // 海鮮
            Dairy,          // 乳製品
            Bakery,         // 麵包/烘焙
            Beverages,      // 飲料
            Snacks,         // 零食
            Frozen,         // 冷凍食品
            Household,      // 家居用品
            PersonalCare,   // 個人護理
            Others          // 其他
        }

        public class Product
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string? Description { get; set; }

            public decimal Price { get; set; }

            public decimal? OriginalPrice { get; set; }

            public ProductCategory Category { get; set; }     // ← 使用 enum

            public string? SubCategory { get; set; }

            public string Photo { get; set; } = string.Empty;   // e.g. "milk.jpg"

            public int StockQuantity { get; set; }

            public bool IsAvailable { get; set; } = true;

            public DateTime? ExpiryDate { get; set; }

            public string? Brand { get; set; }

            public decimal? Weight { get; set; }
            public string? Unit { get; set; } = "piece";

            public int? Rating { get; set; }
            public int? ReviewCount { get; set; }

            public bool IsOnSale { get; set; } = false;
            public int? DiscountPercentage { get; set; }
        }
    }

