using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SupermarketMock;
using SupermarketMock.DTOs;
using SupermarketMock.Services;
using System.Text;
using IdGen;
using Hangfire;
using SupermarketMock.IServices;
using SupermarketMock.Middleware;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OfficeOpenXml;
using Microsoft.OpenApi.Models;
using FluentValidation;


var builder = WebApplication.CreateBuilder(args);

// ===== Kestrel 全域請求大小限制（防禦大檔案上傳拖垮伺服器）=====
// 超過此限制的請求會在 Kestrel 層級直接被拒絕（413 Payload Too Large）
// 不會進入 ASP.NET Core 管線，也不會佔用應用程式記憶體
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5 MB
});

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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

// Multipart 表單上傳大小限制（與 Kestrel 保持一致）
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5 MB
    options.ValueLengthLimit = 5 * 1024 * 1024;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("https://localhost:4200") // 允許 Angular 的網址
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//註冊 DbContext
builder.Services.AddDbContext<SupermarketContext>(options =>
    options.UseSqlServer(connectionString));

// 註冊 Hangfire 並指定使用 SQL Server
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180) // 支援到最新的 SqlServer 規格
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

// 快取服務（用於 Product Categories、Dashboard Stats 等讀取頻繁的端點）
builder.Services.AddMemoryCache();


//Services註冊
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();

// FluentValidation：掃描組件並自動註冊所有 AbstractValidator<T> 為 IValidator<T>
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 綁定 appsettings.json 中的 "Jwt" 區段到 JwtSetting 類別
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// 綁定 appsettings.json 中的 "Smtp" 區段到 SmtpSettings 類別
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });
builder.Services.AddAuthorization();

// ===== API Rate Limiting =====
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Rate limit exceeded 回傳統一 JSON 格式
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? (int)retryAfterValue.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        var result = new ApiResult
        {
            Success = false,
            Message = $"Too many requests. Please try again after {retryAfter} seconds."
        };

        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        await context.HttpContext.Response.WriteAsync(json, cancellationToken);
    };

    // Auth endpoints: 5 requests per minute per IP (prevent brute-force)
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // AI Chat: 10 requests per minute per IP (expensive OpenAI calls)
    options.AddFixedWindowLimiter("ai-chat", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // General: 100 requests per minute per IP (default for all endpoints)
    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 2;
    });
});

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString!, name: "sqlserver", tags: new[] { "db" });

// 註冊雪花 ID 產生器，設定當前伺服器節點編號為 1
builder.Services.AddSingleton<IIdGenerator<long>>(new IdGenerator(1));

// 設定 EPPlus License Context (個人 / 學習用途使用 NonCommercialPersonal)
ExcelPackage.License.SetNonCommercialPersonal("SupermarketProject");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

// 全域例外處理中介層（放於管線早期，攔截所有未處理的例外）
app.UseMiddleware<GlobalExceptionMiddleware>();

// 啟用 Rate Limiting（放於 Middleware 後、Authorization 前）
app.UseRateLimiter();

app.UseAuthorization();

// Health Check 端點：GET /health
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.ToString(),
                description = e.Value.Description
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapControllers();

app.UseHangfireDashboard();

// Serve Angular SPA: fallback to index.html for client-side routing
app.MapFallbackToFile("index.html");

app.Run();
