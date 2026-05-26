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

        public DbSet<Cart> Carts => Set<Cart>();

        public DbSet<CartItem> CartItems => Set<CartItem>();

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

        public DbSet<Promotion> Promotions => Set<Promotion>();

        public DbSet<ProductPromotion> ProductPromotions => Set<ProductPromotion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === 解決 Decimal Precision Warning ===
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Weight)
                .HasColumnType("decimal(18,3)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.SubTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountValue)
                .HasColumnType("decimal(18, 2)");

            // 唯一索引
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Order>(entity =>
            {
                // 1. 為外鍵 UserId 建立索引（加速使用者訂單查詢與 JOIN）
                entity.HasIndex(o => o.UserId);

                // 2. 複合索引（適用於後台管理：依狀態排序最新訂單）
                entity.HasIndex(o => new { o.Status, o.CreatedAt });

                entity.HasIndex(p => p.SnowflakeId).IsUnique();
            });

            // 設定 SnowflakeId 為唯一索引
            modelBuilder.Entity<Product>().HasIndex(p => p.SnowflakeId).IsUnique();



            // ==================== User 與 Cart 一對一 ====================
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== CartItem 複合主鍵 ====================
            modelBuilder.Entity<CartItem>()
            .HasKey(ci => new { ci.CartId, ci.ProductId });

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem 使用 OrderId + ProductId 作為複合主鍵
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductId });

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product 與 ProductCategory 的關聯
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);

            //=====設定中間表 ProductPromotion=====
            // 1.設定中間表 ProductPromotion 的複合主鍵 (Composite Key)
            modelBuilder.Entity<ProductPromotion>()
            .HasKey(pp => new { pp.ProductId, pp.PromotionId });

            // 2.設定 Product 與 中間表 的一對多關係
            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3.設定 Promotion 與 中間表 的一對多關係
            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Promotion)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4.將 Promotion 中的 Type (Enum) 轉換為字串儲存在資料庫中
            modelBuilder.Entity<Promotion>()
                .Property(p => p.Type)
                .HasConversion<string>()
                .HasMaxLength(50);


            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@supermart.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin"
                }

             );

            modelBuilder.Entity<Product>().HasData(DataSeeder.Products);
           modelBuilder.Entity<Promotion>().HasData(DataSeeder.Promotions);
           modelBuilder.Entity<ProductPromotion>().HasData(DataSeeder.ProductPromotions);
           modelBuilder.Entity<ProductCategory>().HasData(ProductCategory.DefaultCategories);





        }



    }
}
