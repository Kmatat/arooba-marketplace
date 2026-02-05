# Arooba Marketplace - Backend API

**Egypt's Most Inclusive Local E-Commerce Platform**

> Arooba connects Egyptian artisans, craftspeople, and local vendors with customers through a unified marketplace. This backend powers vendor management, product catalogs, order processing, and a sophisticated pricing engine with full financial reconciliation.

---

## Table of Contents

- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Business Modules](#business-modules)
- [The Pricing Engine](#the-pricing-engine)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Docker](#docker)
- [Testing](#testing)
- [License](#license)

---

## Architecture

The backend follows **Clean Architecture** with the **CQRS (Command Query Responsibility Segregation)** pattern, ensuring a clear separation of concerns and testability at every layer.

```
                    +-----------------------+
                    |     Arooba.API         |
                    |  ASP.NET Core 8 Web   |
                    |  Controllers, Middleware|
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |  Arooba.Application    |
                    |  CQRS Handlers (MediatR)|
                    |  Validators, DTOs      |
                    +-----------+-----------+
                                |
              +-----------------+-----------------+
              |                                   |
  +-----------v-----------+         +-------------v-----------+
  |    Arooba.Domain       |         |  Arooba.Infrastructure   |
  |  Entities, Value Objects|         |  EF Core, Repositories   |
  |  Domain Events, Enums  |         |  Pricing Engine, SQL     |
  +------------------------+         +-------------------------+
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|---------------|
| **Presentation** | `Arooba.API` | ASP.NET Core 8 Web API, controllers, middleware, authentication, Swagger/OpenAPI |
| **Application** | `Arooba.Application` | CQRS command/query handlers via MediatR, FluentValidation validators, AutoMapper profiles, DTOs, business interfaces, pipeline behaviors |
| **Domain** | `Arooba.Domain` | Core business entities, value objects (`Money`, `PricingBreakdown`, `Address`, `PhoneNumber`), domain events, enumerations, business rules |
| **Infrastructure** | `Arooba.Infrastructure` | Entity Framework Core 8 persistence with MS SQL Server, repository implementations, pricing engine, external service integrations |

### Design Principles

- **Dependency Inversion**: Inner layers never depend on outer layers. Domain has zero external dependencies.
- **CQRS**: Commands (writes) and Queries (reads) are separate MediatR handlers for clarity and scalability.
- **Domain Events**: Business-critical state changes emit events (e.g., `OrderPlaced`, `VendorApproved`) dispatched via MediatR.
- **Result Pattern**: Operations return `Result<T>` objects instead of throwing exceptions for expected failures.
- **Value Objects**: Financial amounts use `Money`, addresses use `Address`, ensuring domain integrity.

---

## Technology Stack

| Category | Technology | Version |
|----------|-----------|---------|
| **Runtime** | .NET | 8.0 |
| **Web Framework** | ASP.NET Core | 8.0 |
| **ORM** | Entity Framework Core | 8.0 |
| **Database** | Microsoft SQL Server | 2022 |
| **CQRS / Mediator** | MediatR | 12.2.0 |
| **Validation** | FluentValidation | 11.9.0 |
| **Object Mapping** | AutoMapper | 13.0.1 |
| **Authentication** | JWT Bearer (ASP.NET Core) | 8.0.0 |
| **API Documentation** | Swashbuckle / Swagger | 6.5.0 |
| **Testing** | xUnit | 2.7+ |
| **Test Assertions** | FluentAssertions | 6.12+ |
| **Mocking** | Moq | 4.20+ |
| **Containerization** | Docker | Multi-stage build |

---

## Business Modules

### 1. Identity & Access Management (IAM)

User registration, authentication (JWT), and role-based access control.

| Role | Description |
|------|-------------|
| `customer` | End consumer browsing and purchasing |
| `parent_vendor` | Business owner managing products and sub-vendors |
| `sub_vendor` | Artisan working under a parent vendor |
| `admin_super` | Full platform access |
| `admin_finance` | Financial reconciliation and payouts |
| `admin_operations` | Order and logistics management |
| `admin_support` | Customer and vendor support |

### 2. Vendor Ecosystem

Hierarchical vendor management supporting Egypt's artisan economy.

- **Parent Vendors**: Legally registered businesses or cooperatives
- **Sub-Vendors**: Individual artisans managed by a parent (e.g., "Aunt Nadia")
- **Cooperatives**: Legal umbrella entities for non-legalized vendors (5% cooperative fee)
- **Onboarding**: Document verification, commercial registration, tax ID validation
- **Reliability**: Strike system (3 strikes triggers account review), minimum 4.0 rating

### 3. Product Information Management (PIM)

Full product catalog with SKU management, category-based pricing, and image optimization.

- **SKU Format**: `PARENT-SUB-CAT-001`
- **Stock Modes**: Ready Stock or Made-to-Order (with lead times)
- **Image Handling**: Auto-compressed to under 150KB for 3G network compatibility
- **Geo-Fencing**: Products can be restricted to specific delivery zones
- **Price Deviation Detection**: Products priced +/-20% from category average are flagged for review

### 4. Order Management System (OMS)

Multi-vendor order processing with automatic shipment splitting.

- **Order Lifecycle**: Pending -> Accepted -> Ready to Ship -> In Transit -> Delivered
- **Multi-Vendor Splitting**: A single customer order is split into separate shipments per pickup location
- **Vendor SLA**: 24-hour acceptance window ("Accept or Die" rule)
- **Payment Methods**: COD, Fawry, Card, Wallet
- **Anti-Fraud**: COD cancellation limits, device fingerprinting, unreachable customer tracking

### 5. Customer Experience

Customer profiles, loyalty programs, subscriptions, and referrals.

- **Loyalty Points**: 1 point per 1 EGP spent
- **Referral Program**: "Give 50 EGP, Get 50 EGP" (unlocked on first delivery)
- **Subscriptions**: Weekly, biweekly, or monthly recurring deliveries
- **Multiple Addresses**: Home, Office, etc. with zone-based delivery assignment

### 6. Finance & Reconciliation

The financial backbone, powered by the 5-bucket waterfall split.

- **Vendor Wallets**: Real-time balance tracking (pending, available, withdrawn)
- **Escrow**: 14-day hold after delivery before funds become available
- **Ledger**: Complete transaction history per vendor with bucket-level detail
- **Payouts**: Weekly batch payouts with 500 EGP minimum threshold
- **COD Reconciliation**: 48-hour courier deposit cycle with 1% discrepancy threshold

### 7. Shipping & Logistics

Zone-based shipping with volumetric weight calculation and SmartCom integration.

- **Delivery Zones**: Greater Cairo, Alexandria, Delta, Upper Egypt, Canal Cities, Sinai
- **Weight Calculation**: `Chargeable Weight = MAX(Actual, (L x W x H) / 5000)`
- **SmartCom Buffer**: Logistics surcharge subsidizes part of the shipping fee for customers
- **Wasted Trip Fee**: 20 EGP penalty if vendor is not ready at pickup

### 8. Admin Dashboard

Platform-wide KPIs, analytics, and monitoring.

- **KPI Targets**: GMV, order volume, vendor activity, conversion rates
- **Operational Metrics**: Order acceptance rate (95%+), first-attempt delivery (85%+)
- **Financial Health**: COD ratio (target <65%), refund rate (target <12%)
- **System Uptime**: 99.9% target

---

## The Pricing Engine

The pricing engine is the most critical piece of business logic. It implements the **Additive Uplift Model** -- Arooba never reduces vendor prices; it adds layers on top.

### How It Works

Think of it like a layered cake:

```
Layer 1: Vendor sets their base price           (e.g., 500 EGP for a carpet)
Layer 2: Cooperative adds 5% (if non-legalized)  (e.g., + 25 EGP)
Layer 3: Arooba adds marketplace uplift           (e.g., 20% = + 100 EGP)
Layer 4: VAT is calculated on applicable portions (e.g., + 14% on each)
Layer 5: Shipping fee is added separately         (e.g., + 50 EGP delivery)
```

The customer sees **one final price**. Behind the scenes, the system knows exactly who gets what.

### The 5-Bucket Waterfall

Every payment is split into exactly 5 buckets for financial reconciliation:

| Bucket | Name | Description | Example (500 EGP carpet, non-legalized vendor) |
|--------|------|-------------|------------------------------------------------|
| **A** | Vendor Revenue | Vendor base price + parent uplift | 500.00 EGP |
| **B** | Vendor VAT | 14% of Bucket A (only if VAT registered) | 0.00 EGP (not registered) |
| **C** | Arooba Revenue | Cooperative fee + marketplace uplift + logistics surcharge | 25 + 100 + 10 = 135.00 EGP |
| **D** | Arooba VAT | 14% of Bucket C (always applies) | 18.90 EGP |
| **E** | Logistics Fee | Delivery fee paid to courier | 50.00 EGP |
| | **Total** | **Customer pays** | **703.90 EGP** |

### Uplift Matrix (Category-Based)

| Category | Min | Max | Default | Risk |
|----------|-----|-----|---------|------|
| Jewelry & Accessories | 15% | 18% | 15% | Low |
| Fashion & Apparel | 22% | 25% | 22% | High |
| Home Decor (Fragile) | 25% | 30% | 25% | High |
| Home Decor (Textiles) | 20% | 20% | 20% | Medium |
| Leather Goods | 20% | 20% | 20% | Medium |
| Beauty & Personal Care | 20% | 20% | 20% | Medium |
| Furniture & Woodwork | 15% | 15% | 15% | Medium |
| Food & Essentials | 10% | 15% | 12% | Low |

### Minimum Uplift Rule

A 20 EGP soap at 20% uplift yields only 4 EGP commission -- that does not cover bank transfer fees. The engine enforces:

- **Minimum fixed uplift**: 15 EGP on any product
- **Low-price threshold**: Products under 100 EGP receive a fixed 20 EGP markup
- **MVP flat rate**: 20% default across all categories (overridable per category)

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express / LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) / [JetBrains Rider](https://www.jetbrains.com/rider/)

### Clone and Build

```bash
cd backend
dotnet restore
dotnet build
```

### Database Setup

1. Update the connection string in `src/Arooba.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AroobaConnection": "Server=localhost;Database=AroobaMarketplace;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. Apply Entity Framework migrations:

```bash
dotnet ef database update \
  --project src/Arooba.Infrastructure \
  --startup-project src/Arooba.API
```

### Run the API

```bash
dotnet run --project src/Arooba.API
```

- API available at: `https://localhost:5001`
- Swagger UI at: `https://localhost:5001/swagger`

### Run with Docker

```bash
docker compose up --build
```

- API available at: `http://localhost:5000`
- SQL Server at: `localhost:1433`

### Run Tests

```bash
dotnet test
```

---

## API Endpoints

| Module | Method | Endpoint | Description |
|--------|--------|----------|-------------|
| **Auth** | `POST` | `/api/auth/register` | Register a new user |
| **Auth** | `POST` | `/api/auth/login` | Authenticate and receive JWT |
| **Auth** | `POST` | `/api/auth/refresh` | Refresh access token |
| **Vendors** | `GET` | `/api/vendors` | List all vendors (paginated) |
| **Vendors** | `GET` | `/api/vendors/{id}` | Get vendor by ID |
| **Vendors** | `POST` | `/api/vendors` | Register a new vendor |
| **Vendors** | `PUT` | `/api/vendors/{id}` | Update vendor details |
| **Vendors** | `GET` | `/api/vendors/{id}/sub-vendors` | List sub-vendors |
| **Vendors** | `POST` | `/api/vendors/{id}/sub-vendors` | Add sub-vendor |
| **Products** | `GET` | `/api/products` | List products (paginated, filterable) |
| **Products** | `GET` | `/api/products/{id}` | Get product by ID |
| **Products** | `POST` | `/api/products` | Create a new product |
| **Products** | `PUT` | `/api/products/{id}` | Update product details |
| **Products** | `PATCH` | `/api/products/{id}/status` | Change product status |
| **Orders** | `GET` | `/api/orders` | List orders (paginated) |
| **Orders** | `GET` | `/api/orders/{id}` | Get order by ID with shipments |
| **Orders** | `POST` | `/api/orders` | Create a new order |
| **Orders** | `PATCH` | `/api/orders/{id}/status` | Update order status |
| **Customers** | `GET` | `/api/customers` | List customers |
| **Customers** | `GET` | `/api/customers/{id}` | Get customer profile |
| **Customers** | `GET` | `/api/customers/{id}/addresses` | List customer addresses |
| **Customers** | `POST` | `/api/customers/{id}/addresses` | Add customer address |
| **Finance** | `GET` | `/api/finance/wallets/{vendorId}` | Get vendor wallet |
| **Finance** | `GET` | `/api/finance/ledger/{vendorId}` | Get vendor ledger entries |
| **Finance** | `GET` | `/api/finance/transactions/{orderId}` | Get order transaction split |
| **Finance** | `POST` | `/api/finance/payouts` | Trigger vendor payout |
| **Shipping** | `GET` | `/api/shipping/zones` | List delivery zones |
| **Shipping** | `GET` | `/api/shipping/rates` | List rate cards |
| **Shipping** | `POST` | `/api/shipping/calculate` | Calculate shipping fee |
| **Dashboard** | `GET` | `/api/dashboard/stats` | Get platform KPIs |
| **Dashboard** | `GET` | `/api/dashboard/gmv` | Get GMV time series |
| **Pricing** | `POST` | `/api/pricing/calculate` | Calculate full price breakdown |
| **Pricing** | `POST` | `/api/pricing/check-deviation` | Check price deviation |
| **Categories** | `GET` | `/api/categories` | List product categories |
| **Categories** | `GET` | `/api/categories/{id}` | Get category details |

---

## Project Structure

```
backend/
|-- Arooba.sln                              # Solution file
|-- Directory.Build.props                   # Shared MSBuild properties
|-- global.json                             # .NET SDK version pinning
|-- .editorconfig                           # Code style configuration
|-- .dockerignore                           # Docker build exclusions
|-- docker-compose.yml                      # Multi-container orchestration
|
|-- src/
|   |-- Arooba.Domain/                      # Core Domain Layer
|   |   |-- Arooba.Domain.csproj
|   |   |-- Common/
|   |   |   |-- BaseEntity.cs               # Base entity with ID, timestamps, events
|   |   |   |-- AuditableEntity.cs          # Auditable entity extension
|   |   |   |-- IDomainEvent.cs             # Domain event interface
|   |   |   |-- Result.cs                   # Result pattern for operations
|   |   |-- Entities/
|   |   |   |-- User.cs                     # IAM user entity
|   |   |   |-- ParentVendor.cs             # Parent vendor entity
|   |   |   |-- SubVendor.cs                # Sub-vendor entity
|   |   |   |-- Cooperative.cs              # Cooperative entity
|   |   |   |-- Product.cs                  # Product entity
|   |   |   |-- Category.cs                 # Product category entity
|   |   |   |-- Order.cs                    # Order aggregate root
|   |   |   |-- OrderItem.cs                # Order line item
|   |   |   |-- Shipment.cs                 # Shipment entity
|   |   |   |-- Customer.cs                 # Customer profile entity
|   |   |   |-- VendorWallet.cs             # Vendor wallet entity
|   |   |   |-- LedgerEntry.cs              # Financial ledger entry
|   |   |   |-- ShippingZone.cs             # Delivery zone entity
|   |   |   |-- RateCard.cs                 # Shipping rate card entity
|   |   |-- ValueObjects/
|   |   |   |-- Money.cs                    # Monetary amount with currency
|   |   |   |-- Address.cs                  # Structured address
|   |   |   |-- PhoneNumber.cs              # Egyptian phone number (+20)
|   |   |   |-- PricingBreakdown.cs         # 5-bucket pricing breakdown
|   |   |-- Enums/
|   |       |-- UserRole.cs
|   |       |-- VendorStatus.cs
|   |       |-- VendorType.cs
|   |       |-- ProductStatus.cs
|   |       |-- StockMode.cs
|   |       |-- OrderStatus.cs
|   |       |-- PaymentMethod.cs
|   |       |-- TransactionType.cs
|   |       |-- BalanceStatus.cs
|   |       |-- UpliftType.cs
|   |       |-- SubscriptionFrequency.cs
|   |
|   |-- Arooba.Application/                 # Application Layer
|   |   |-- Arooba.Application.csproj
|   |   |-- DependencyInjection.cs          # Service registration
|   |   |-- Common/
|   |   |   |-- Behaviors/
|   |   |   |   |-- ValidationBehavior.cs   # MediatR validation pipeline
|   |   |   |   |-- LoggingBehavior.cs      # MediatR logging pipeline
|   |   |   |-- Exceptions/
|   |   |   |   |-- BadRequestException.cs
|   |   |   |   |-- NotFoundException.cs
|   |   |   |   |-- ForbiddenAccessException.cs
|   |   |   |   |-- ValidationException.cs
|   |   |   |-- Interfaces/
|   |   |   |   |-- IAroobaDbContext.cs      # Database context interface
|   |   |   |   |-- IPricingEngine.cs        # Pricing engine interface
|   |   |   |   |-- ICurrentUserService.cs   # Current user accessor
|   |   |   |-- Mappings/
|   |   |       |-- MappingProfile.cs        # AutoMapper profile
|   |   |-- Vendors/
|   |   |   |-- Commands/                   # CreateVendor, UpdateVendor, etc.
|   |   |   |-- Queries/                    # GetVendors, GetVendorById, etc.
|   |   |-- Products/
|   |   |   |-- Commands/                   # CreateProduct, UpdateProduct, etc.
|   |   |   |-- Queries/                    # GetProducts, GetProductById, etc.
|   |   |-- Orders/
|   |   |   |-- Commands/                   # CreateOrder, UpdateOrderStatus, etc.
|   |   |   |-- Queries/                    # GetOrders, GetOrderById, etc.
|   |   |-- Pricing/
|   |   |   |-- Commands/                   # CalculatePrice, CheckDeviation
|   |   |-- Finance/
|   |   |   |-- Commands/                   # TriggerPayout
|   |   |   |-- Queries/                    # GetWallet, GetLedger
|   |   |-- Customers/
|   |   |   |-- Queries/                    # GetCustomers, GetCustomerById
|   |   |-- Shipping/
|   |   |   |-- Queries/                    # GetZones, CalculateShipping
|   |   |-- Dashboard/
|   |       |-- Queries/                    # GetStats, GetGmv
|   |
|   |-- Arooba.Infrastructure/              # Infrastructure Layer
|   |   |-- Arooba.Infrastructure.csproj
|   |   |-- DependencyInjection.cs          # Service registration
|   |   |-- Persistence/
|   |   |   |-- AroobaDbContext.cs           # EF Core DbContext
|   |   |   |-- Configurations/             # Entity type configurations
|   |   |   |-- Migrations/                 # EF Core migrations
|   |   |-- Repositories/
|   |   |   |-- VendorRepository.cs
|   |   |   |-- ProductRepository.cs
|   |   |   |-- OrderRepository.cs
|   |   |-- Services/
|   |       |-- PricingEngine.cs            # Core pricing calculation
|   |       |-- ShippingCalculator.cs       # Shipping fee calculator
|   |       |-- EscrowService.cs            # Escrow release logic
|   |
|   |-- Arooba.API/                         # Presentation Layer
|       |-- Arooba.API.csproj
|       |-- Program.cs                      # Application entry point
|       |-- Dockerfile                      # Multi-stage Docker build
|       |-- appsettings.json                # Configuration
|       |-- appsettings.Development.json    # Dev-specific overrides
|       |-- Controllers/
|       |   |-- AuthController.cs
|       |   |-- VendorsController.cs
|       |   |-- ProductsController.cs
|       |   |-- OrdersController.cs
|       |   |-- CustomersController.cs
|       |   |-- FinanceController.cs
|       |   |-- ShippingController.cs
|       |   |-- DashboardController.cs
|       |   |-- PricingController.cs
|       |   |-- CategoriesController.cs
|       |-- Middleware/
|       |   |-- ExceptionHandlingMiddleware.cs
|       |   |-- RequestLoggingMiddleware.cs
|       |-- Filters/
|           |-- ApiExceptionFilterAttribute.cs
|
|-- tests/
|   |-- Arooba.Domain.Tests/                # Domain unit tests
|   |   |-- Arooba.Domain.Tests.csproj
|   |   |-- ValueObjects/
|   |   |-- Entities/
|   |-- Arooba.Application.Tests/           # Application layer tests
|   |   |-- Arooba.Application.Tests.csproj
|   |   |-- Vendors/
|   |   |-- Products/
|   |   |-- Pricing/
|   |-- Arooba.API.Tests/                   # Integration tests
|       |-- Arooba.API.Tests.csproj
|       |-- Controllers/
|
|-- docs/
    |-- API-REFERENCE.md                    # Detailed API reference
    |-- ARCHITECTURE.md                     # Architecture documentation
```

---

## Configuration

### Application Settings (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "AroobaConnection": "Server=localhost;Database=AroobaMarketplace;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "<your-256-bit-secret>",
    "Issuer": "Arooba.API",
    "Audience": "Arooba.Client",
    "ExpirationMinutes": 60,
    "RefreshExpirationDays": 7
  },
  "PricingSettings": {
    "VatRate": 0.14,
    "MvpFlatRate": 0.20,
    "MinimumFixedUplift": 15,
    "LowPriceThreshold": 100,
    "LowPriceFixedMarkup": 20,
    "CooperativeFee": 0.05,
    "LogisticsSurcharge": 10
  },
  "EscrowSettings": {
    "HoldDays": 14,
    "MinimumPayoutThreshold": 500,
    "PayoutBatchDay": "weekly",
    "CodDepositCycleHours": 48,
    "CodDiscrepancyThreshold": 0.01
  },
  "ShippingSettings": {
    "VolumetricDivisor": 5000,
    "SmartComBaseRate": 50,
    "SubsidizedRate": 45,
    "WastedTripFee": 20
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__AroobaConnection` | SQL Server connection string | (see appsettings) |
| `JwtSettings__Secret` | JWT signing secret | (none -- required) |
| `JwtSettings__Issuer` | JWT token issuer | `Arooba.API` |
| `JwtSettings__Audience` | JWT token audience | `Arooba.Client` |
| `PricingSettings__VatRate` | Egypt VAT rate | `0.14` |
| `PricingSettings__MvpFlatRate` | Default marketplace uplift | `0.20` |
| `EscrowSettings__HoldDays` | Days before escrow release | `14` |

---

## Docker

### Quick Start

```bash
# Build and run all services
docker compose up --build

# Run in detached mode
docker compose up -d --build

# View logs
docker compose logs -f arooba-api

# Stop all services
docker compose down

# Stop and remove volumes (resets database)
docker compose down -v
```

### Services

| Service | Port | Description |
|---------|------|-------------|
| `arooba-api` | 5000 (HTTP), 5001 (HTTPS) | ASP.NET Core Web API |
| `sqlserver` | 1433 | Microsoft SQL Server 2022 Express |

---

## Testing

The test suite is organized by layer, mirroring the Clean Architecture structure.

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Arooba.Domain.Tests
dotnet test tests/Arooba.Application.Tests
dotnet test tests/Arooba.API.Tests

# Run with verbose output
dotnet test --verbosity normal

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

| Project | Scope | Tools |
|---------|-------|-------|
| `Arooba.Domain.Tests` | Value objects, entities, business rules | xUnit, FluentAssertions |
| `Arooba.Application.Tests` | CQRS handlers, validators, pricing logic | xUnit, FluentAssertions, Moq |
| `Arooba.API.Tests` | Controller integration tests | xUnit, WebApplicationFactory |

---

## License

**Proprietary** -- Arooba Marketplace. All rights reserved.

This software is the confidential and proprietary property of Arooba Marketplace (aroobh.com). Unauthorized copying, distribution, or use of this software, via any medium, is strictly prohibited.
