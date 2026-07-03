# Supermarket Project Context

## Tech Stack

- **Backend:** .NET 8 Web API, Clean Architecture, EF Core Code-First, Repository + Service Pattern
- **Frontend:** Angular 19 with Signals, Standalone Components, Tailwind CSS 4
- **Database:** Microsoft SQL Server with Soft Delete (IsDeleted, DeletedAt)
- **Authentication:** JWT with Refresh Token Rotation
- **Background Jobs:** Hangfire (recurring jobs, delayed tasks)
- **API Documentation:** Swagger/OpenAPI with XML comments
- **Deployment:** Azure App Service + Azure SQL Database
- **Testing:** xUnit + Moq (Unit Tests), Playwright (E2E)
- **Logging:** Serilog (structured logging)
- **Validation:** FluentValidation (DTO validation)

---

## Coding Standards

### General
- Use async/await everywhere, never use .Result or .Wait()
- Follow Clean Architecture layers strictly (Domain → Application → Infrastructure → API)
- Use ApiResult<T> for all API responses (consistent error handling)
- Add XML comments for all public methods and DTOs
- Prefer bulk operations (ExecuteUpdateAsync / ExecuteDeleteAsync) for performance
- Always filter !IsDeleted in queries unless explicitly needed for soft-deleted data

### API Design
- RESTful endpoint naming conventions (/api/{resource}/{action})
- Pagination for all list endpoints (page, pageSize, totalCount)
- Global exception handling with ProblemDetails
- Versioning strategy for breaking changes (URL-based: /api/v1/)

### Database
- Use AsNoTracking() for read-only queries
- Use UPDLOCK for race condition prevention in critical operations
- Index foreign keys and frequently queried columns
- Use Transactions for operations spanning multiple tables
- Seed data via Migrations or separate seed classes

### Validation
- Use FluentValidation for all DTOs
- Validate at API boundary, not just service layer
- Return detailed validation errors (400 Bad Request)

### Logging
- Use Serilog with structured logging
- Log: Request/Response (excluding sensitive data), Exceptions with stack trace, Business events
- Use correlation IDs for request tracing

### Error Handling
- Never expose internal errors to clients
- Use ProblemDetails standard (RFC 7807)
- Global exception filter in API layer

---

## Project Structure

### Backend (.NET)
```
SupermarketMock/
├── src/
│   ├── SupermarketMock.Domain/          # Entities, Value Objects, Interfaces
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Interfaces/
│   ├── SupermarketMock.Application/     # Services, DTOs, Validators, Interfaces
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Interfaces/
│   ├── SupermarketMock.Infrastructure/  # EF Core, Repositories, External Services
│   │   ├── Data/
│   │   ├── Repositories/
│   │   └── Services/
│   └── SupermarketMock.Api/             # Controllers, Middleware, Configuration
│       ├── Controllers/
│       ├── Middleware/
│       └── Filters/
└── tests/
    └── SupermarketMock.Tests/           # Unit Tests
```

### Frontend (Angular)
```
supermarket-app/
├── src/
│   ├── app/
│   │   ├── core/                       # Core module (guards, interceptors, services)
│   │   ├── shared/                     # Shared components (pipes, directives, widgets)
│   │   ├── features/                   # Feature modules (products, orders, admin)
│   │   └── app.config.ts               # App configuration
│   └── styles.scss                     # Global styles
```

---

## API Conventions

### Response Format (ApiResult<T>)
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": [],
  "pagination": { "page": 1, "pageSize": 10, "totalCount": 100 }
}
```

### Error Response (ProblemDetails - RFC 7807)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": { "fieldName": ["Error message"] }
}
```

### HTTP Status Codes
| Code | Usage |
|------|-------|
| 200 OK | Successful read/update |
| 201 Created | Successful create |
| 204 No Content | Successful delete |
| 400 Bad Request | Validation errors |
| 401 Unauthorized | Missing/invalid JWT |
| 403 Forbidden | Insufficient permissions |
| 404 Not Found | Resource not found |
| 409 Conflict | Duplicate resource |
| 500 Internal Server Error | Unexpected errors |

---

## Security
- JWT Access Token: 15-30 minutes expiry
- Refresh Token: 7 days expiry, stored in HttpOnly cookie
- Password hashing: BCrypt with work factor 12
- CORS: Configure allowed origins explicitly
- Rate limiting: Consider AspNetCoreRateLimit for API protection
- Input sanitization: Prevent SQL injection (EF Core handles this) and XSS
- Audit logging: Log all write operations with user ID and timestamp

---

## Deployment

### Azure Resources
- Azure App Service (ASP.NET Core Runtime)
- Azure SQL Database (DTU or Serverless)
- Azure Blob Storage (for images/files)

### Required Environment Variables
- ConnectionStrings__DefaultConnection
- JwtSettings__Secret
- JwtSettings__Issuer
- JwtSettings__Audience
- Serilog__WriteTo__1__Args__connectionString

### Health Checks
- Implement /health endpoint for App Service health monitoring
- Include SQL, Hangfire health checks

---

## Testing Strategy

### Unit Tests (xUnit + Moq)
- Test all Service methods
- Test all FluentValidation validators
- Aim for 80%+ coverage on business logic
- Mock repositories, external services

### Integration Tests
- Test API endpoints with TestServer
- Use in-memory database or test container
- Test authentication flow

### E2E Tests (Playwright)
- Critical user journeys (login, checkout, order history)
- Run in CI/CD pipeline

---

## Current State

### Entities
- Product
- Order
- User
- Review
- Coupon

### Features
- Admin Dashboard with Low Stock Alert
- Sales Trend Chart
- Batch Operations
- Product management
- Order management
- User management
- Review management
- Coupon management

---

## TODO / Future Enhancements
- [ ] Add caching layer (Redis)
- [ ] Implement CQRS pattern for complex queries
- [ ] Add notification service (email/SMS)
- [ ] Implement payment integration
- [ ] Add real-time features (SignalR)
- [ ] Mobile app (MAUI/Flutter)
