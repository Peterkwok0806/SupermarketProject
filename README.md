# 🛒 Supermarket E-Commerce Platform

[![.NET 8](https://shields.io)](https://microsoft.com)
[![Angular](https://shields.io)](https://angular.dev)
[![Tailwind CSS](https://shields.io)](https://tailwindcss.com)

Welcome to the **Supermarket E-Commerce Platform**! This is a full-stack e-commerce solution engineered with an **Enterprise-grade Architecture** to deliver a secure, scalable, and high-performance online shopping experience.

The system adopts a decoupled **Single-Page Application (SPA) frontend built with Angular 18+** and a robust **monolithic RESTful backend powered by ASP.NET Core Web API (.NET 8)**.

---

## Key Features (Business & Tech Highlights)

*   **Secure Identity & Session Management**:
    *   Full user registration, authentication, and token-based state lifecycle.
    *   Secured via **JWT (JSON Web Tokens)** for stateless API authorization.
    *   Implemented **Refresh Token Rotation (RTR)** to facilitate silent token renewal and replay attack detection, ensuring ironclad session management.
*   **Shopping Cart & Server-Side Pricing**:
    *   Real-time cart operations (Add, remove, atomic quantifiably distinct adjustments, flush).
    *   On-the-fly server-side subtotal and total calculations to guarantee financial data integrity.
*   **High-Concurrency Order Processing**:
    *   Atomic transactional workflow transforming shopping cart items into verified finalized orders.
    *   Equipped with a **Pessimistic Concurrency Guard** to eliminate deadlocks and over-selling under sudden peak traffic.
*   **Product Discovery & Autocomplete Suggestions**:
    *   Multi-level category filtering and keyword-based server-side fuzzy search.
    *   Performance-optimized **Instant Search Auto-suggestions** using client-side RxJS debouncing and a backend top-8 distinct projection to optimize DB query overhead.
*   **Asynchronous Background Processing**:
    *   Integrated **Hangfire** to offload heavy, non-blocking tasks (e.g., transactional verification emails), optimizing the primary API thread responsiveness.

---

## Tech Stack & Engineering Standards

*   **Backend Architecture**:
    *   Core Framework: .NET 8 / ASP.NET Core Web API
    *   Data Access Layer: Entity Framework Core (Code First Workflow with Migrations)
    *   Primary Database: Microsoft SQL Serve
    *   Job Orchestration: Hangfire 
    *   Distributed ID Generator: IdGen (Snowflake Id)
*   **Frontend Architecture**:
    *   Core Framework: Angular
    *   Asynchronous Pipelines: RxJS
    *   UI Framework: Tailwind CSS

## Project Structure
```text
.
├── supermarket-app/ # Angular Frontend Project
│   ├── src/
│   │   └── app/
│   │       ├── components/ # Signal-driven UI Components (Products, Cart, Order)
│   │       ├── models/ # TypeScript Domain Models & Contract Interfaces
│   │       └── services/ # Cross-component Shared Services & RxJS API Pipes
│   │       └── ...
│   └── package.json # Frontend script automation and ecosystem
│
└── SupermarketMock/ # ASP.NET Core Backend Project
    ├── Controllers/ # Thin REST Controllers decoupling routes from logic
    ├── DTOs/ # Data Transfer Objects enforcing clean structural API schemas
    ├── Models/ # EF Core Data Domain Entities
    ├── Services/ # Core Business Logic Layer (Fully decoupled)
    ├── appsettings.json # Application configuration nodes (JWT Keys, DB Connections)
    └── Program.cs # Global IoC Service Bootstrapper & Middleware Registry
```
---

## Quality Assurance & Test-Driven Standards

To satisfy enterprise compliance and defend data-integrity boundaries, rigorous **Automated Unit Testing** is deeply integrated into the core services layer.

*   **Testing Framework**: xUnit coupled with Moq.
*   **Database Isolation**: EF Core InMemory Database Provider .

### 1. ProductService Unit Specs (`ProductServiceTests.cs`)
*   `GetProductsAsync_WhenCategoryIsNull_ShouldReturnPagedAllProducts`: Validates unrestricted server-side database slicing and page calculations.
*   `GetProductsAsync_WhenCategoryHasValue_ShouldReturnPagedAndFilteredProducts`: Assures category lookup filtering complies with relational conditions.
*   `GetProductSuggestionsAsync_WhenTriggered_ShouldReturnLimitToMax8DistinctNames`: Validates that the search autocomplete endpoint tightly caps results at 8 entries to maximize thread and network efficiency.

### 2. OrderService Unit Specs (`OrderServiceTests.cs`)
*   `CreateOrderAsync_BuyXGetYFree_SuccessAndDeductStock`: Tests intricate marketing algorithms ("Buy 2 Get 1 Free") to verify server-side defensive re-pricing and atomic inventory subtraction occur precisely
*   `CreateOrderAsync_QuantitySpecialPrice_SuccessAndDeductStock`: Tests intricate marketing algorithms ("Buy 3 Get discountValue") to verify server-side defensive re-pricing and atomic inventory subtraction occur precisely
*   `CreateOrderAsync_WhenStockIsInsufficient_ShouldRollbackAndReturnErrorMessage`: Verifies that if an over-selling condition triggers, the service instantly fails, blocks database commit, throws the appropriate domain error, and strictly leaves the          inventory count uncorrupted.

---

## Getting Started

### Backend Setup (.NET 8 Web API)

**Prerequisites**:
*   .NET 8 SDK
*   Microsoft SQL Server instance running locally

**Execution**:
1.  Navigate into the backend target folder:
    ```bash
    cd SupermarketMock
    ```
2.  Restore the required NuGet dependencies and configure your local SQL Server instance in `appsettings.json`:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SupermarketDB;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```
3.  Execute the Entity Framework migration command (EF Core will automatically provision and spin up the database schema for you):
    ```bash
    dotnet ef database update
    ```
4.  Launch the API engine:
    ```bash
    dotnet run
    ```
    The API gateway will host locally at `https://localhost:7154`.

### Frontend Setup (Angular)

**Prerequisites**:
*   Node.js (v18.x or above)
*   Angular CLI (`npm install -g @angular/cli`)

**Execution**:
1.  Navigate into the frontend target folder:
    ```bash
    cd supermarket-app
    ```
2.  Install the required npm dependencies:
    ```bash
    npm install
    ```
3.  Launch the local development server under SSL:
    ```bash
    ng serve --ssl
    ```
    The application will deploy at `https://localhost:4200` (SSL is forced to support secure HTTPS handshake agreements with the .NET Core backend).

### Executing Automated Tests

1.  Navigate into the backend target folder:
    ```bash
    cd SupermarketMock.Tests
    ```
2.  Execute the full suite of automated backend service test specs
   ```bash
   dotnet test
   ```


