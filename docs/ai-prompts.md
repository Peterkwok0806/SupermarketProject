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


