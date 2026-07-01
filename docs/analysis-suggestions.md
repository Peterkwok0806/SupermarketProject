# 🔍 SupermarketProject — Comprehensive Analysis & Suggestions

> Generated on 2026-07-01 — Full codebase review of ASP.NET Core backend + Angular frontend

---

## 📁 Table of Contents

1. [Backend Code Optimization](#1-backend-code-optimization)
2. [Frontend Optimization](#2-frontend-optimization)
3. [Architecture & Infrastructure](#3-architecture--infrastructure)
4. [Testing Gaps](#4-testing-gaps)
5. [New Features Worth Adding](#5-new-features-worth-adding)
6. [Priority Recommendation](#6-priority-recommendation)

---

## 1. Backend Code Optimization

### 1.1 DRY Violations — Extract Shared Promotion/Coupon Pricing Logic

**Problem:** The discount calculation logic is **duplicated** in 3 places:
- `CartService.MapToDto()` / `CartService.TotalPrice()`
- `OrderService.CreateOrderAsync()`
- `CouponService.CalculateDiscount()` / `OrderService.CalculateCouponDiscount()`

**Suggestion:** Create a shared `PricingCalculator` static helper:

```csharp
// Services/PricingCalculator.cs
public static class PricingCalculator
{
    public static decimal CalculateItemSubTotal(Product product, Promotion? promo, int quantity)
    {
        decimal basePrice = CalculateFinalPrice(product, promo);

        if (promo == null ||
            (promo.Type != PromotionType.BuyXGetYFree &&
             promo.Type != PromotionType.QuantitySpecialPrice))
        {
            return basePrice * quantity;
        }

        if (promo.Type == PromotionType.BuyXGetYFree)
        {
            int buyQty = promo.BuyQuantity!.Value;
            int freeQty = promo.FreeQuantity!.Value;
            int groupSize = buyQty + freeQty;
            int completedGroups = quantity / groupSize;
            int remainder = quantity % groupSize;
            int chargeableQuantity = (buyQty * completedGroups) + remainder;
            return basePrice * chargeableQuantity;
        }

        if (promo.Type == PromotionType.QuantitySpecialPrice)
        {
            int specialQty = promo.BuyQuantity!.Value;
            decimal specialPrice = promo.DiscountValue!.Value;
            int specialGroups = quantity / specialQty;
            int remainder = quantity % specialQty;
            return (specialGroups * specialPrice) + (remainder * basePrice);
        }

        return basePrice * quantity;
    }

    public static decimal CalculateFinalPrice(Product product, Promotion? promo)
    {
        if (promo == null) return product.Price;

        return promo.Type switch
        {
            PromotionType.PercentageOff =>
                Math.Round(product.Price * (1 - (promo.DiscountValue!.Value / 100)), 2),
            PromotionType.FixedDiscount =>
                Math.Max(0, product.Price - promo.DiscountValue!.Value),
            _ => product.Price
        };
    }

    public static decimal CalculateCouponDiscount(Coupon coupon, decimal orderSubtotal)
    {
        decimal discount = coupon.Type switch
        {
            CouponType.Percentage => orderSubtotal * (coupon.DiscountValue / 100m),
            CouponType.FixedAmount => coupon.DiscountValue,
            CouponType.FreeShipping => 0,
            _ => 0
        };

        if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount.Value)
            discount = coupon.MaximumDiscountAmount.Value;

        discount = Math.Min(discount, orderSubtotal);
        return Math.Round(discount, 2);
    }
}
```

---

### 1.2 Global Exception Handling Middleware

**Problem:** Each controller/service has its own `try/catch` blocks with inconsistent error formats. Some return `StatusCode(500, ...)`, others return `ApiResult { Success = false }`.

**Suggestion:** Add a global exception handling middleware:

```csharp
// Middleware/GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var result = new ApiResult
            {
                Success = false,
                Message = _logger.IsEnabled(LogLevel.Debug)
                    ? ex.Message
                    : "An internal server error occurred. Please try again later."
            };

            await context.Response.WriteAsJsonAsync(result);
        }
    }
}

// Register in Program.cs:
// app.UseMiddleware<GlobalExceptionMiddleware>();
```

---

### 1.3 API Rate Limiting

**Problem:** No rate limiting on any endpoint. The AI Chat endpoint (`ChatController`) and Auth endpoints are especially vulnerable to abuse.

**Suggestion:** Use `Microsoft.AspNetCore.RateLimiting`:

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("ai-chat", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// app.UseRateLimiter();

// Apply to specific endpoints:
[HttpPost("login")]
[EnableRateLimiting("auth")]
public async Task<IActionResult> LoginAsync(...) { ... }
```

---

### 1.4 Response Caching for Read-Heavy Endpoints

**Problem:** `GetCategoriesAsync()`, `GetDashboardStatsAsync()`, and `GetTopSellingProductsAsync()` are called frequently but data changes infrequently.

**Suggestion:** Add in-memory caching:

```csharp
// Register in Program.cs
builder.Services.AddMemoryCache();

// In ProductService.cs
private readonly IMemoryCache _cache;

public ProductService(SupermarketContext context, IFileUploadService fileUploadService,
    IIdGenerator<long> idGenerator, IMemoryCache cache)
{
    _context = context;
    _fileUploadService = fileUploadService;
    _idGenerator = idGenerator;
    _cache = cache;
}

public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync()
{
    return await _cache.GetOrCreateAsync("categories", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        return await _context.ProductCategories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    });
}
```

---

### 1.5 Order Cancellation — Restore Stock (TODO in code)

**Problem:** Line 407-411 in `OrderService.cs` has a `// TODO: 未來可加入恢復庫存的邏輯` comment that was never implemented.

**Suggestion:**

```csharp
if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
{
    // 1. Restore stock for each item
    foreach (var item in order.OrderItems)
    {
        var product = await _context.Products.FindAsync(item.ProductId);
        if (product != null)
        {
            product.StockQuantity += item.Quantity;
        }
    }

    // 2. Restore coupon usage count if applicable
    if (order.CouponId.HasValue)
    {
        var usage = await _context.CouponUsages
            .FirstOrDefaultAsync(u => u.OrderId == order.Id);
        if (usage != null)
        {
            var coupon = await _context.Coupons.FindAsync(order.CouponId.Value);
            if (coupon != null)
            {
                coupon.UsedCount = Math.Max(0, coupon.UsedCount - 1);
                coupon.UpdatedAt = DateTime.UtcNow;
            }
            _context.CouponUsages.Remove(usage);
        }
    }
}
```

---

### 1.6 `ReviewService.AdminGetDashboardAsync()` — Performance Issue

**Problem:** Loads ALL reviews into memory (`ToListAsync()`) then does in-memory counting. This will degrade as data grows.

**Suggestion:** Use SQL aggregation:

```csharp
public async Task<ReviewDashboardDto> AdminGetDashboardAsync()
{
    var today = DateTime.UtcNow.Date;
    var tomorrow = today.AddDays(1);

    var pendingCount = await _context.ProductReviews
        .CountAsync(r => !r.IsDeleted && r.Status == ReviewStatus.Pending);
    var approvedCount = await _context.ProductReviews
        .CountAsync(r => !r.IsDeleted && r.Status == ReviewStatus.Approved);
    var rejectedCount = await _context.ProductReviews
        .CountAsync(r => !r.IsDeleted && r.Status == ReviewStatus.Rejected);
    var hiddenCount = await _context.ProductReviews
        .CountAsync(r => !r.IsDeleted && r.Status == ReviewStatus.Hidden);
    var todayCount = await _context.ProductReviews
        .CountAsync(r => !r.IsDeleted && r.CreatedAt >= today && r.CreatedAt < tomorrow);

    var avgRating = await _context.ProductReviews
        .Where(r => !r.IsDeleted && r.Status == ReviewStatus.Approved)
        .AverageAsync(r => (double?)r.Rating);

    return new ReviewDashboardDto
    {
        PendingCount = pendingCount,
        ApprovedCount = approvedCount,
        RejectedCount = rejectedCount,
        HiddenCount = hiddenCount,
        TodayCount = todayCount,
        AverageRating = avgRating.HasValue ? Math.Round(avgRating.Value, 2) : 0
    };
}
```

---

### 1.7 `FileUploadService` — Missing File Size Validation

**Problem:** `FileUploadService.UploadImageAsync()` has no file size limit. A user could upload an extremely large file.

**Suggestion:**

```csharp
public async Task<string?> UploadImageAsync(IFormFile file, string subFolder)
{
    if (file == null || file.Length == 0)
        return null;

    // Add file size validation (5MB limit)
    const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    if (file.Length > MaxFileSize)
        return null;

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    var fileExtension = Path.GetExtension(file.FileName).ToLower();

    if (!allowedExtensions.Contains(fileExtension))
        return null;

    // ... rest of the method
}
```

---

### 1.8 Add Swagger JWT Authentication Configuration

**Problem:** Swagger UI exists but has no JWT authentication button, making it hard to test protected endpoints.

**Suggestion:**

```csharp
// Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});
```

---

### 1.9 Add Health Check Endpoint

**Suggestion:** Add database health check for monitoring:

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sqlserver", tags: new[] { "db" });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.ToString()
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});
```

---

### 1.10 `GetMyOrders` — Missing Pagination

**Problem:** `GetOrdersByUserIdAsync()` returns ALL orders without pagination. If a user has hundreds of orders, this will be slow.

**Suggestion:** Add pagination support with `PagedResultDto<OrderDto>`:

```csharp
// IOrderService.cs
Task<PagedResultDto<OrderDto>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 10);

// OrderService.cs
public async Task<PagedResultDto<OrderDto>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 10)
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : pageSize;

    var query = _context.Orders
        .Include(o => o.Coupon)
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .Where(o => o.UserId == userId);

    var totalCount = await query.CountAsync();

    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResultDto<OrderDto>
    {
        Items = orders.Select(MapToOrderDto).ToList(),
        TotalCount = totalCount,
        PageNumber = page,
        PageSize = pageSize
    };
}
```

---

### 1.11 Add Input Validation with FluentValidation

**Problem:** Manual validation scattered throughout services (e.g., checking `if (dto.Name == "")` etc.).

**Suggestion:** Use FluentValidation for clean, testable validation rules:

```csharp
// Install: FluentValidation.AspNetCore

// Validators/CreateProductDtoValidator.cs
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("商品名稱不可為空")
            .MaximumLength(200).WithMessage("商品名稱最多 200 字");
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("價格必須大於 0");
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("請選擇商品分類");
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("庫存量不可為負數");
    }
}

