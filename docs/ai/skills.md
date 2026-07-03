# AI Agent Skills & Capabilities Context

## My Skill Level (Updated July 2026)

---

## Strong Skills (Production-Ready)

### Backend Development
- .NET 8 Web API + Clean Architecture
- Entity Framework Core (Code-First, Migration, Repository Pattern)
- SQL Server + LINQ + Performance Tuning (UPDLOCK, AsNoTracking, ExecuteUpdateAsync)
- JWT Authentication with Refresh Token Rotation
- RESTful API Design + Swagger/OpenAPI
- Hangfire for background jobs
- Serilog structured logging
- Global exception handling with ProblemDetails (RFC 7807)

### Frontend Development
- Angular 19 with Signals, Standalone Components
- Tailwind CSS (utility-first styling)
- JWT token management (interceptors, guards)
- Chart.js / ng2-charts for data visualization
- SweetAlert2 for confirmation dialogs
- Reactive Forms with validation

### Database & Data
- Soft Delete pattern (IsDeleted, DeletedAt)
- Batch Operations (ExecuteUpdateAsync / ExecuteDeleteAsync)
- Query optimization and indexing strategies
- Database transactions and concurrency control

### Architecture & Patterns
- Clean Architecture + Repository + Service pattern
- FluentValidation for DTO validation
- ApiResult<T> wrapper pattern
- Pagination patterns

### Testing
- xUnit + Moq unit testing
- Playwright E2E testing

### DevOps & Deployment
- Azure App Service + SQL Database deployment
- Git & GitHub workflow
- CI/CD pipeline understanding

---

## Good Knowledge (Use with Care)

### Advanced Patterns
- CQRS (basic understanding)
- MediatR for cross-cutting concerns
- Basic Event Sourcing concepts

### Cloud & Infrastructure
- Docker containerization (basic)
- Azure Blob Storage integration
- Azure Key Vault for secrets

### Other
- Basic Prompt Engineering with AI tools
- Basic system design patterns

---

## Learning / Limited Experience

- Advanced System Design (Microservices, CQRS, Event Sourcing)
- Kubernetes orchestration
- Advanced AI Engineering (LangChain, custom agents)
- Mobile Development (MAUI / Flutter)
- GraphQL API design
- Redis / distributed caching
- SignalR for real-time features

---

## Development Preferences

### Code Style
- Prefer Clean Architecture + Repository + Service pattern
- Always use async/await (never .Result or .Wait())
- Use ApiResult<T> or ApiResult for consistent API responses
- Add XML comments for public methods
- Use FluentValidation for DTO validation
- Follow existing code style and naming conventions

### Performance
- Prefer bulk operations (ExecuteUpdateAsync / ExecuteDeleteAsync)
- Use AsNoTracking() for read-only queries
- Use UPDLOCK for critical race condition prevention

### Quality
- Production-ready, clean, maintainable code
- Comprehensive error handling
- Structured logging for debugging

### Process
- Iterative development (small steps + review)
- I review and improve AI-generated code
- Prefer incremental improvements over large refactors

---

## AI Usage Style

- I use AI to accelerate development (boilerplate, refactoring, testing)
- I always review and improve AI-generated code
- I want production-ready, clean, maintainable code
- I prefer iterative development (small steps + review)

---

## Instruction for AI Agent

When generating code, always consider my skill level above:

### DO:
- Use patterns I know well (Clean Architecture, Repository, Service)
- Suggest incremental improvements
- Provide code with XML comments
- Include validation (FluentValidation)
- Show bulk operation alternatives for performance
- Add error handling best practices

### DON'T:
- Use overly advanced patterns I haven't implemented yet (unless I ask)
- Generate complex microservices architecture
- Use patterns requiring Docker/Kubernetes knowledge
- Suggest GraphQL when REST would work
- Use blocking calls (.Result, .Wait())

### Consider Adding:
- Performance tips for EF Core
- Pagination for list endpoints
- Global exception handling
- Health check endpoints
- Structured logging setup
- E2E test examples with Playwright

---

## Quick Reference

### Preferred Patterns
```
✅ Good                          ❌ Avoid
---                              ----
async/await                      .Result, .Wait()
ApiResult<T>                     raw objects
FluentValidation                 manual validation
ExecuteUpdateAsync               loop + Update
AsNoTracking()                   default tracking
UPDLOCK hint                     no locking
Serilog                          Console.WriteLine
ProblemDetails                   generic errors
```

### File Locations
- Backend: `SupermarketMock/src/`
- Frontend: `supermarket-app/src/`
- Tests: `SupermarketMock.Tests/`
- Docs: `docs/ai/`

### Key Technologies
- .NET 8, EF Core, SQL Server
- Angular 19, Tailwind CSS 4
- Hangfire, Swagger, Serilog
- xUnit, Moq, Playwright
- Azure App Service, Azure SQL
