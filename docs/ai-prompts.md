# AI Coding Prompts Library - Supermarket Project

**Project Tech Stack:**  
.NET 8 Web API + Clean Architecture + EF Core | Angular 18+ Signals + Tailwind CSS

---

## 2. Low Stock Alert (Dashboard Card + Backend API)

**Date:** 2026-06-26  
**Status:** ✅ Completed & Reviewed  
**Priority:** High

### Purpose
在 Admin Dashboard 新增低庫存警示卡片，顯示低庫存商品數量及 Top 5 清單。

### Prompt Used - Backend

```markdown
You are an expert .NET 8 full-stack developer using Clean Architecture.

**Project Context:**
- Product entity: Id, Name, StockQuantity, IsAvailable
- We use Repository + Service pattern
- Response format should be consistent with existing APIs (use ApiResponse or similar)

**Task:**
Create a backend API to get low stock alert statistics for Admin Dashboard.

Requirements:
1. Add method in IProductService: `Task<LowStockAlertDto> GetLowStockAlertAsync(int threshold = 10)`
2. Implement in ProductService (use efficient query, avoid loading all products)
3. Add endpoint in ProductController: GET /api/products/low-stock-alert
4. Return total low stock count + list of top 5 low stock products (Id, Name, StockQuantity)

Provide complete code for:
- LowStockAlertDto.cs
- IProductService.cs (only the new method)
- ProductService.cs (new method implementation)
- ProductController.cs (new endpoint)

Follow existing coding style, use async/await, add XML comments.

### Prompt Used - Frontend

You are an expert Angular 18+ developer using Signals and Tailwind CSS.

**Context:**
- We have Admin Dashboard component
- Existing ProductService with HTTP calls
- Use Signals for state management

**Task:**
Implement Low Stock Alert card on Dashboard.

Requirements:
1. Add `getLowStockAlert()` method in ProductService
2. In AdminDashboardComponent:
   - Call the API on init using Signals
   - Show a red warning card with count
   - Display top 5 low stock products
   - Click card can navigate to /admin/products

Provide complete code for:
- ProductService.ts (new method)
- AdminDashboardComponent.ts + .html + .css (only the new card part)

### Prompt Used - Backend Review Prompt

Please review the code you just generated for LowStockAlert.

Check carefully for the following:
- Performance issues (N+1 queries? Should use AsNoTracking for read-only?)
- EF Core best practices (efficient querying, Take(5), ordering)
- Security (Should this endpoint be restricted to Admin role only?)
- Error handling and null checks
- Consistency with existing code style and naming convention
- Potential bugs or edge cases (threshold = 0, no low stock products, etc.)
- Whether the response format matches other existing APIs

Suggest specific improvements if any.

### Prompt Used - Frontend Review Prompt

Please review the Angular code for the Low Stock Alert card.

Check for:
- Correct and modern Signal usage (signal(), computed(), effect() if needed)
- Proper error handling and loading state management
- Tailwind CSS consistency with existing dashboard cards
- TypeScript type safety and interface usage
- Potential memory leaks or unnecessary subscriptions
- Accessibility and UX (color contrast, hover effects, click area)
- Overall code cleanliness and best practices

Suggest improvements.


You are an expert full-stack .NET 8 + Angular 18+ developer.

**Project Context:**
- Clean Architecture (.NET 8 Web API + EF Core)
- Order entity exists (assume it has: Id, OrderDate, TotalAmount, Status)
- Dashboard uses Angular Signals + Tailwind
- We prefer ng2-charts or Chart.js for frontend charting

**Task: Implement Sales Trend Chart on Admin Dashboard**

**Backend Requirements:**
1. Create a new method in IOrderService (or IProductService if more suitable)

2. Implement in Service:
- Return daily sales amount for the last N days
 - Efficient query (group by date)

3. Add endpoint in OrderController (or ProductController):

4. Create or use siutuable DTO for reutrn

**Frontend Requirements (ng2-charts):**
1. Install ng2-charts and chart.js if not already installed (assume they are available)

2.In AdminDashboardComponent:
Add signal for sales trend data
Call API on init
Display a Line Chart (Sales Amount over time)
Add summary cards: Total Sales + Total Orders this period

3. Nice-to-have: Toggle between Last 7 / 14 / 30 days

**Output:**
Provide complete code for:

Backend: DTOs, IOrderService (or appropriate service), Service implementation, Controller endpoint
Frontend: Dashboard component .ts + .html (chart + summary cards)

Use modern Angular Signals. Make the chart visually appealing with Tailwind.


Please review the Sales Trend Chart implementation (Backend + Frontend).

**Check the following items carefully:**

### 1. Backend Review
- Is the query efficient? (Grouping by date, proper indexing consideration)
- Correct use of EF Core (AsNoTracking, Date handling, timezone awareness)
- DTO design is clean and useful? 
- Error handling and edge cases (no orders, future dates, etc.)
- Endpoint follows existing style and security (Admin only?)

### 2. Frontend Review (Angular + Chart)
- Correct use of modern Signals (`signal`, `computed`, `effect` if used)
- Chart configuration (ng2-charts or Chart.js) is proper
- Data transformation from API to chart format is correct
- Responsive design and Tailwind styling consistency with Dashboard
- Loading state and error handling implemented?
- Performance (no memory leak, proper cleanup)

### 3. Overall Quality
- UX: Chart is visually clear and informative?
- Code cleanliness and best practices
- Consistency with existing codebase (naming, structure, error messages)
- Accessibility (labels, colors, contrast)
- Any potential bugs or improvements

**Output Format:**
- **Strengths**
- **Issues** (Critical / Important / Minor)
- **Specific Code Suggestions**
- **Overall Score** (/10)
- **Recommended Fixes** (if any)

Be detailed and honest.

---

## 9. General Loop Engineering Prompt — 全端功能開發自動迴圈（Template）

**Date:** 2026-07-02  
**Status:** ✅ Validated via Wishlist Feature Implementation  
**Priority:** ⭐ Core Workflow Template

### Purpose
這是一個經過實戰驗證的「全端功能開發」自動迴圈 prompt 模板。適用於任何需要同時修改 ASP.NET 後端與 Angular 前端的新功能開發。直接複製下方 Prompt 範本，替換 `【】` 中的內容即可使用。

### Architecture Summary（專案約定）

| 層級 | 技術 | 關鍵檔案 |
|------|------|----------|
| 後端 ORM | EF Core + SQL Server | `SupermarketContext.cs`, `Models/` |
| 後端 API | ASP.NET Core (JWT Auth) | `Controllers/`, `Services/`, `IServices/` |
| 前端框架 | Angular 18+ Standalone Components | `app.routes.ts`, `components/`, `services/` |
| 前端樣式 | Tailwind CSS | 全域 `styles.css` |
| 使用者辨識 | Controller 取 `ClaimTypes.NameIdentifier` → `int UserId` | `GetCurrentUserId()` |
| 前端狀態 | Angular Signals (`signal`, `computed`, `effect`) | 各 `*.service.ts` |
| 回傳格式 | 一律用 `ApiResult` / `ApiResult<T>` 包裝 | `DTOs/ApiResult.cs` |

### Standard File Structure（每功能必建的「三件套 + 雙層 Service」）

```
後端（每個新功能至少建立 4 個檔案）:
├── Models/{Feature}Item.cs          ← Entity 模型
├── IServices/I{Feature}Service.cs   ← 介面
├── Services/{Feature}Service.cs     ← 實作
└── Controllers/{Feature}Controller.cs ← API 端點 + 請求 DTO

