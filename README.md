# Gordon’s Azure .NET API Demo

A production-style ASP.NET Core Web API showcasing clean architecture, Azure AD auth, EF Core with SQL Server, MediatR, FluentValidation, AutoMapper, Serilog, and Swagger (OpenAPI) — plus CI/CD to Azure App Service via GitHub Actions (OIDC).

> **Highlights**
>
> * Clean layering: **Presentation → Application → Domain → Infrastructure → Persistence**
> * Two EF Core DbContexts with independent migrations history (App & Logs)
> * MediatR pipeline behaviors (Serilog enrichment + fluent validation)
> * Global exception handling returning a consistent `ApiResponse<T>`
> * End-to-end **CorrelationId** in headers, logs, and response bodies
> * Enum persistence friendly to SQL + JSON serialization as strings
> * Swagger UI with **OAuth2 / PKCE** (Azure AD / Entra ID)
> * GitHub Actions pipeline: build, migrate, deploy — no password secrets

---

## Table of Contents

* [Architecture](#architecture)
* [Tech Stack](#tech-stack)
* [Solution Layout](#solution-layout)
* [Getting Started](#getting-started)
* [Configuration](#configuration)
* [Running & Swagger](#running--swagger)
* [API Conventions](#api-conventions)
* [Data & EF Core](#data--ef-core)
* [Authentication](#authentication)
* [Logging & Observability](#logging--observability)
* [CI/CD (GitHub → Azure)](#cicd-github--azure)
* [Common CLI Commands](#common-cli-commands)
* [Sample Endpoints](#sample-endpoints)
* [Roadmap](#roadmap)
* [License](#license)

---

## Architecture

```mermaid
flowchart TD
    A[Client / Swagger UI] --> B[ASP.NET Core Middleware]
    B --> C[Authentication & Authorization]
    B --> D[RequestLogMiddleware]
    B --> E[Controllers (Application)]
    E --> F[MediatR Handlers & Behaviors]
    F --> G[FluentValidation]
    F --> H[AutoMapper]
    F --> I[Repositories (Infrastructure)]
    I --> J[EF Core (Persistence)]
    J --> K[(SQL Server: AppDb)]
    D --> L[(SQL Server: RequestLogs)]
    B --> M[Global Exception Handler → ApiResponse<T>]
```

---

## Tech Stack

**Runtime / Framework**

* ASP.NET Core (.NET 8)

**Core NuGet packages** (pinned as in project)

* AutoMapper **12.\*** + `AutoMapper.Extensions.Microsoft.DependencyInjection` **12.\***
* FluentValidation **12.0.0**, FluentValidation.AspNetCore **11.3.1**
* MediatR **12.1.1**
* Microsoft.Data.SqlClient **6.0.2**
* EF Core: `Microsoft.EntityFrameworkCore.SqlServer` **9.0.7**, `Microsoft.EntityFrameworkCore.Design` **9.0.7**
* Microsoft.Identity.Web **3.10.0**
* Serilog: `Serilog.AspNetCore` **9.0.0**, enrichers, console/file/MSSQL sinks
* Swashbuckle.AspNetCore **9.0.3**

---

## Solution Layout

```
src/
  RESTfulAPI/
    Application/
      Behaviors/                 # MediatR pipeline behaviors (Serilog enrich, Validation)
      Controllers/               # API controllers
      DTOs/                      # Request/response DTOs
      Mappers/                   # AutoMapper profiles
      Requests/                  # MediatR request types
      Validators/                # FluentValidation validators
      Filters/                   # Swagger operation filters (AuthorizeCheckOperationFilter)
    Domain/
      Entities/                  # EF entities (Patients, Payments, etc.)
      Enums/                     # Gender, PaymentStatus, ...
    Infrastructure/
      Auth/                      # Dev auth handler
      HostedServices/            # DbWarmupService
      Repositories/              # Implementations
      Repositories/Interfaces/   # Abstractions
    Persistence/
      Converters/                # EF value converters (Gender <-> 'M'/'F'/'O')
      AppDbContext.cs
      LogDbContext.cs
    Presentation/
      Common/                    # ApiResponse<T>, ApiResponseCorrelation
      Conventions/               # Route prefix + slugify conventions
      Middleware/                # RequestLogMiddleware, ExceptionHandlingExtensions
    Program.cs
.github/
  workflows/
    ci-cd-api.yml               # Build, migrate, deploy to Azure
README.md                        # ← Put this at repo root
```

---

## Getting Started

### Prerequisites

* .NET 8 SDK
* SQL Server (LocalDB or Azure SQL)
* Azure subscription (if using the provided CI/CD to App Service)

### Restore, build, run

```bash
dotnet restore
dotnet build
dotnet run --project ./src/RESTfulAPI/RESTfulAPI.csproj
```

Navigate to: **[https://localhost:5001/swagger](https://localhost:5001/swagger)**

> In **Development**, a **Dev** authentication scheme auto-signs you in so you can call `[Authorize]` endpoints without Azure AD setup.

---

## Configuration

Adjust `appsettings*.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "AzureSql": "Server=tcp:<server>.database.windows.net;Initial Catalog=<db>;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connect Timeout=60"
  },

  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain":  "<tenant>.onmicrosoft.com",
    "TenantId":"<tenant-guid>",
    "ClientId":"<api-app-client-id>",
    "Scopes":  "api://<api-app-client-id>/access_as_user"
  },

  "Swagger": {
    "ClientId": "<public-client-id-for-swagger>"
  }
}
```

* Two contexts point to the same database by default:

  * `AppDbContext` (business data)
  * `LogDbContext` (request logging)
* Each context uses a **separate migrations history table**.

---

## Running & Swagger

* **Swagger UI** is enabled in Dev and (optionally) Prod.
* In Prod, click **Authorize** and sign in with Azure AD (Authorization Code + PKCE).
* CORS policy `Spa` allows: `http://localhost:5000`, `https://localhost:5001`.

---

## API Conventions

### Response Envelope

All endpoints return:

```json
{
  "success": true,
  "status": 200,
  "message": "The request was successful.",
  "data": { /* payload */ },
  "errors": null,
  "correlationId": "d3ed0b8a-...-....",
  "timestamp": "2025-08-20T18:30:19.025Z"
}
```

### Correlation

* **Headers**: `X-Correlation-ID` (stable per request), `X-Request-ID`
* **Body**: `correlationId` in `ApiResponse<T>`
* **Logs/DB**: the same CorrelationId is written to Serilog and `dbo.RequestLogs` to drive “find it everywhere with one id”.

---

## Data & EF Core

* **Entities**: `Patient`, `Payment`, `PaymentMethod`, `Department`, `InsuranceProvider`, etc.
* **Enums**

  * `Gender` stored as `'M'/'F'/'O'` via a value converter; serialized as **strings** in JSON.
  * `PaymentStatus` stored as `TINYINT`; serialized as **strings** in JSON.
* **Interceptors**

  * `NameNormalizationInterceptor` capitalizes `FirstName`/`LastName` on `SaveChanges`.
* **MediatR + AutoMapper**

  * DTOs live under `Application/DTOs`, mappings under `Application/Mappers`.
* **Validation**

  * FluentValidation enforces DTO rules (e.g., MRN pattern, casing expectations, ranges).

### Migrations

```bash
# Add migrations
dotnet ef migrations add Init_App --context AppDbContext
dotnet ef migrations add Init_Log --context LogDbContext

# Apply locally
dotnet ef database update --context AppDbContext
dotnet ef database update --context LogDbContext
```

---

## Authentication

* **Development**: custom “Dev” auth scheme auto-authenticates.
* **Production**: JWT Bearer via **Microsoft.Identity.Web** (Azure AD).
  Swagger UI is configured for OAuth2 using your tenant + API scope.

---

## Logging & Observability

* **Serilog** configured from `appsettings.*` with enrichers and sinks (Console/File/SQL).
* **RequestLogMiddleware** writes a concise request/response row to `dbo.RequestLogs`

  * Method, Path, Status, Duration, Content types, UserAgent, Referrer, IP
  * (Optional) redacted headers/body snapshot
  * Exception details when present
* **Global Exception Handler** emits clean `ApiResponse.Error(...)`

  * Maps FluentValidation → **422**
  * 404/403/400 explicitly handled
  * SQL unique violations (e.g., duplicate MRN) mapped to **409 Conflict** with a user-friendly message

Use the **CorrelationId** to pivot across:

* Response payload
* Response headers
* Serilog entries
* `dbo.RequestLogs`

---

## CI/CD (GitHub → Azure)

Workflow: `.github/workflows/ci-cd-api.yml`

**Pipeline steps**

1. Checkout
2. Setup .NET 8
3. Install `dotnet-ef`
4. Build & `dotnet publish`
5. **OIDC** login to Azure (`azure/login@v2`) — no password secrets
6. Apply EF Core migrations (App & Log contexts) using `PROD_SQL_CONN`
7. Deploy the published output to **Azure Web App**

**Required GitHub Secrets**

* `AZURE_CLIENT_ID` – App registration (service principal) client ID
* `AZURE_TENANT_ID` – Tenant ID
* `AZURE_SUBSCRIPTION_ID` – Subscription ID
* `AZURE_WEBAPP_NAME` – Azure Web App name
* `PROD_SQL_CONN` – Full SQL connection string (used by `dotnet ef database update`)

---

## Common CLI Commands

```bash
# List packages (direct)
dotnet list package

# Include transitive dependencies
dotnet list package --include-transitive

# Outdated packages
dotnet list package --outdated

# Vulnerabilities (if supported)
dotnet list package --vulnerable
```

---

## Sample Endpoints

### GET /api/patients

Returns patients as DTOs. Enums are serialized as strings.

### POST /api/patients

Creates a patient via MediatR handler; validated with FluentValidation; names normalized by interceptor; returns `201 Created`.

**Sample request**

```json
{
  "medicalRecordNumber": "MRN000123",
  "firstName": "michael",
  "lastName": "jones",
  "dateOfBirth": "1975-05-21",
  "gender": "Male",
  "address": "1 Main St",
  "phoneNumber": "(555)555-1212",
  "email": "mj@example.com",
  "insuranceProviderId": 3,
  "insurancePolicyNumber": "ABC12345"
}
```
