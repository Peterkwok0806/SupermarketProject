using SupermarketMock.Models;

namespace SupermarketMock.DTOs
{
    public class ProductDto
    {
        public int id { get; set; }
        public string snowflakeId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string photo { get; set; } = string.Empty;

        // === 新增折扣相關欄位 ===
        public bool isOnSale { get; set; }          // 是否正在特價
        public decimal? originalPrice { get; set; }  // 原價（特價時才顯示，平時為 null）
        public List<string> promotionNames { get; set; } = new();    // 命中哪一個活動名稱
    }

    public class ProductDetailDto
    {
        public int id { get; set; }

        public long snowflakeId { get; set; }

        public string name { get; set; } = string.Empty;

        public string? description { get; set; }

        public decimal price { get; set; }

        public int categoryId { get; set; }
        public ProductCategory category { get; set; } = null!;

        public string? subCategory { get; set; }

        public string photo { get; set; } = string.Empty;

        public int stockQuantity { get; set; }

        public bool isAvailable { get; set; } = true;

        public DateTime? expiryDate { get; set; }

        public string? brand { get; set; }

        public decimal? weight { get; set; }
        public string? unit { get; set; } = "piece";

        public int? rating { get; set; }
        public int? reviewCount { get; set; }

        public bool isOnSale { get; set; }          // 是否正在特價
        public decimal? originalPrice { get; set; }  // 原價（特價時才顯示，平時為 null）
        public List<string> promotionNames { get; set; } = new();    // 命中哪一個活動名稱

    }

    
}
