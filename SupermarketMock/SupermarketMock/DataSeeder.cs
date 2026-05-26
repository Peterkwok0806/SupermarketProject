using SupermarketMock.Models;

namespace SupermarketMock
{
    public static class DataSeeder
    {
        // ==================== 1. 商品 Seed Data ====================
        public static List<Product> Products => new List<Product>
        {
            new Product { Id = 1,   SnowflakeId = 317625235773855528,  Name = "Fresh Milk",          Price = 22.90m, Description = "Whole milk 1L",          StockQuantity = 120, CategoryId = 5,  Photo = "milk.jpg" },
            new Product { Id = 2,   SnowflakeId = 317625235773854947,  Name = "Whole Wheat Bread",   Price = 18.50m, Description = "Fresh baked bread",      StockQuantity = 80,  CategoryId = 6,  Photo = "bread.jpg" },
            new Product { Id = 3,   SnowflakeId = 317625235773856039,  Name = "Large Eggs (10pcs)",  Price = 28.00m, Description = "Farm fresh eggs",        StockQuantity = 65,  CategoryId = 5,  Photo = "eggs.jpg" },
            new Product { Id = 4,   SnowflakeId = 317625235773857742,  Name = "Red Apple",           Price = 15.90m, Description = "Sweet Japan Fuji apple", StockQuantity = 200, CategoryId = 2,  Photo = "apple.jpg" },
            new Product { Id = 5,   SnowflakeId = 317625235773855304,  Name = "Banana",              Price = 12.90m, Description = "Philippine banana",      StockQuantity = 150, CategoryId = 2,  Photo = "banana.jpg" },
            new Product { Id = 6,   SnowflakeId = 317625235773856174,  Name = "Chicken Breast",      Price = 48.00m, Description = "500g boneless",         StockQuantity = 45,  CategoryId = 3,  Photo = "chicken.jpg" },
            new Product { Id = 7,   SnowflakeId = 317625235773855812,  Name = "Coca Cola 1.25L",     Price = 9.90m,  Description = "Classic coke",          StockQuantity = 90,  CategoryId = 7,  Photo = "coke.jpg" },
            new Product { Id = 8,   SnowflakeId = 317625235773854539,  Name = "Potato Chips",        Price = 14.50m, Description = "Original flavor",        StockQuantity = 110, CategoryId = 8,  Photo = "chips.jpg" },
            new Product { Id = 9,   SnowflakeId = 317625235773856987,  Name = "White Rice 5kg",      Price = 45.00m, Description = "Thailand long grain",   StockQuantity = 60,  CategoryId = 12, Photo = "rice.jpg" },
            new Product { Id = 10,  SnowflakeId = 317625235773854504, Name = "Tomato",              Price = 8.90m,  Description = "Fresh red tomato",      StockQuantity = 180, CategoryId = 1,  Photo = "tomato.jpg" },
            new Product { Id = 11,  SnowflakeId = 317625235773856480, Name = "Strawberry Yogurt",   Price = 7.90m,  Description = "150g cup",              StockQuantity = 95,  CategoryId = 5,  Photo = "yogurt.jpg" },
            new Product { Id = 12,  SnowflakeId = 317625235773857309, Name = "Salmon Fillet",       Price = 89.00m, Description = "200g fresh salmon",     StockQuantity = 30,  CategoryId = 4,  Photo = "salmon.jpg" },
            new Product { Id = 13,  SnowflakeId = 317625235773854823, Name = "Orange Juice",        Price = 19.90m, Description = "1L 100% juice",         StockQuantity = 70,  CategoryId = 7,  Photo = "orangejuice.jpg" },
            new Product { Id = 14,  SnowflakeId = 317625235773854877, Name = "Ice Cream Vanilla",   Price = 35.00m, Description = "500ml tub",             StockQuantity = 55,  CategoryId = 9,  Photo = "icecream.jpg" },
            new Product { Id = 15,  SnowflakeId = 317625235773854243, Name = "Toothpaste",          Price = 16.90m, Description = "Colgate 100g",          StockQuantity = 140, CategoryId = 11, Photo = "toothpaste.jpg" }
        };

        // ==================== 2. 促銷活動 Seed Data ====================

        public static List<Promotion> Promotions => new List<Promotion>
        {
            new Promotion
            {
                Id = 1,
                Name = "Summer Dairy 20% OFF",
                Type = PromotionType.PercentageOff,
                DiscountValue = 20, // 20% off (八折)
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 8, 31)
            },
            new Promotion
            {
                Id = 2,
                Name = "Meat & Seafood Flash Sale",
                Type = PromotionType.FixedDiscount,
                DiscountValue = 15, // 現折 15 元
                StartDate = new DateTime(2026, 5, 20),
                EndDate = new DateTime(2026, 5, 30)
            },
            new Promotion
            {
                Id = 3,
                Name = "Soft Drinks Buy 2 Get 1 FREE",
                Type = PromotionType.BuyXGetYFree,
                BuyQuantity = 2,
                FreeQuantity = 1,
                StartDate = new DateTime(2026, 5, 1),
                EndDate = new DateTime(2026, 5, 31)
            },
            new Promotion
            {
                Id = 4,
                Name = "Snacks Bundle Offer",
                Type = PromotionType.QuantitySpecialPrice,
                BuyQuantity = 2,
                DiscountValue = 25, // 任選2件特價 25 元
                StartDate = new DateTime(2026, 5, 15),
                EndDate = new DateTime(2026, 6, 15)
            }
        };

        // ==================== 3. 多對多中間表 Seed Data (結合商品與活動) ====================
        public static List<ProductPromotion> ProductPromotions => new List<ProductPromotion>
        {
            // 活動 1 (乳製品八折) -> 綁定鮮奶(1)、優格(11)
            new ProductPromotion { ProductId = 1, PromotionId = 1, Priority = 10 }, // 沿用全域時間
            new ProductPromotion
            {
                ProductId = 11,
                PromotionId = 1,
                Priority = 10,
                OverrideStartDate = new DateTime(2026, 6, 15), // 優格特例：較晚開始
                OverrideEndDate = new DateTime(2026, 7, 15)    // 較早結束
            },

            // 活動 2 (肉類海鮮折15元) -> 綁定雞胸肉(6)、鮭魚(12)
            new ProductPromotion { ProductId = 6, PromotionId = 2, Priority = 20 },
            new ProductPromotion { ProductId = 12, PromotionId = 2, Priority = 20 },

            // 活動 3 (飲品買二送一) -> 綁定可樂(7)、柳橙汁(13)
            new ProductPromotion { ProductId = 7, PromotionId = 3, Priority = 5 },
            new ProductPromotion
            {
                ProductId = 13,
                PromotionId = 3,
                Priority = 5,
                OverrideEndDate = new DateTime(2026, 5, 25) // 柳橙汁特例：提早一天結束活動
            },

            // 活動 4 (零食2件特價25元) -> 綁定洋芋片(8)
            new ProductPromotion { ProductId = 8, PromotionId = 4, Priority = 1 }
        };




    }
}
