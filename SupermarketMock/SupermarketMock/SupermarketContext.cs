using Microsoft.EntityFrameworkCore;
using SupermarketMock.Models;


namespace SupermarketMock
{
    public class SupermarketContext : DbContext
    {
        public SupermarketContext(DbContextOptions<SupermarketContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Fresh Milk", Price = 22.90m, Description = "Whole milk 1L", StockQuantity = 120, Category = ProductCategory.Dairy, Photo = "milk.jpg" },
            new Product { Id = 2, Name = "Whole Wheat Bread", Price = 18.50m, Description = "Fresh baked bread", StockQuantity = 80, Category = ProductCategory.Bakery, Photo = "bread.jpg" },
            new Product { Id = 3, Name = "Large Eggs (10pcs)", Price = 28.00m, Description = "Farm fresh eggs", StockQuantity = 65, Category = ProductCategory.Dairy, Photo = "eggs.jpg" },
            new Product { Id = 4, Name = "Red Apple", Price = 15.90m, Description = "Sweet Japan Fuji apple", StockQuantity = 200, Category = ProductCategory.Fruits, Photo = "apple.jpg" },
            new Product { Id = 5, Name = "Banana", Price = 12.90m, Description = "Philippine banana", StockQuantity = 150, Category = ProductCategory.Fruits, Photo = "banana.jpg" },
            new Product { Id = 6, Name = "Chicken Breast", Price = 48.00m, Description = "500g boneless", StockQuantity = 45, Category = ProductCategory.Meat, Photo = "chicken.jpg" },
            new Product { Id = 7, Name = "Coca Cola 1.25L", Price = 9.90m, Description = "Classic coke", StockQuantity = 90, Category = ProductCategory.Beverages, Photo = "coke.jpg" },
            new Product { Id = 8, Name = "Potato Chips", Price = 14.50m, Description = "Original flavor", StockQuantity = 110, Category = ProductCategory.Snacks, Photo = "chips.jpg" },
            new Product { Id = 9, Name = "White Rice 5kg", Price = 45.00m, Description = "Thailand long grain", StockQuantity = 60, Category = ProductCategory.Others, Photo = "rice.jpg" },
            new Product { Id = 10, Name = "Tomato", Price = 8.90m, Description = "Fresh red tomato", StockQuantity = 180, Category = ProductCategory.Vegetables, Photo = "tomato.jpg" },
            new Product { Id = 11, Name = "Strawberry Yogurt", Price = 7.90m, Description = "150g cup", StockQuantity = 95, Category = ProductCategory.Dairy, Photo = "yogurt.jpg" },
            new Product { Id = 12, Name = "Salmon Fillet", Price = 89.00m, Description = "200g fresh salmon", StockQuantity = 30, Category = ProductCategory.Seafood, Photo = "salmon.jpg" },
            new Product { Id = 13, Name = "Orange Juice", Price = 19.90m, Description = "1L 100% juice", StockQuantity = 70, Category = ProductCategory.Beverages, Photo = "orangejuice.jpg" },
            new Product { Id = 14, Name = "Ice Cream Vanilla", Price = 35.00m, Description = "500ml tub", StockQuantity = 55, Category = ProductCategory.Frozen, Photo = "icecream.jpg" },
            new Product { Id = 15, Name = "Toothpaste", Price = 16.90m, Description = "Colgate 100g", StockQuantity = 140, Category = ProductCategory.PersonalCare, Photo = "toothpaste.jpg" }
            );
        }



    }
}