前端（每個新功能至少建立 6 個檔案）:
├── models/{feature}.ts              ← TypeScript 介面
├── services/{feature}-api.service.ts ← 純 HTTP 呼叫層
├── services/{feature}.service.ts    ← Signal 狀態管理 + 業務邏輯
├── components/{feature}/{feature}.component.ts
├── components/{feature}/{feature}.component.html
└── components/{feature}/{feature}.component.css
```

---

### 📋 Prompt 範本（直接複製使用）

```markdown
你是一個具備「架構思維」與「自動化除錯能力」的高級全端工程師。
我們即將在現有的專案中，開發【功能名稱】。

請你啟動 Loop Engineering（自動迴圈模式），主動翻閱檔案、修改程式碼、並在終端機執行測試。
在達成最終目標前，你不需要每一步都問我。如果遇到任何編譯或語法錯誤，請自行閱讀錯誤 Log 並反覆修正，直到所有檢查完全通過。

---

【第 1 階段：理解現有架構】
1. 請先搜尋並理解專案的基礎架構：
   - 後端 ORM 與資料庫配置方式（SupermarketContext.cs + Models/）。
   - 後端 API 的驗證機制：從 JWT ClaimTypes.NameIdentifier 取得當前 UserId。
   - 前端 Angular 的狀態管理方式：Signal + effect 自動同步登入狀態。
   - 前端 HttpClient 注入風格：使用 inject() 函數，API URL 從 environment.apiUrl 取得。
   - 前端 Service 雙層模式：ApiService（純 HTTP）+ Service（Signal 狀態管理）。

【第 2 階段：後端開發與自動編譯迴圈】
1. 建立所需的 Entity 模型（Models/），包含導航屬性（Navigation Properties）。
2. 在 SupermarketContext.cs 中：
   - 新增 DbSet<T>
   - 用 Fluent API 配置：複合主鍵、唯一索引（防重複）、FK 關聯、DeleteBehavior
   - Decimal 欄位指定 .HasColumnType("decimal(18,2)")
3. 【核心業務限制】：務必加入防禦性邏輯，例如「上限 50 個 / 庫存大於 0 才能加入 / 同一使用者不能重複」等條件。
4. 建立「三件套」：
   - IServices/I{Name}Service.cs — 介面定義
   - Services/{Name}Service.cs — 完整實作（含業務邏輯 + 促銷價格計算等）
   - Controllers/{Name}Controller.cs — API 端點 + 請求 DTO（嚴禁 Controller 直接接裸參數）