// Register in Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

---

## 2. Frontend Optimization

### 2.1 Remove `console.log` Statements

**Problem:** Multiple `console.log` statements throughout `auth.service.ts`, `cart.service.ts`, etc. (e.g., `console.log('1. 開始執行')`, `console.log('Token 刷新成功')`, `console.log('4. Signal 已更新')`).

**Suggestion:** Remove all debug logs or replace with a proper `LoggerService`:

```typescript
// services/logger.service.ts
@Injectable({ providedIn: 'root' })
export class LoggerService {
  private isDev = !environment.production;

  log(message: string, ...args: any[]) {
    if (this.isDev) console.log(message, ...args);
  }

  error(message: string, ...args: any[]) {
    console.error(message, ...args); // Always log errors
  }

  warn(message: string, ...args: any[]) {
    if (this.isDev) console.warn(message, ...args);
  }
}
```

---

### 2.2 Lazy Loading for All Routes

**Problem:** Most routes in `app.routes.ts` eagerly import their components, increasing initial bundle size.

**Suggestion:** Convert to lazy loading:

```typescript
export const routes: Routes = [
    {
      path: '',
      loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent)
    },
    {
      path: 'products',
      loadComponent: () => import('./components/productlist/productlist.component').then(m => m.ProductlistComponent)
    },
    {
      path: 'product/:id',
      loadComponent: () => import('./components/product-detail/product-detail.component').then(m => m.ProductDetailComponent)
    },
    {
      path: 'cart',
      loadComponent: () => import('./components/cart/cart.component').then(m => m.CartComponent),
      canActivate: [authGuard]
    },
    {
      path: 'register',
      loadComponent: () => import('./components/auth/register/register.component').then(m => m.RegisterComponent)
    },
    {
      path: 'login',
      loadComponent: () => import('./components/auth/login/login.component').then(m => m.LoginComponent)
    },
    {
      path: 'checkout',
      loadComponent: () => import('./components/checkout/checkout.component').then(m => m.CheckoutComponent),
      canActivate: [authGuard]
    },
    {
      path: 'order-success',
      loadComponent: () => import('./components/order-success/order-success.component').then(m => m.OrderSuccessComponent),
      canActivate: [authGuard]
    },
    {
      path: 'orders',
      loadComponent: () => import('./components/orders/orders.component').then(m => m.OrdersComponent),
      canActivate: [authGuard]
    },
    {
      path: 'order/:snowflakeId',
      loadComponent: () => import('./components/order-detail/order-detail.component').then(m => m.OrderDetailComponent),
      canActivate: [authGuard]
    },
    {
      path: 'profile',
      loadComponent: () => import('./components/profile/profile.component').then(m => m.ProfileComponent),
      canActivate: [authGuard]
    },
    {
      path: 'coupons',
      loadComponent: () => import('./components/coupons/coupons.component').then(m => m.CouponsComponent),
      canActivate: [authGuard]
    },
    {
      path: 'admin',
      loadChildren: () => import('./admin.routes').then(m => m.adminRoutes)
    },
    { path: '**', redirectTo: '' }
];
```

