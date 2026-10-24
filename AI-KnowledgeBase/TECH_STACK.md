# Technology Stack Specification

This document lists the exact versions of runtimes, frameworks, NuGet packages, client-side libraries, and toolchains verified directly from the repository's configuration files ([Directory.Build.props](file:///e:/ProjectTemplate/Directory.Build.props), [Directory.Packages.props](file:///e:/ProjectTemplate/Directory.Packages.props), and [libman.json](file:///e:/ProjectTemplate/src/Solution.Web/libman.json)).

---

## 1. Runtime and Language

| Component | Version | Verification Source |
| :--- | :--- | :--- |
| **.NET Target Framework** | `net10.0` | `Directory.Build.props` (`<TargetFramework>net10.0</TargetFramework>`) |
| **C# Language Version** | `14` (preview/latest) | `Directory.Build.props` (`<LangVersion>preview</LangVersion>`) |
| **Nullable Reference Types** | `enable` | `Directory.Build.props` (`<Nullable>enable</Nullable>`) |
| **Implicit Usings** | `enable` | `Directory.Build.props` (`<ImplicitUsings>enable</ImplicitUsings>`) |
| **Code Analysis Mode** | `All` | `Directory.Build.props` (`<AnalysisMode>All</AnalysisMode>`) |
| **Analysis Level** | `latest-recommended` | `Directory.Build.props` (`<AnalysisLevel>latest-recommended</AnalysisLevel>`) |
| **Treat Warnings As Errors** | `true` | `Directory.Build.props` (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) |

---

## 2. Server-Side Frameworks & NuGet Packages

Packages are centrally managed via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`).

| Package Name | Version | Usage Scope |
| :--- | :--- | :--- |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | `10.0.12` | Identity store and user/role managers in `Solution.Infrastructure` |
| **Microsoft.EntityFrameworkCore** | `10.0.12` | EF Core ORM abstractions |
| **Microsoft.EntityFrameworkCore.SqlServer** | `10.0.12` | SQL Server database provider |
| **Microsoft.EntityFrameworkCore.InMemory** | `10.0.12` | In-memory provider for unit/integration tests |
| **Microsoft.EntityFrameworkCore.Design** | `10.0.12` | Design-time tools for migrations |
| **ClosedXML** | `0.105.1` | Excel `.xlsx` report generation and member spreadsheet import |
| **CsvHelper** | `33.1.0` | High-throughput CSV parsing and streaming export |
| **Microsoft.NET.Test.Sdk** | `18.10.1` | Test execution host in `Solution.Tests` |
| **xunit** | `2.9.3` | Test framework in `Solution.Tests` |
| **xunit.runner.visualstudio** | `3.1.5` | Visual Studio / `dotnet test` runner |
| **coverlet.collector** | `10.0.1` | Code coverage collector |

---

## 3. Database Engine

| Technology | Supported Targets | Schema Design |
| :--- | :--- | :--- |
| **Microsoft SQL Server** | SQL Server 2019, 2022, Azure SQL | Two custom schemas: `app` (application data) and `auth` (Identity data). |
| **In-Memory Database** | EF Core InMemory | Used exclusively for rapid unit/integration test isolation. |

---

## 4. Frontend & Client-Side Libraries

Client-side assets are managed via Microsoft Library Manager (`libman.json`) located at `src/Solution.Web/libman.json`.

| Library | Version | Destination Provider & Directory | Description |
| :--- | :--- | :--- | :--- |
| **Bootstrap** | `5.3.3` | `cdnjs` -> `wwwroot/lib/bootstrap/` | Primary CSS grid, responsive layout, modals, navbar, cards, badges |
| **Bootstrap Icons** | `1.11.3` | `cdnjs` -> `wwwroot/lib/bootstrap-icons/` | SVG vector icon font for UI buttons, badges, and status cards |
| **jQuery** | `3.7.1` | `cdnjs` -> `wwwroot/lib/jquery/` | DOM traversal, event handling, AJAX integration |
| **jQuery Validation** | `1.21.0` | `cdnjs` -> `wwwroot/lib/jquery-validate/` | Core client-side form validation |
| **jQuery Validation Unobtrusive** | `4.0.0` | `cdnjs` -> `wwwroot/lib/jquery-validation-unobtrusive/` | Data-attribute driven validation mapping ASP.NET model attributes |

---

## 5. Security Tooling & Cryptography

| Capability | Mechanism | Implementation Reference |
| :--- | :--- | :--- |
| **CSP Nonce Generation** | 128-bit cryptographically secure random bytes (Base64) | [SecurityHeadersMiddleware.cs](file:///e:/ProjectTemplate/src/Solution.Web/Middleware/SecurityHeadersMiddleware.cs) |
| **Account Activation Tokens** | 256-bit cryptographically secure random bytes (Base64URL) | [MemberAccountController.cs](file:///e:/ProjectTemplate/src/Solution.Web/Controllers/MemberAccountController.cs) |
| **Anti-Forgery (CSRF)** | Cookie-to-Header token verification (`__Host-Antiforgery`) | `Program.cs` / `AutoValidateAntiforgeryTokenAttribute` |
| **Password Hashing** | PBKDF2 with HMAC-SHA256, 100,000 iterations (Identity v3) | ASP.NET Core Identity default |
| **Cookie Storage** | SameSite=Strict, Secure=Always, HttpOnly=True (`__Host-AppIdentity`) | `ServiceCollectionExtensions.cs` |