5. 【回傳格式一致】：所有 Service 方法回傳值必須用 ApiResult / ApiResult<T> 包裝，嚴禁回傳裸型別。
6. 在 Program.cs 的 DI 容器中註冊新 Service（AddScoped）。
7. 若涉及資料庫結構變更，自動執行：
   dotnet ef migrations add 【Migration名稱】 --project ... --startup-project ...
   dotnet ef database update --project ... --startup-project ...
8. 【自動化迴圈】：每次修改檔案後，自動執行 dotnet build。如果編譯失敗，自行看 Log 修正，直到成功。

【第 3 階段：前端開發與自動編譯迴圈】
1. 建立 models/{feature}.ts — TypeScript 介面（對應後端 ApiResult 結構）。
2. 建立 services/{feature}-api.service.ts：
   - 使用 inject(HttpClient)
   - URL 格式：`${environment.apiUrl}api/{controller}`
   - 嚴禁在此層做狀態管理，只負責 HTTP 呼叫
3. 建立 services/{feature}.service.ts：
   - 使用 inject() 注入 ApiService + AuthService + NotificationService + LoggerService
   - 使用 signal<T>() 管理狀態
   - 使用 effect() 偵測登入狀態，自動載入/重設資料
   - 提供 isInXxx() / toggleXxx() 等業務方法
4. 建立 Component（.ts + .html + .css）：
   - 使用 Angular Standalone Components
   - 注入 Service 取得資料與方法
5. 【錯誤攔截】：後端回傳 400/401/500 時，前端必須：
   - 解析 response.error.message 欄位
   - 透過 NotificationService.error() 顯示 Toast
   - 未授權時自動跳轉登入頁
6. 修改 Routes（app.routes.ts）：新增路由，受保護路由加上 canActivate: [authGuard]。
7. 修改 Navbar（header.component）：
   - 在導覽列加入帶數量 Badge 的圖標連結
   - header.component.ts 注入新 Service，暴露總數 Signal
8. 【自動化迴圈】：修改完成後，自動執行 ng build。如果噴錯，自行修正，直到綠燈通過。

---

⚠️【安全控制與中斷規則】

1. 分段檢查點（Checkpoint）：
   後端編譯通過 + Migration 完成後 → 暫停並回報，等確認後才進入前端。
   前端 ng build 通過後 → 暫停並回報，列出所有修改檔案。

2. 錯誤修正上限：
   同一個編譯錯誤連續修改重試超過 3 次都無法解決 → 立刻停止迴圈，吐出錯誤訊息。

3. 保持風格一致：
   - 建立新檔案時，模仿專案內既有的乾淨檔案
   - Angular 使用 inject() 函數（非 constructor 注入）
   - Service 使用 Signal（非 BehaviorSubject）
   - 嚴禁引入不相容的第三方套件

4. 交付時必須列出：
   - 所有新建檔案清單
   - 所有修改檔案清單
   - dotnet build 結果（0 errors）
   - ng build 結果（0 errors）

確認收到所有指令、理解業務限制與安全控制規則後，請切換至 Act Mode，從【第 1 階段】開始自主向下推進！
```

---

### 🎯 Usage Example（ Wishlist 功能實際使用範例）

將上述 Prompt 範本中的 `【功能名稱】` 替換為具體內容後：

```markdown
...開發【願望清單（Wishlist / Favorites）功能】。

【核心業務限制】：
- 每個使用者最多收藏 50 個商品
- 同一使用者不可重複收藏同一商品（資料庫層級唯一索引）
- 收藏時必須驗證商品是否存在且未被軟刪除
- 回傳的商品資料須包含促銷活動價格計算
```

### 📁 Wishlist 實際檔案清單（2026-07-02 驗證通過）

| 檔案 | 狀態 | 說明 |
|------|------|------|
| `Models/WishlistItem.cs` | 新建 | Entity 模型（Id, UserId, ProductId, CreatedAt） |
| `SupermarketContext.cs` | 修改 | DbSet + Fluent API（唯一索引, FK, DeleteBehavior） |
| `IServices/IWishlistService.cs` | 新建 | 介面（Add/Remove/Get/Check） |
| `Services/WishlistService.cs` | 新建 | 實作（含 50 上限 + 促銷價格計算 + AsNoTracking） |
| `Controllers/WishlistController.cs` | 新建 | 4 個 API 端點 + AddToWishlistDto |
| `Program.cs` | 修改 | DI 註冊 IWishlistService → WishlistService |
| `Migrations/..._AddWishlist.cs` | 自動 | EF Migration |
| `models/wishlist.ts` | 新建 | TS 介面 |
| `services/wishlist-api.service.ts` | 新建 | HTTP 呼叫層 |
| `services/wishlist.service.ts` | 新建 | Signal 狀態管理（effect 自動同步登入） |
| `components/wishlist/*` (3 files) | 新建 | 願望清單頁面（含空狀態 UI） |
| `app.routes.ts` | 修改 | /wishlist 路由 + authGuard |
| `header.component.ts` + `.html` | 修改 | Navbar ❤️ 圖標 + 數量 Badge |
| `productlist.component.ts` + `.html` | 修改 | 商品卡片愛心按鈕 |
| `product-detail.component.ts` + `.html` | 修改 | 詳情頁愛心按鈕 |