---

### 2.3 Add a Global Error Handling Service

**Problem:** Error handling is inconsistent — some components use `notificationService.error()`, others use `console.error()`, and some use raw `alert()`.

**Suggestion:** Create a centralized `ErrorHandlerService`:

```typescript
// services/global-error-handler.ts
import { ErrorHandler, Injectable, inject } from '@angular/core';
import { NotificationService } from './notification.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private notification = inject(NotificationService);

  handleError(error: any): void {
    console.error('Global error:', error);

    if (error?.status === 0) {
      this.notification.error('無法連線到伺服器，請檢查網路');
    } else if (error?.status === 403) {
      this.notification.error('您沒有權限執行此操作');
    } else if (error?.status >= 500) {
      this.notification.error('伺服器發生錯誤，請稍後再試');
    }
    // ... handle other cases
  }
}

// Register in app.config.ts:
// providers: [{ provide: ErrorHandler, useClass: GlobalErrorHandler }]
```

---

### 2.4 Add Skeleton Loading Components

**Problem:** No loading placeholders while data is being fetched. Users see blank screens or spinners.

**Suggestion:** Create reusable skeleton components:

```typescript
// components/common/skeleton/skeleton.component.ts
@Component({
  selector: 'app-skeleton',
  template: `
    <div class="skeleton" [ngClass]="type" [style.width]="width" [style.height]="height">
      <div class="skeleton-shimmer"></div>
    </div>
  `,
  styles: [`
    .skeleton {
      background: #e0e0e0;
      border-radius: 4px;
      position: relative;
      overflow: hidden;
    }
    .skeleton-shimmer {
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent);
      animation: shimmer 1.5s infinite;
    }
    @keyframes shimmer {
      0% { transform: translateX(-100%); }
      100% { transform: translateX(100%); }
    }
  `]
})
export class SkeletonComponent {
  @Input() type: 'text' | 'circle' | 'rect' = 'text';
  @Input() width: string = '100%';
  @Input() height: string = '20px';
}
```

