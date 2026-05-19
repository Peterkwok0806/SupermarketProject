namespace SupermarketMock.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }  
        public int DisplayOrder { get; set; }


        public static List<ProductCategory> DefaultCategories => new()
        {
            new ProductCategory { Id = 1, Name = "Vegetables", Description = "蔬菜", Icon = "🥬", DisplayOrder = 1 },
            new ProductCategory { Id = 2, Name = "Fruits", Description = "水果", Icon = "🍎", DisplayOrder = 2 },
            new ProductCategory { Id = 3, Name = "Meat", Description = "肉類", Icon = "🥩", DisplayOrder = 3 },
            new ProductCategory { Id = 4, Name = "Seafood", Description = "海鮮", Icon = "🐟", DisplayOrder = 4 },
            new ProductCategory { Id = 5, Name = "Dairy", Description = "乳製品", Icon = "🥛", DisplayOrder = 5 },
            new ProductCategory { Id = 6, Name = "Bakery", Description = "麵包/烘焙", Icon = "🍞", DisplayOrder = 6 },
            new ProductCategory { Id = 7, Name = "Beverages", Description = "飲料", Icon = "🥤", DisplayOrder = 7 },
            new ProductCategory { Id = 8, Name = "Snacks", Description = "零食", Icon = "🍿", DisplayOrder = 8 },
            new ProductCategory { Id = 9, Name = "Frozen", Description = "冷凍食品", Icon = "❄️", DisplayOrder = 9 },
            new ProductCategory { Id = 10, Name = "Household", Description = "家居用品", Icon = "🏠", DisplayOrder = 10 },
            new ProductCategory { Id = 11, Name = "PersonalCare", Description = "個人護理", Icon = "🧴", DisplayOrder = 11 },
            new ProductCategory { Id = 12, Name = "Others", Description = "其他", Icon = "📦", DisplayOrder = 99 }
        };
    }



}
