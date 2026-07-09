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
using Microsoft.SemanticKernel;
using OpenAI;


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


// ===== Semantic Kernel 註冊（官方 DI 模式） =====
// AI Service 會自動成為 Singleton，Kernel 為 Transient，避免 Captive Dependency
var aiSettings = builder.Configuration.GetSection("AzureOpenAI");
var modelId = aiSettings["ModelId"] ?? "gpt-4o-mini";
var endpoint = aiSettings["Endpoint"];
var apiKey = aiSettings["ApiKey"];
var deploymentName = aiSettings["DeploymentName"];

// 檢查 ApiKey 是否為 placeholder（包含雙底線或以 YOUR_ 開頭）
static bool IsPlaceholder(string? value) =>
    string.IsNullOrWhiteSpace(value) ||
    value.Contains("__") ||
    value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);

var kernelBuilder = builder.Services.AddKernel();

if (!string.IsNullOrWhiteSpace(deploymentName) && !string.IsNullOrWhiteSpace(endpoint))
{
    // Azure OpenAI 模式
    if (IsPlaceholder(apiKey))
        throw new InvalidOperationException(
            "AzureOpenAI:ApiKey 未設定或仍為 placeholder，請透過環境變數 AzureOpenAI__ApiKey 設定有效的 API Key。");
    kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey!);
}
else if (endpoint?.Contains("localhost") == true || endpoint?.Contains("127.0.0.1") == true)
{
    // Ollama 本地模式 — 建立自訂 OpenAIClient 指向 Ollama endpoint
    var ollamaOptions = new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint)
    };
    var credential = new System.ClientModel.ApiKeyCredential(apiKey ?? "ollama");
    var openAIClient = new OpenAIClient(credential, ollamaOptions);
    kernelBuilder.AddOpenAIChatCompletion(modelId, openAIClient: openAIClient);
}
else if (!string.IsNullOrWhiteSpace(endpoint) && !IsPlaceholder(apiKey))
{
    // OpenAI / OpenRouter 直連模式
    kernelBuilder.AddOpenAIChatCompletion(modelId, apiKey!, endpoint);
}
else
{
    throw new InvalidOperationException(
        $"AI 設定不完整：Endpoint='{endpoint ?? "(空)"}', ApiKey 是否有效={!IsPlaceholder(apiKey)}。" +
        "請檢查 appsettings.json 中的 AzureOpenAI 區段，或透過環境變數 AzureOpenAI__ApiKey 提供有效的 API Key。");
}

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
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();

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