---

### 2.5 Add Wishlist / Favorites Feature

**Suggestion:** This is listed in your `todo.md` and would be a high-value feature:

**Backend:**
```csharp
// Models/Wishlist.cs
public class Wishlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// IWishlistService / WishlistService
// WishlistController
```

**Frontend:**
- Heart icon on product cards (toggle add/remove)
- Dedicated `/wishlist` page
- Badge count in header

---

### 2.6 Add Product Comparison Feature

**Suggestion:** Allow users to select 2-4 products and compare specs side-by-side.

- Add a "Compare" button on product cards
- Store selected products in a service (max 4)
- Create a `/compare` route with a comparison table component

---

### 2.7 Add Order Tracking Timeline

**Suggestion:** Instead of just showing order status as text, create a visual timeline component:

```html
<!-- order-timeline.component.html -->
<div class="timeline">
  <div class="timeline-item" *ngFor="let step of steps" [class.active]="step.active" [class.completed]="step.completed">
    <div class="timeline-dot">
      <mat-icon *ngIf="step.completed">check</mat-icon>
    </div>
    <div class="timeline-content">
      <span class="timeline-label">{{ step.label }}</span>
      <span class="timeline-date" *ngIf="step.date">{{ step.date | date:'short' }}</span>
    </div>
  </div>
</div>
```

---

## 3. Architecture & Infrastructure

### 3.1 Docker Support

**Suggestion:** Add `Dockerfile` and `docker-compose.yml`:

```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: ./SupermarketMock
      dockerfile: Dockerfile
    ports:
      - "7154:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=SupermarketDb;User=sa;Password=YourPassword123!
    depends_on:
      - sqlserver

  frontend:
    build:
      context: ./supermarket-app
      dockerfile: Dockerfile
    ports:
      - "4200:80"

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

---

### 3.2 Serilog Structured Logging

**Problem:** Only uses `ILogger<T>` basic logging. No structured logs, no log file rotation.

**Suggestion:** Add Serilog:

```csharp
// Install: Serilog.AspNetCore, Serilog.Sinks.File, Serilog.Sinks.MSSqlServer

// Program.cs
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
);

