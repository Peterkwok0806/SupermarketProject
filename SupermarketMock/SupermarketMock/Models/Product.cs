
    namespace SupermarketMock.Models
    {

        public class Product
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string? Description { get; set; }

            public decimal Price { get; set; }

            public decimal? OriginalPrice { get; set; }

            public int CategoryId { get; set; }                    
            public ProductCategory Category { get; set; } = null!;    

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

        // ==================== Seed Data ====================
        public static List<Product> SeedData => new List<Product>
        {
            new Product { Id = 1,  Name = "Fresh Milk",          Price = 22.90m, Description = "Whole milk 1L",          StockQuantity = 120, CategoryId = 5,  Photo = "milk.jpg" },
            new Product { Id = 2,  Name = "Whole Wheat Bread",   Price = 18.50m, Description = "Fresh baked bread",      StockQuantity = 80,  CategoryId = 6,  Photo = "bread.jpg" },
            new Product { Id = 3,  Name = "Large Eggs (10pcs)",  Price = 28.00m, Description = "Farm fresh eggs",        StockQuantity = 65,  CategoryId = 5,  Photo = "eggs.jpg" },
            new Product { Id = 4,  Name = "Red Apple",           Price = 15.90m, Description = "Sweet Japan Fuji apple", StockQuantity = 200, CategoryId = 2,  Photo = "apple.jpg" },
            new Product { Id = 5,  Name = "Banana",              Price = 12.90m, Description = "Philippine banana",      StockQuantity = 150, CategoryId = 2,  Photo = "banana.jpg" },
            new Product { Id = 6,  Name = "Chicken Breast",      Price = 48.00m, Description = "500g boneless",         StockQuantity = 45,  CategoryId = 3,  Photo = "chicken.jpg" },
            new Product { Id = 7,  Name = "Coca Cola 1.25L",     Price = 9.90m,  Description = "Classic coke",          StockQuantity = 90,  CategoryId = 7,  Photo = "coke.jpg" },
            new Product { Id = 8,  Name = "Potato Chips",        Price = 14.50m, Description = "Original flavor",        StockQuantity = 110, CategoryId = 8,  Photo = "chips.jpg" },
            new Product { Id = 9,  Name = "White Rice 5kg",      Price = 45.00m, Description = "Thailand long grain",   StockQuantity = 60,  CategoryId = 12, Photo = "rice.jpg" },
            new Product { Id = 10, Name = "Tomato",              Price = 8.90m,  Description = "Fresh red tomato",      StockQuantity = 180, CategoryId = 1,  Photo = "tomato.jpg" },
            new Product { Id = 11, Name = "Strawberry Yogurt",   Price = 7.90m,  Description = "150g cup",              StockQuantity = 95,  CategoryId = 5,  Photo = "yogurt.jpg" },
            new Product { Id = 12, Name = "Salmon Fillet",       Price = 89.00m, Description = "200g fresh salmon",     StockQuantity = 30,  CategoryId = 4,  Photo = "salmon.jpg" },
            new Product { Id = 13, Name = "Orange Juice",        Price = 19.90m, Description = "1L 100% juice",         StockQuantity = 70,  CategoryId = 7,  Photo = "orangejuice.jpg" },
            new Product { Id = 14, Name = "Ice Cream Vanilla",   Price = 35.00m, Description = "500ml tub",             StockQuantity = 55,  CategoryId = 9,  Photo = "icecream.jpg" },
            new Product { Id = 15, Name = "Toothpaste",          Price = 16.90m, Description = "Colgate 100g",          StockQuantity = 140, CategoryId = 11, Photo = "toothpaste.jpg" }
        };

    }
        



    }

