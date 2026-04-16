using Microsoft.EntityFrameworkCore;
using SupermarketMock.Models;

namespace SupermarketMock
{
    public class SupermarketContext : DbContext
    {
        public SupermarketContext(DbContextOptions<SupermarketContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)"); // 手動指定資料型別

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Milk", Price = 2.49m, Description = "Fresh whole milk", StockQuantity = 100 },
                new Product { Id = 2, Name = "Bread", Price = 3.09m, Description = "Whole wheat bread", StockQuantity = 75 },
                new Product { Id = 3, Name = "Eggs", Price = 4.50m, Description = "Dozen large eggs", StockQuantity = 50 }
            );
        }

        

    }
}
