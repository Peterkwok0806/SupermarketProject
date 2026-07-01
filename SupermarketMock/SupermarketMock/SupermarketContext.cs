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

        public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();

        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();
        public DbSet<ReviewHelpful> ReviewHelpfuls => Set<ReviewHelpful>();

        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
        public DbSet<CouponProduct> CouponProducts => Set<CouponProduct>();
        public DbSet<CouponCategory> CouponCategories => Set<CouponCategory>();

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


            // ==================== ProductReview 設定 ====================
            // 將 ReviewStatus Enum 轉為字串儲存
            modelBuilder.Entity<ProductReview>()
                .Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // ProductReview 與 Product（多對一）
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductReview 與 User（多對一）
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductReview 與 Order（多對一，可選）
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // 唯一索引：同一使用者對同一商品 (同訂單) 僅能評論一次
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.UserId, r.ProductId, r.OrderId })
                .IsUnique();

            // 常用查詢索引
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.ProductId, r.Status, r.CreatedAt });

            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.UserId, r.CreatedAt });

            // 效能優化：加速 AdminGetDashboardAsync 的狀態統計
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.IsDeleted, r.Status })
                .HasDatabaseName("IX_ProductReviews_IsDeleted_Status");

            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.IsDeleted, r.CreatedAt })
                .HasDatabaseName("IX_ProductReviews_IsDeleted_CreatedAt");

            // ReviewImage 設定
            modelBuilder.Entity<ReviewImage>()
                .HasOne(i => i.Review)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // ReviewHelpful 複合主鍵
            modelBuilder.Entity<ReviewHelpful>()
                .HasKey(h => new { h.UserId, h.ReviewId });

            modelBuilder.Entity<ReviewHelpful>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReviewHelpful>()
                .HasOne(h => h.Review)
                .WithMany(r => r.HelpfulVotes)
                .HasForeignKey(h => h.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== Coupon 設定 ====================
            // Enum 轉字串
            modelBuilder.Entity<Coupon>()
                .Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.Scope)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Coupon Code 唯一索引
            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Coupon decimal precision
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinimumOrderAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MaximumDiscountAmount)
                .HasColumnType("decimal(18,2)");

            // Order.CouponId FK
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany()
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            // CouponUsage FK relationships
            modelBuilder.Entity<CouponUsage>()
                .HasOne(u => u.Coupon)
                .WithMany(c => c.CouponUsages)
                .HasForeignKey(u => u.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CouponUsage>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CouponUsage>()
                .HasOne(u => u.Order)
                .WithMany()
                .HasForeignKey(u => u.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CouponUsage>()
                .Property(u => u.DiscountApplied)
                .HasColumnType("decimal(18,2)");

            // CouponUsage indexes
            modelBuilder.Entity<CouponUsage>()
                .HasIndex(u => new { u.CouponId, u.UserId });

            // CouponProduct composite key
            modelBuilder.Entity<CouponProduct>()
                .HasKey(cp => new { cp.CouponId, cp.ProductId });

            modelBuilder.Entity<CouponProduct>()
                .HasOne(cp => cp.Coupon)
                .WithMany(c => c.CouponProducts)
                .HasForeignKey(cp => cp.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CouponProduct>()
                .HasOne(cp => cp.Product)
                .WithMany()
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // CouponCategory composite key
            modelBuilder.Entity<CouponCategory>()
                .HasKey(cc => new { cc.CouponId, cc.CategoryId });

            modelBuilder.Entity<CouponCategory>()
                .HasOne(cc => cc.Coupon)
                .WithMany(c => c.CouponCategories)
                .HasForeignKey(cc => cc.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CouponCategory>()
                .HasOne(cc => cc.Category)
                .WithMany()
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);


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
