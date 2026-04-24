# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build OnlineSales.slnx

# Run the API (default: http://192.168.1.6:5153)
dotnet run --project SalesWebApi/SalesWebApi.csproj

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~ClassName"

# Add EF migrations (example for VehicleSales module)
dotnet ef migrations add <MigrationName> --project VehicleSales --startup-project SalesWebApi --context VehicleSalesDbContext

# Apply migrations
dotnet ef database update --project VehicleSales --startup-project SalesWebApi --context VehicleSalesDbContext
```

## Required Configuration

The following must be set (user secrets or environment variables):

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:MariaDB` | MariaDB/MySQL connection string |
| `Jwt:EncryptionKey` | JWT signing key (HS256) |
| `BaseUrl` | API base URL (also used as JWT issuer/audience) |
| `AllowedOrigins` | CORS allowed origins array |
| `R2:AccountId`, `R2:AccessKeyId`, `R2:SecretAccessKey`, `R2:BucketName` | Cloudflare R2 for vehicle photo uploads |
| `DataProtection:EmailConfirmationTokenPurpose` | Data protection purpose string |

**Initial DB setup:** Run `/SQL scripts/init.sql` first to create schemas, then apply EF migrations per context.

User secrets ID for SalesWebApi: `bbb25c88-464c-48b9-98ea-e1eb77bd10e8`

## Architecture

The solution is a modular .NET 10 Web API organized as vertical modules, each owning its own domain logic, persistence context, and migrations.

**Solution projects:**
- `SalesWebApi` — ASP.NET Core entry point; wires up all modules via extension methods
- `VehicleSales` — Vehicle listing domain (commands, queries, entities, EF context)
- `UserIdentity` — Auth/user management (JWT, registration, email confirmation)
- `ObjectUploadTracking` — Tracks Cloudflare R2 presigned uploads with expiry
- `EmailSending` — Email abstraction (currently console stub)
- `Common` — Shared DbContext registration helpers and constants
- `SalesWebApi.IntegrationTests` — xUnit v3 integration tests using `WebApplicationFactory<Program>`

### Request Flow

```
HTTP endpoint (SalesWebApi/Endpoints/) 
  → Command or Query (per-module, e.g. VehicleSales/Commands/)
  → DbContext or raw MySqlConnector
  → MariaDB
```

Endpoints are minimal API handlers (not controllers). Each module exposes a `Map*Endpoints(IEndpointRouteBuilder)` extension method called from `Program.cs`.

### Key Patterns

**Functional error handling** — All commands return `Result<T>` from `CSharpFunctionalExtensions`. Business logic errors propagate via `Result.Failure(errorCode)` rather than exceptions. Endpoints translate `Result` to HTTP responses.

**Error codes** — Commands return string error codes (e.g., `"VehicleSaleNotFound"`, `"Unauthorized"`). These are mapped to HTTP status codes in the endpoint layer. Follow the existing pattern when adding new commands.

**Value Objects** — Domain constraints are enforced via sealed records with private constructors and a static `Create(input)` method returning `Result<T>`. Example: `SaleTitle.Create(string)`, `Money.Create(decimal)`.

**Read path uses raw SQL** — `GetVehicleSales` and similar queries use `MySqlConnector` directly (not EF) for performance. Write path uses `VehicleSalesDbContext`.

**Multiple DbContexts** — Each module has its own EF context and its own `__EFMigrationsHistory_<ContextName>` table.

**Background jobs** — TickerQ runs periodic jobs (e.g., `RemoveUnconfirmedUsersJob`, `RemoveExpiredObjectUploads`).

**Photo uploads** — Photos are not uploaded through the API. The API issues presigned S3 PUT URLs for Cloudflare R2; the client uploads directly. `ObjectUploadTracking` tracks pending uploads and cleans up unconfirmed ones.

### Authentication

JWT Bearer (HS256). The issuer and audience are both set to `BaseUrl`. Protected endpoints call `.RequireAuthorization()`. The current user's ID is extracted via the `ClaimsPrincipal.UserId()` extension method.

### Module Registration

Each module registers itself via an `Add*` extension on `IServiceCollection` and a `Use*` or `Map*` extension on the app/endpoint builder. Example in `Program.cs`:

```csharp
builder.Services.AddVehicleSales(builder.Configuration);
// ...
app.MapVehicleSalesEndpoints();
```

### Testing

Integration tests use snapshot verification (Verify.XunitV3). Verified snapshots live in `*.verified.json` files alongside the test files. When adding new tests, run once to generate the snapshot, review it, then commit it.
