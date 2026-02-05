# Arooba Marketplace - Architecture Documentation

> Technical architecture documentation for the Arooba Marketplace .NET backend, covering Clean Architecture, CQRS pattern, module design, and key design decisions.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Clean Architecture Diagram](#clean-architecture-diagram)
- [Dependency Flow](#dependency-flow)
- [Layer Details](#layer-details)
- [CQRS Pattern](#cqrs-pattern)
- [Module Architecture](#module-architecture)
- [The Pricing Engine](#the-pricing-engine)
- [Domain Events](#domain-events)
- [Data Flow](#data-flow)
- [Security Architecture](#security-architecture)
- [Design Decisions](#design-decisions)

---

## Architecture Overview

The Arooba Marketplace backend is built on **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) with the **CQRS (Command Query Responsibility Segregation)** pattern. This architecture ensures:

1. **Independence from frameworks**: Business logic does not depend on ASP.NET Core or Entity Framework.
2. **Testability**: Every layer can be tested in isolation using mocks and stubs.
3. **Independence from the database**: The domain layer has no knowledge of SQL Server or EF Core.
4. **Independence from the UI**: The API layer can be swapped (REST, gRPC, GraphQL) without touching business logic.

---

## Clean Architecture Diagram

```
+=========================================================================+
|                                                                         |
|                         EXTERNAL SYSTEMS                                |
|              (Clients, Mobile Apps, Admin Portal)                       |
|                                                                         |
+====================================+====================================+
                                     |
                                     | HTTP / HTTPS
                                     |
+====================================v====================================+
|                                                                         |
|                     PRESENTATION LAYER (Arooba.API)                     |
|                                                                         |
|   +-------------+  +----------------+  +------------------+             |
|   | Controllers |  |  Middleware     |  |  Filters         |             |
|   |             |  |                |  |                  |             |
|   | Auth        |  | ExceptionHndlr |  | ApiException     |             |
|   | Vendors     |  | RequestLogger  |  | Filter           |             |
|   | Products    |  |                |  |                  |             |
|   | Orders      |  +----------------+  +------------------+             |
|   | Customers   |                                                       |
|   | Finance     |  +----------------+  +------------------+             |
|   | Shipping    |  | Program.cs     |  | appsettings.json |             |
|   | Dashboard   |  | (Composition   |  | (Configuration)  |             |
|   | Pricing     |  |  Root)         |  |                  |             |
|   | Categories  |  +----------------+  +------------------+             |
|   +------+------+                                                       |
|          |                                                              |
+====================================+====================================+
           |  Depends on (via DI)    |
           v                         v
+====================================+====================================+
|                                                                         |
|                   APPLICATION LAYER (Arooba.Application)                |
|                                                                         |
|   +-------------------------------+  +-------------------------------+  |
|   |        COMMANDS (Write)       |  |        QUERIES (Read)         |  |
|   |                               |  |                               |  |
|   | CreateVendorCommand           |  | GetVendorsQuery               |  |
|   | CreateVendorHandler           |  | GetVendorsHandler             |  |
|   | CreateVendorValidator         |  |                               |  |
|   |                               |  | GetProductsQuery              |  |
|   | CreateProductCommand          |  | GetProductsHandler            |  |
|   | CreateProductHandler          |  |                               |  |
|   | CreateProductValidator        |  | GetOrdersQuery                |  |
|   |                               |  | GetOrdersHandler              |  |
|   | CreateOrderCommand            |  |                               |  |
|   | CreateOrderHandler            |  | GetWalletQuery                |  |
|   | CreateOrderValidator          |  | GetWalletHandler              |  |
|   +-------------------------------+  +-------------------------------+  |
|                                                                         |
|   +-------------------------------+  +-------------------------------+  |
|   |    PIPELINE BEHAVIORS         |  |     COMMON                    |  |
|   |                               |  |                               |  |
|   | ValidationBehavior<TReq,TRes> |  | Interfaces:                   |  |
|   | LoggingBehavior<TReq,TRes>    |  |   IAroobaDbContext             |  |
|   |                               |  |   IPricingEngine               |  |
|   +-------------------------------+  |   ICurrentUserService          |  |
|                                      |                               |  |
|   +-------------------------------+  | Exceptions:                   |  |
|   |    DTOs / MAPPING             |  |   NotFoundException            |  |
|   |                               |  |   ValidationException          |  |
|   | VendorDto, ProductDto, etc.   |  |   BadRequestException          |  |
|   | MappingProfile (AutoMapper)   |  |   ForbiddenAccessException     |  |
|   +-------------------------------+  +-------------------------------+  |
|                                                                         |
+====================================+====================================+
           |  Depends on             |
           v                         v
+==================+     +===========================================+
|                  |     |                                           |
|  DOMAIN LAYER    |     |  INFRASTRUCTURE LAYER                    |
| (Arooba.Domain)  |     |  (Arooba.Infrastructure)                 |
|                  |     |                                           |
| +==============+ |     | +-------------------------------------+  |
| | ENTITIES     | |     | | PERSISTENCE                         |  |
| |              | |     | |                                     |  |
| | User         | |     | | AroobaDbContext : IAroobaDbContext   |  |
| | ParentVendor | |     | | Entity Type Configurations          |  |
| | SubVendor    | |     | | EF Core Migrations                  |  |
| | Cooperative  | |     | +-------------------------------------+  |
| | Product      | |     |                                           |
| | Category     | |     | +-------------------------------------+  |
| | Order        | |     | | REPOSITORIES                        |  |
| | OrderItem    | |     | |                                     |  |
| | Shipment     | |     | | VendorRepository                    |  |
| | Customer     | |     | | ProductRepository                   |  |
| | VendorWallet | |     | | OrderRepository                     |  |
| | LedgerEntry  | |     | +-------------------------------------+  |
| | ShippingZone | |     |                                           |
| | RateCard     | |     | +-------------------------------------+  |
| +==============+ |     | | SERVICES                            |  |
|                  |     | |                                     |  |
| +==============+ |     | | PricingEngine : IPricingEngine      |  |
| | VALUE OBJECTS| |     | | ShippingCalculator                  |  |
| |              | |     | | EscrowService                       |  |
| | Money        | |     | +-------------------------------------+  |
| | Address      | |     |                                           |
| | PhoneNumber  | |     |               +-----+                    |
| | PricingBrkdwn| |     |               | SQL |                    |
| +==============+ |     |               |Server|                    |
|                  |     |               +-----+                    |
| +==============+ |     |                                           |
| | ENUMS        | |     +===========================================+
| |              | |
| | UserRole     | |
| | VendorStatus | |
| | VendorType   | |
| | ProductStatus| |
| | OrderStatus  | |
| | PaymentMethod| |
| | StockMode    | |
| | etc.         | |
| +==============+ |
|                  |
| +==============+ |
| | COMMON       | |
| |              | |
| | BaseEntity   | |
| | AuditableEnt.| |
| | IDomainEvent | |
| | Result<T>    | |
| +==============+ |
|                  |
+==================+
```

---

## Dependency Flow

Dependencies flow **inward only**. Outer layers depend on inner layers, never the reverse.

```
                    Depends On
  Arooba.API ────────────────────> Arooba.Application
       |                                  |
       |                                  |  Depends On
       |                                  +──────────────> Arooba.Domain
       |                                                        ^
       |  Depends On                                            |
       +────────────────────> Arooba.Infrastructure ────────────+
                                                     Depends On
```

### Dependency Rules

| Layer | Can Depend On | Cannot Depend On |
|-------|--------------|-----------------|
| `Arooba.Domain` | Nothing (zero external dependencies except MediatR.Contracts for `INotification`) | Application, Infrastructure, API |
| `Arooba.Application` | Domain | Infrastructure, API |
| `Arooba.Infrastructure` | Application, Domain | API |
| `Arooba.API` | Application, Infrastructure (for DI registration only) | -- |

### Why This Matters

- **Domain isolation**: Business rules in `Arooba.Domain` can never be corrupted by framework changes.
- **Swappable infrastructure**: Replace SQL Server with PostgreSQL by changing only `Arooba.Infrastructure`.
- **Testable application logic**: Mock `IAroobaDbContext` and `IPricingEngine` to test handlers in isolation.

---

## Layer Details

### Arooba.Domain

The innermost layer. Contains enterprise-wide business rules that would exist even without the software.

**Key Components**:

- **BaseEntity**: All entities inherit from this. Provides `Id` (Guid), `CreatedAt`, `UpdatedAt`, and domain event support.
- **Value Objects**: Immutable types that represent concepts with no identity:
  - `Money`: Amount + currency with arithmetic operators. Enforces same-currency operations.
  - `PricingBreakdown`: The 5-bucket waterfall calculation result.
  - `Address`: Structured Egyptian address with zone reference.
  - `PhoneNumber`: Egyptian phone number with +20 prefix validation.
- **Domain Events**: Markers implementing `IDomainEvent` (which extends MediatR's `INotification`). Examples: `VendorApprovedEvent`, `OrderPlacedEvent`, `EscrowReleasedEvent`.
- **Result Pattern**: `Result<T>` allows operations to return success/failure without exceptions for expected business failures.

**NuGet Dependencies**: Only `MediatR.Contracts` (for `INotification` interface).

---

### Arooba.Application

The orchestration layer. Contains application-specific business logic as CQRS handlers.

**Key Components**:

- **Commands**: Write operations (e.g., `CreateVendorCommand`, `CreateOrderCommand`)
- **Queries**: Read operations (e.g., `GetProductsQuery`, `GetWalletQuery`)
- **Validators**: FluentValidation validators for every command
- **Pipeline Behaviors**:
  - `ValidationBehavior<TRequest, TResponse>`: Automatically validates all commands before reaching the handler
  - `LoggingBehavior<TRequest, TResponse>`: Logs request/response for observability
- **Interfaces**: Abstractions for infrastructure concerns (`IAroobaDbContext`, `IPricingEngine`)
- **DTOs**: Data Transfer Objects for API responses
- **AutoMapper Profiles**: Entity-to-DTO mapping configuration

**NuGet Dependencies**: MediatR, FluentValidation, AutoMapper, EF Core (for `IQueryable` only).

---

### Arooba.Infrastructure

The implementation layer. Provides concrete implementations for interfaces defined in Application.

**Key Components**:

- **AroobaDbContext**: EF Core DbContext implementing `IAroobaDbContext`. Configures all entity mappings, relationships, and indexes.
- **Entity Type Configurations**: Fluent API configurations for each entity (table names, column types, relationships, indexes).
- **Repositories**: Encapsulate complex query logic beyond simple CRUD.
- **PricingEngine**: Implements `IPricingEngine`. Contains the core Additive Uplift Model calculation.
- **ShippingCalculator**: Zone-based weight calculation with volumetric weight support.
- **EscrowService**: 14-day hold logic and release determination.

**NuGet Dependencies**: EF Core, EF Core SqlServer, EF Core Tools/Design.

---

### Arooba.API

The outermost layer. Serves as the composition root and HTTP entry point.

**Key Components**:

- **Program.cs**: Application startup, DI container configuration, middleware pipeline.
- **Controllers**: Thin controllers that delegate to MediatR. Each controller method sends a command/query and returns the result.
- **Middleware**:
  - `ExceptionHandlingMiddleware`: Catches unhandled exceptions, maps to RFC 7807 Problem Details.
  - `RequestLoggingMiddleware`: Logs HTTP method, path, status code, and duration.
- **Swagger/OpenAPI**: Auto-generated API documentation with JWT authentication support.

**NuGet Dependencies**: ASP.NET Core JWT Bearer, Swashbuckle.

---

## CQRS Pattern

Every API operation is modeled as either a **Command** (write) or **Query** (read), handled by MediatR.

### Command Flow (Example: Create Order)

```
                                    MediatR Pipeline
                                    ================

  Controller                  Validation              Logging               Handler
  ==========                  ==========              =======               =======
      |                           |                      |                      |
      |  Send(CreateOrderCmd) --> |                      |                      |
      |                           | Validate(cmd)        |                      |
      |                           | - Items required     |                      |
      |                           | - Address required   |                      |
      |                           | - Zone must exist    |                      |
      |                           |                      |                      |
      |                           | [Valid] -----------> |                      |
      |                           |                      | Log("Processing      |
      |                           |                      |   CreateOrder...")    |
      |                           |                      |                      |
      |                           |                      | [Proceed] ---------> |
      |                           |                      |                      |
      |                           |                      |                      | 1. Load products
      |                           |                      |                      | 2. Calculate pricing
      |                           |                      |                      |    (IPricingEngine)
      |                           |                      |                      | 3. Split shipments
      |                           |                      |                      | 4. Calculate 5 buckets
      |                           |                      |                      | 5. Save to DB
      |                           |                      |                      | 6. Raise OrderPlaced
      |                           |                      |                      |    domain event
      |                           |                      |                      |
      | <------- Result<OrderDto> +----- <Result> -------+------ <Result> -----+
      |
      | Return 201 Created
```

### Query Flow (Example: Get Vendors)

```
  Controller                  Logging               Handler
  ==========                  =======               =======
      |                          |                      |
      |  Send(GetVendorsQry) --> |                      |
      |                          | Log("Querying        |
      |                          |   vendors...")        |
      |                          |                      |
      |                          | [Proceed] ---------> |
      |                          |                      |
      |                          |                      | 1. Build IQueryable
      |                          |                      | 2. Apply filters
      |                          |                      | 3. Apply sorting
      |                          |                      | 4. Apply pagination
      |                          |                      | 5. Project to DTO
      |                          |                      |
      | <--- PaginatedList<Dto>--+------ <Result> -----+
      |
      | Return 200 OK
```

### Pipeline Behaviors

MediatR pipeline behaviors wrap every request, similar to ASP.NET middleware but at the application layer:

```
Request --> [ValidationBehavior] --> [LoggingBehavior] --> [Handler] --> Response
                    |                        |
                    | Throws                 | Logs request
                    | ValidationException    | and response
                    | if invalid             | details
```

---

## Module Architecture

Each business module follows the same internal structure:

```
Module/
|-- Commands/
|   |-- Create{Entity}/
|   |   |-- Create{Entity}Command.cs       # MediatR IRequest<Result<Dto>>
|   |   |-- Create{Entity}Handler.cs        # MediatR IRequestHandler
|   |   |-- Create{Entity}Validator.cs      # FluentValidation AbstractValidator
|   |-- Update{Entity}/
|   |-- Delete{Entity}/
|
|-- Queries/
|   |-- Get{Entity}ById/
|   |   |-- Get{Entity}ByIdQuery.cs         # MediatR IRequest<Result<Dto>>
|   |   |-- Get{Entity}ByIdHandler.cs       # MediatR IRequestHandler
|   |-- Get{Entities}/
|       |-- Get{Entities}Query.cs           # With pagination params
|       |-- Get{Entities}Handler.cs
|
|-- Dtos/
|   |-- {Entity}Dto.cs                      # Response DTO
|   |-- {Entity}BriefDto.cs                 # List item DTO
|
|-- EventHandlers/
    |-- {Entity}CreatedEventHandler.cs      # Domain event handler
```

---

## The Pricing Engine

The pricing engine is the most business-critical component. It implements the **Additive Uplift Model**.

### Calculation Flow

```
                    INPUT
                    =====
                    vendorBasePrice: 500 EGP
                    categoryId: "home-decor-textiles"
                    isVendorVatRegistered: false
                    isNonLegalizedVendor: true
                    parentUpliftType: "fixed"
                    parentUpliftValue: 30
                         |
                         v
              +-------------------------+
              | Step 1: Cooperative Fee |
              | (non-legalized only)    |
              | 500 x 0.05 = 25 EGP    |
              +----------+--------------+
                         |
                         v
              +-------------------------+
              | Step 2: Parent Uplift   |
              | (sub-vendor products)   |
              | Fixed: 30 EGP           |
              +----------+--------------+
                         |
                         v
              +-------------------------+
              | Step 3: Marketplace     |
              | Uplift (category-based) |
              | 525 x 0.20 = 105 EGP   |
              |                         |
              | Check: 105 >= 15 min?   |
              | YES -> use 105          |
              +----------+--------------+
                         |
                         v
              +-------------------------+
              | Step 4: Logistics       |
              | Surcharge               |
              | Fixed: 10 EGP           |
              +----------+--------------+
                         |
                         v
     +-------------------+-------------------+
     |                                       |
     v                                       v
+----+------+                         +------+-------+
| BUCKET A  |                         | BUCKET C     |
| Vendor    |                         | Arooba       |
| Revenue   |                         | Revenue      |
|           |                         |              |
| base +    |                         | coop +       |
| parent    |                         | uplift +     |
| uplift    |                         | logistics    |
|           |                         |              |
| 500 + 30  |                         | 25 + 105 +   |
| = 530 EGP |                         | 10 = 140 EGP |
+----+------+                         +------+-------+
     |                                       |
     v                                       v
+----+------+                         +------+-------+
| BUCKET B  |                         | BUCKET D     |
| Vendor VAT|                         | Arooba VAT   |
|           |                         |              |
| NOT VAT   |                         | ALWAYS       |
| registered|                         | applies      |
| = 0 EGP   |                         | 140 x 0.14   |
|           |                         | = 19.60 EGP  |
+-----------+                         +--------------+
     |                                       |
     +-------------------+-------------------+
                         |
                         v
              +-------------------------+
              | FINAL PRICE             |
              | (excl. Bucket E)        |
              |                         |
              | 530 + 0 + 140 + 19.60   |
              | = 689.60 EGP            |
              +-------------------------+
                         |
                         v
              +-------------------------+
              | BUCKET E (Delivery)     |
              | Calculated separately   |
              | per shipment using      |
              | zone + weight model     |
              +-------------------------+
```

### Minimum Uplift Protection

```
                    Input: 50 EGP soap
                         |
                         v
              +-------------------------+
              | Percentage Uplift       |
              | 50 x 0.20 = 10 EGP     |
              +----------+--------------+
                         |
                         v
              +-------------------------+
              | Check: 10 < 15 min?     |
              | YES -> use 15 EGP       |
              +----------+--------------+
                         |
                         v
              +-------------------------+
              | Check: 50 < 100 low     |
              | price threshold?        |
              | YES -> use MAX(15, 20)  |
              | = 20 EGP fixed markup   |
              +-------------------------+
```

---

## Domain Events

Domain events decouple side effects from core operations. When an entity raises an event, handlers in other modules react asynchronously.

### Event Flow

```
  Order Aggregate                MediatR                Event Handlers
  ===============                =======                ==============
       |                            |                         |
       | AddDomainEvent(            |                         |
       |   OrderPlacedEvent)        |                         |
       |                            |                         |
       | SaveChanges() -----------> |                         |
       |                            |                         |
       | (DbContext intercepts      |                         |
       |  and dispatches events)    |                         |
       |                            | Publish(OrderPlaced) -> |
       |                            |                         |
       |                            |     +----> UpdateVendorOrderCount
       |                            |     |
       |                            |     +----> CreateLedgerEntries
       |                            |     |
       |                            |     +----> SendVendorNotification
       |                            |     |
       |                            |     +----> UpdateDashboardStats
       |                            |
```

### Key Domain Events

| Event | Raised When | Handlers |
|-------|-------------|----------|
| `VendorRegisteredEvent` | New vendor created | Send welcome notification, create wallet |
| `VendorApprovedEvent` | Admin approves vendor | Activate products, notify vendor |
| `ProductCreatedEvent` | New product listed | Calculate pricing, validate images |
| `OrderPlacedEvent` | Customer places order | Create ledger entries, notify vendor(s), start SLA timer |
| `OrderDeliveredEvent` | Delivery confirmed | Start 14-day escrow timer, update wallet |
| `EscrowReleasedEvent` | 14-day hold expires | Move funds to available balance, check payout threshold |
| `PayoutProcessedEvent` | Vendor payout sent | Update wallet, create ledger entry |
| `ReliabilityStrikeEvent` | Vendor misses SLA | Increment strikes, check for suspension |

---

## Data Flow

### End-to-End Order Flow

```
  Customer App          API Gateway          Backend              SQL Server
  ============          ===========          =======              ==========
       |                     |                  |                      |
       | POST /api/orders -> |                  |                      |
       |                     | -> AuthMiddleware |                      |
       |                     |    (JWT verify)   |                      |
       |                     |                  |                      |
       |                     | -> Controller    |                      |
       |                     |    Send(Cmd) --> |                      |
       |                     |                  |                      |
       |                     |                  | Validate             |
       |                     |                  |   |                  |
       |                     |                  | Load Products -----> |
       |                     |                  |   <--- Products ---- |
       |                     |                  |                      |
       |                     |                  | PricingEngine        |
       |                     |                  |   .Calculate()       |
       |                     |                  |   (per item)         |
       |                     |                  |                      |
       |                     |                  | Split into           |
       |                     |                  | Shipments            |
       |                     |                  |   (by pickup loc)    |
       |                     |                  |                      |
       |                     |                  | Calculate            |
       |                     |                  | ShippingFee          |
       |                     |                  |   (per shipment)     |
       |                     |                  |                      |
       |                     |                  | Save Order --------> |
       |                     |                  | Save OrderItems ---> |
       |                     |                  | Save Shipments ----> |
       |                     |                  | Save TxnSplits ----> |
       |                     |                  |                      |
       |                     |                  | Dispatch Events      |
       |                     |                  |   OrderPlacedEvent   |
       |                     |                  |     |                |
       |                     |                  |     +-> Update Wallet|
       |                     |                  |     +-> Ledger Entry |
       |                     |                  |     +-> Notification |
       |                     |                  |                      |
       |                     | <-- 201 Created  |                      |
       | <-- Order Response  |                  |                      |
       |                     |                  |                      |
```

---

## Security Architecture

### Authentication Flow

```
  Client                  API                     JWT Service
  ======                  ===                     ===========
     |                     |                          |
     | POST /auth/login -> |                          |
     |   (mobile + pass)   |                          |
     |                     | Verify credentials       |
     |                     | Generate JWT ----------> |
     |                     |                          |
     |                     |   Claims:                |
     |                     |   - sub: userId          |
     |                     |   - role: parent_vendor  |
     |                     |   - vendorId: v-123      |
     |                     |   - exp: +60min          |
     |                     |                          |
     |                     | <-- accessToken -------- |
     |                     | <-- refreshToken         |
     | <-- Tokens          |                          |
     |                     |                          |
     | GET /api/vendors -> |                          |
     |   Authorization:    |                          |
     |   Bearer <token>    |                          |
     |                     | [Authorize(Roles =       |
     |                     |   "parent_vendor,admin")]|
     |                     |                          |
     |                     | Extract claims           |
     |                     | ICurrentUserService      |
     |                     |   .UserId                |
     |                     |   .Role                  |
     |                     |                          |
```

### Authorization Matrix

| Endpoint | Customer | Parent Vendor | Sub Vendor | Admin |
|----------|----------|---------------|------------|-------|
| GET /api/products | Read all | Read own | Read own | Read all |
| POST /api/products | -- | Create | -- | Create |
| GET /api/orders | Own orders | Own orders | -- | All orders |
| POST /api/orders | Create | -- | -- | Create |
| GET /api/finance/wallets | -- | Own wallet | -- | All wallets |
| GET /api/dashboard/stats | -- | -- | -- | Full access |
| POST /api/vendors | -- | Register | -- | Register |

---

## Design Decisions

### 1. Why Clean Architecture?

**Decision**: Use Clean Architecture over simpler N-Tier or vertical slice.

**Rationale**: Arooba Marketplace has complex business logic (pricing engine, 5-bucket waterfall, escrow) that must be isolated from infrastructure concerns. The pricing engine, in particular, contains business rules that should be testable without a database. Clean Architecture ensures the domain model remains pure and the application can evolve independently at each layer.

**Trade-off**: More boilerplate code and indirection. Accepted because business logic complexity justifies the structure.

---

### 2. Why CQRS with MediatR?

**Decision**: Use MediatR for CQRS instead of traditional service classes.

**Rationale**:
- Each operation is a single class, making it easy to locate and reason about.
- Pipeline behaviors provide cross-cutting concerns (validation, logging) without repetition.
- Commands and queries have different optimization needs (writes need validation; reads need projection).
- Domain events are naturally dispatched through the same mediator.

**Trade-off**: Extra classes per operation. Accepted because it enforces single responsibility and simplifies testing.

---

### 3. Why `decimal` for Money Instead of `double`?

**Decision**: All financial calculations use `decimal` via the `Money` value object.

**Rationale**: Floating-point arithmetic (`double`) introduces rounding errors that are unacceptable in financial calculations. A 0.01 EGP discrepancy across thousands of transactions compounds into real losses. The `Money` value object enforces same-currency operations and prevents accidental mixing of currencies.

---

### 4. Why Record Types for Value Objects?

**Decision**: Value objects (`Money`, `PricingBreakdown`) are C# `record` types.

**Rationale**: Records provide value-based equality, immutability, and `with` expressions. A `Money(100, "EGP")` should equal another `Money(100, "EGP")` regardless of reference -- records guarantee this.

---

### 5. Why 14-Day Escrow Hold?

**Decision**: Funds are held for 14 days after delivery before becoming available.

**Rationale**: Egypt's e-commerce landscape has a high return rate. If vendors are paid immediately and the customer returns the product, Arooba must recover the payout -- which is operationally expensive. The 14-day hold is industry standard (Amazon, Noon use similar holds) and provides a buffer for returns, disputes, and fraud investigation.

---

### 6. Why Entity Framework Core Over Dapper?

**Decision**: Use EF Core as the primary ORM.

**Rationale**:
- Change tracking simplifies domain event dispatch (events collected during SaveChanges).
- Migrations provide database versioning aligned with code changes.
- LINQ-based queries compose well with the CQRS query pattern.
- Interceptors enable audit trail automation (`CreatedAt`, `UpdatedAt`).

**Mitigation**: For performance-critical read queries, raw SQL or compiled queries can be used within the infrastructure layer without affecting other layers.

---

### 7. Why Cooperative Fee as a Separate Line Item?

**Decision**: The 5% cooperative fee is tracked as a separate field in `PricingBreakdown`, not merged into the marketplace uplift.

**Rationale**: Cooperatives are legal entities that provide tax umbrellas for non-legalized vendors. Their fee must be separately tracked for:
- Tax reporting (cooperative revenue vs. Arooba revenue)
- Cooperative payout reconciliation
- Vendor transparency (vendors see exactly what each party takes)

---

### 8. Why Multi-Stage Docker Build?

**Decision**: Use a 4-stage Dockerfile (restore, build, test, runtime).

**Rationale**:
- **Restore stage**: Caches NuGet packages. Only re-runs if `.csproj` files change.
- **Build stage**: Compiles and publishes. Cached when source is unchanged.
- **Test stage**: Optional (skippable via `--target build`). Ensures tests pass before deployment.
- **Runtime stage**: Uses the slim `aspnet` image (~220MB vs. ~900MB SDK). Runs as non-root user for security.

---

### 9. Why Zone-Based Shipping Over Flat Rate?

**Decision**: Shipping fees are calculated per zone-to-zone rate card, not a flat rate.

**Rationale**: Egypt spans a vast geography. Shipping from Cairo to Alexandria (1-2 days) costs significantly less than Cairo to Sinai (5-7 days). A flat rate would either overcharge nearby customers or subsidize distant ones. Zone-based rates allow fair pricing while the SmartCom buffer provides some subsidization.

---

### 10. Why the Result Pattern Over Exceptions?

**Decision**: Application layer operations return `Result<T>` for expected failures; exceptions are reserved for unexpected failures.

**Rationale**:
- "Vendor not found" is an expected scenario, not an exception.
- `Result<T>` forces callers to handle both success and failure paths explicitly.
- Exception-based flow control is expensive and obscures business logic.
- Unexpected failures (database down, null reference) still throw and are caught by middleware.

---

## Technology Compatibility Matrix

| Component | Minimum Version | Recommended |
|-----------|----------------|-------------|
| .NET SDK | 8.0.0 | 8.0.x (latest patch) |
| SQL Server | 2019 | 2022 |
| Docker Engine | 20.10+ | 24.0+ |
| Docker Compose | 2.0+ | 2.24+ |
| Visual Studio | 2022 17.8+ | Latest |
| VS Code | 1.85+ | Latest + C# Dev Kit |
| JetBrains Rider | 2023.3+ | Latest |
