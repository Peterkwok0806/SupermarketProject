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


