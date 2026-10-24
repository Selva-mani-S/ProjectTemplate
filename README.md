# Production-Grade ASP.NET Core (.NET 10) Security Baseline Template

This baseline template provides a production-grade architecture and implementation plan for web applications using **ASP.NET Core (.NET 10)**, **MVC**, **Bootstrap 5**, and **ASP.NET Core Identity with Entity Framework Core**, strictly adhering to **OWASP Top 10 (2021)** and **OWASP ASVS** standards.

---

## 🏗️ Solution Architecture

The solution follows a multi-project Clean Architecture pattern:

```text
/ (Repository Root)
├── Directory.Build.props              # Enforces compiler flags, analyzers, and warning rules globally
├── Directory.Packages.props           # Central Package Management (CPM) pinning all NuGet dependencies
├── Solution.slnx                      # .NET Solution file
├── README.md
└── src/
    ├── Solution.Domain/               # Core Domain models and entities (No infrastructure dependencies)
    ├── Solution.Application/          # Interfaces (IAuditLogger) and application service abstractions
    ├── Solution.Infrastructure/       # Persistence (AppDbContext, AppIdentityDbContext), Logging, Identity Seeder
    └── Solution.Web/                  # ASP.NET Core MVC presentation layer, Middleware, Controllers, Views
        ├── appsettings.json           # LocalDB connection string & framework settings
        ├── libman.json                # Client-side asset governance (Bootstrap 5, Icons, jQuery)
        └── wwwroot/lib/               # Pinned local static assets fetched via LibMan
```

---

## 🔐 OWASP Security Controls Implemented

1. **A01: Broken Access Control & A07: Identification and Authentication Failures**
   - **Global Fallback Policy:** All controllers/actions require authentication by default unless explicitly decorated with `[AllowAnonymous]`.
   - **Strict Identity Configuration:** Password requirements (12+ characters, upper, lower, digits, special characters, 4 unique characters), account lockout (max 5 failed attempts, 15-minute duration), and mandatory email confirmation.
   - **Secure Cookie Configuration:** `__Host-AppIdentity` and `__Host-Antiforgery` cookies configured with `HttpOnly = true`, `SecurePolicy = Always`, `SameSite = Strict`, and 30-minute sliding expiration.
   - **Pre-Seeded Site Administrator:** Startup seed (`admin@baseline.local` / `AdminP@ssw0rd123!`) with `Administrator` role.
   - **Department User Management & Self-Registration:** Site admins manage department-level users; anonymous users can self-register.

2. **A03: Injection & Cross-Site Scripting (XSS)**
   - **Global Anti-Forgery Filter:** `AutoValidateAntiforgeryTokenAttribute` applied globally to all MVC actions.
   - **Dynamic CSP Nonce Generation:** Custom `SecurityHeadersMiddleware` generates a 128-bit cryptographically secure base64 nonce per request, attached to `HttpContext.Items` and injected into script/style Content Security Policies without using `'unsafe-inline'`.

3. **A05: Security Misconfiguration**
   - **Security Headers:** `HSTS` (`max-age=31536000; includeSubDomains; preload`), `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, and `Permissions-Policy`.
   - **Header Removal:** Kestrel server headers (`Server`, `X-Powered-By`, `X-AspNet-Version`) removed.

4. **A06: Vulnerable and Outdated Components**
   - **Central Package Management (CPM):** All NuGet packages are centrally managed via `Directory.Packages.props`. `.csproj` files contain zero `Version` attributes.
   - **Client-Side Asset Governance via LibMan:** Local static assets (Bootstrap 5.3.3, Bootstrap Icons 1.11.3, jQuery 3.7.1) explicitly managed, pinned, and restored via `libman.json`.

5. **A09: Security Logging and Monitoring Failures**
   - Custom `IAuditLogger` records security events (login success/failure, account lockout, registration, department updates) into `AppDbContext` without exposing credentials or unencrypted PII.
   - StatusCode pages re-execution pipeline (`/Error/{0}`) hides internal stack traces in production.

---

## 🚀 Execution & Command-Line Guide

### 1. Restore Client-Side Assets via LibMan CLI

Install the Library Manager CLI globally and restore pinned static assets into `wwwroot/lib/`:

```bash
# Install LibMan CLI tool
dotnet tool install --global Microsoft.Web.LibraryManager.Cli

# Restore pinned client-side assets (from src/Solution.Web directory)
cd src/Solution.Web
libman restore
cd ../..
```

---

### 2. Entity Framework Core Migrations Guide

The infrastructure separates **Application Domain & Data Protection** (`AppDbContext` in `app` schema) from **Identity & Authentication** (`AppIdentityDbContext` in `identity` schema).

Ensure you have the EF Core CLI tool installed:

```bash
dotnet tool install --global dotnet-ef
```

#### A. Initializing Migrations for AppDbContext (Domain & DataProtection Keys)

```bash
# Add Initial Migration for AppDbContext
dotnet ef migrations add InitialAppDb \
  --project src/Solution.Infrastructure/Solution.Infrastructure.csproj \
  --startup-project src/Solution.Web/Solution.Web.csproj \
  --context Solution.Infrastructure.Persistence.AppDbContext \
  --output-dir Persistence/Migrations/AppDb

# Update Database for AppDbContext
dotnet ef database update  --project src/Solution.Infrastructure/Solution.Infrastructure.csproj --startup-project src/Solution.Web/Solution.Web.csproj --context Solution.Infrastructure.Persistence.AppDbContext

#### B. Initializing Migrations for AppIdentityDbContext (ASP.NET Core Identity)

```bash
# Add Initial Migration for AppIdentityDbContext
dotnet ef migrations add InitialIdentityDb \
  --project src/Solution.Infrastructure/Solution.Infrastructure.csproj \
  --startup-project src/Solution.Web/Solution.Web.csproj \
  --context Solution.Infrastructure.Persistence.AppIdentityDbContext \
  --output-dir Persistence/Migrations/IdentityDb

# Update Database for AppIdentityDbContext
dotnet ef database update --project src/Solution.Infrastructure/Solution.Infrastructure.csproj --startup-project src/Solution.Web/Solution.Web.csproj --context Solution.Infrastructure.Persistence.AppIdentityDbContext

---

### 3. Build & Run Application

```bash
# Build Solution
dotnet build Solution.slnx

# Run Web Application
dotnet run --project src/Solution.Web/Solution.Web.csproj
```

Default Credentials for Local Development:
- **Site Administrator:** `admin@baseline.local`
- **Password:** `AdminP@ssw0rd123!`