// appsettings.json
// "Serilog": {
//   "MinimumLevel": { "Default": "Information", "Override": { "Microsoft": "Warning" } }
// }
```

---

### 3.3 API Versioning

**Suggestion:** Add API versioning to prepare for future breaking changes:

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Controller:
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController : ControllerBase { }
```

---

### 3.4 Input Validation with FluentValidation

**Problem:** Manual validation scattered throughout services.

**Suggestion:** See [Section 1.11](#111-add-input-validation-with-fluentvalidation) above.

---

## 4. Testing Gaps

### 4.1 Missing Unit Tests

**Problem:** Only `OrderServiceTests.cs` and `ProductServiceTests.cs` exist. Missing tests for:

| Service | Priority | Key Test Cases |
|---------|----------|----------------|
| `CartService` | High | Add to cart with promotions, BuyXGetYFree auto-add, update quantity, remove, clear |
| `CouponService` | High | Create/update/delete, validation logic, scope checks, usage limits |
| `AuthService` | High | Register with email verification, login, refresh token, change password |
| `ReviewService` | Medium | Create review (verified/unverified), edit within 7 days, helpful toggle |
| `DashboardService` | Medium | Stats calculation, sales trend with timezone handling |
| `AdminService` | Low | User management, role/status updates |

**Example test structure:**

```csharp
public class CartServiceTests
{
    [Fact]
    public async Task AddToCart_NewItem_CreatesCartAndAddsItem() { ... }

    [Fact]
    public async Task AddToCart_ExistingItem_IncreasesQuantity() { ... }

    [Fact]
    public async Task AddToCart_WithBuyXGetYFreePromotion_AutoAddsFreeItem() { ... }

    [Fact]
    public async Task UpdateQuantity_BelowOne_RemovesItem() { ... }

    [Fact]
    public async Task RemoveFromCart_NonExistentItem_ReturnsSuccess() { ... }

    [Fact]
    public async Task ClearCart_WithItems_EmptiesCart() { ... }
}
```

---

### 4.2 No Integration Tests

**Suggestion:** Add integration tests using `WebApplicationFactory<T>`:

```csharp
public class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace with InMemory database
                services.AddDbContext<SupermarketContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        var response = await _client.GetAsync("/api/product");
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}
```

---

## 5. New Features Worth Adding

| Feature | Effort | Value | Description |
|---------|--------|-------|-------------|
| **Wishlist/Favorites** | Medium | 🔥 High | Users can save products for later |
| **Order Tracking Timeline** | Low | 🔥 High | Visual order journey with status milestones |
| **Stock Notification** | Medium | 🔥 High | Notify when out-of-stock item returns |
| **Abandoned Cart Recovery** | Medium | 🔥 High | Use Hangfire to email users with leftover cart items |
| **Admin Audit Log** | Medium | 🔥 High | Track all admin actions (who did what, when) |
| **Product Comparison** | Medium | ⭐ Medium | Side-by-side product spec comparison |
| **Recently Viewed Products** | Low | ⭐ Medium | Easy localStorage implementation |
| **Product Tags** | Low | ⭐ Medium | Better filtering beyond categories |
| **Real-time Notifications (SignalR)** | High | 🔥 High | Order status updates, stock alerts |
| **Dark Mode** | Medium | ⭐ Medium | Listed in todo.md |
| **i18n (Internationalization)** | High | ⭐ Medium | Listed in todo.md |
| **Product SEO (Meta Tags)** | Low | ⭐ Medium | Listed in todo.md |

---

## 6. Priority Recommendation

### 🏆 Top 5 Most Impactful Changes

| # | Change | Why |
|---|--------|-----|
| 1 | **Global Exception Middleware + Structured Logging** | Foundation for reliability & debugging |
| 2 | **Extract PricingCalculator** | Eliminates DRY violations across Cart/Order/Coupon services |
| 3 | **Order Cancellation Stock Restore** | Business logic gap (already a TODO in code) |
| 4 | **Lazy Loading All Routes** | Quick win for frontend performance |
| 5 | **DashboardService SQL Aggregation** | Performance fix for data growth |

---

## 📝 Notes

- This analysis covers the codebase as of **2026-07-01**
- The project already has excellent patterns: service layer, DTOs, Fluent API, soft delete, snowflake IDs, concurrency handling
- The main areas for improvement are **code deduplication**, **performance optimization**, and **missing infrastructure** (logging, rate limiting, health checks)
- Many features from the original `todo.md` have been implemented (Coupons, Reviews, Dashboard, Excel Import/Export, AI Chat) — the remaining items are still valid
