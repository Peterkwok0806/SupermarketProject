# Supermarket Project - Todo List

> Updated: 2026-07-03
> Based on: docs/analysis-suggestions.md (2026-07-01)

---

## ✅ 已完成 ✅

### Backend 核心優化
- [x] ~~Global Exception Middleware~~ - 已實作 (`Middleware/GlobalExceptionMiddleware.cs`)
- [x] ~~PricingCalculator (DRY 修復)~~ - 已實作 (`Services/PricingCalculator.cs`)
- [x] ~~Order Cancellation Stock Restore~~ - 已實作並有完整測試 (18 個單元測試)
- [x] ~~ReviewService SQL Aggregation~~ - 已重構 `AdminGetDashboardAsync()` 使用 SQL GroupBy 聚合
- [x] ~~GetMyOrders Pagination~~ - 已實作 `page`, `pageSize` 參數，回傳 `PagedResultDto<OrderDto>`
- [x] ~~Rate Limiting~~ - 已實作 (Auth: 5/分鐘, AI Chat: 10/分鐘, General: 100/分鐘)
- [x] ~~Response Caching~~ - 已設定 MemoryCache (Categories: 10分, Dashboard: 5分, TopSelling: 30分)
- [x] ~~Health Check Endpoint~~ - 已實作 (`GET /health`)
- [x] ~~Swagger JWT Authentication~~ - 已設定 (`Program.cs` AddSecurityDefinition)
- [x] ~~FluentValidation 整合~~ - 已安裝並設定 (`builder.Services.AddValidatorsFromAssemblyContaining<Program>()`)
- [x] ~~FileUploadService 5MB 驗證~~ - 已實作 (Kestrel + FormOptions 設定)

### 功能開發
- [x] ~~商品上下架 (IsAvailable) 真實開關~~ - 已實作
- [x] ~~願望清單 / 收藏 (Wishlist)~~ - 已實作 (`WishlistController`, `WishlistService`, `WishlistComponent`)
- [x] ~~庫存警示 (Low Stock Alert)~~ - 已實作 (`GetLowStockAlert` endpoint, `LowStockAlertDto`)
- [x] ~~批量操作 (Batch Actions)~~ - 已實作 (`BatchToggleAvailability`, `BatchSoftDelete` endpoints)

### Frontend 優化
- [x] ~~LoggerService~~ - 已實作 (`services/logger.service.ts`)
- [x] ~~Global Error Handler~~ - 已實作 (`services/global-error-handler.ts`)
- [x] ~~Console.log 清理~~ - 已替換為 LoggerService (auth.service.ts, cart.service.ts)
- [x] ~~部分 Lazy Loading~~ - 已實作 (product/:id, coupons, admin routes)

### Dashboard 擴充
- [x] ~~銷售趨勢圖表 (Sales Trend Chart)~~ - 已實作 (Chart.js, 支援 7/14/30 天篩選)
- [x] ~~熱銷商品 Top 10~~ - 已實作 (`GetTopSellingProductsAsync` endpoint)

### 測試覆蓋
- [x] ~~Unit Tests: OrderService~~ - 18 個測試 (含優惠券、庫存恢復)
- [x] ~~Unit Tests: ProductService~~ - 3 個測試

### 原有功能
- [x] ~~商品評價 & 評論系統 (Reviews)~~ - 已實作
- [x] ~~優惠券系統 (Coupons)~~ - 已實作
- [x] ~~Dashboard + Excel 匯入/匯出~~ - 已實作
- [x] ~~AI Chat (ChatController)~~ - 已實作

---

## 🔥 高優先順序 - Frontend 優化

### 1. 完成 Lazy Loading 所有路由
- [ ] `HomeComponent` - 改為 lazy loading
- [ ] `CartComponent` - 改為 lazy loading
- [ ] `CheckoutComponent` - 改為 lazy loading
- [ ] `OrdersComponent` - 改為 lazy loading
- [ ] `OrderDetailComponent` - 改為 lazy loading
- [ ] `ProfileComponent` - 改為 lazy loading
- [ ] `RegisterComponent` - 改為 lazy loading
- [ ] `LoginComponent` - 改為 lazy loading

### 2. Skeleton Loading Components
- [x] 建立 `SkeletonComponent`
- [x] 應用於 ProductListComponent
- [x] 應用於 OrdersComponent

---

## 📋 待辦 - 功能開發

### 高價值功能
- [ ] 商品刪除 (含圖片清理)

### UI / UX
- [ ] 深色模式 (Dark Mode)
- [ ] 響應式優化
- [ ] 商品頁 SEO 優化

### 前台 / 用戶端
- [ ] 多語系 (i18n)
- [ ] 商品比較 (Product Comparison)

---

## 📋 待辦 - 工程/系統

### 測試覆蓋
- [ ] Unit Tests: CartService
- [ ] Unit Tests: CouponService
- [ ] Unit Tests: AuthService
- [ ] Unit Tests: ReviewService
- [ ] Integration Tests (WebApplicationFactory)

### 架構/基礎設施
- [ ] Docker Support (Dockerfile + docker-compose.yml)
- [ ] Serilog Structured Logging
- [ ] API Versioning (v1.0)

### 系統功能
- [ ] 操作日誌 (Audit Log)
- [ ] 圖片管理 / 多圖上傳
- [ ] SignalR 即時通知
- [ ] 放棄購物車挽回 (Abandoned Cart Recovery)

---

## 📊 分析摘要 (from analysis-suggestions.md)

### Top 5 最有效益的變更 ✅ 全部完成
1. ~~**Global Exception Middleware + Structured Logging**~~ - 已實作
2. ~~**Extract PricingCalculator**~~ - 已實作
3. ~~**Order Cancellation Stock Restore**~~ - 已實作並有完整測試
4. ~~**Lazy Loading 部分路由**~~ - 已完成
5. ~~**ReviewService SQL Aggregation**~~ - 已完成

### 已具備的良好模式
- Service Layer 架構 ✅
- DTO 資料傳輸 ✅
- Fluent API ✅
- Soft Delete ✅
- Snowflake IDs ✅
- 併發處理 ✅
- Global Exception Middleware ✅
- Rate Limiting ✅
- Health Checks ✅
- FluentValidation ✅
- MemoryCache 快取 ✅

---

## 備註

- 專案已有優秀的架構模式，主要改進方向為：
  - **程式碼去重 (DRY)** ✅ 已修復
  - **效能優化** ✅ 已完成 (SQL Aggregation, Pagination, Caching)
  - **基礎設施完善** ✅ 已完成 (logging, rate limiting, health checks)
  - **測試覆蓋** - 需加強 CartService, CouponService, AuthService
  - **前端效能** - 建議完成所有路由的 Lazy Loading
