# Standing Instructions and Rules for AI Coding Agents

All AI agents interacting with or modifying this repository **MUST** adhere to the rules outlined in this document. Failure to follow these rules will introduce compilation failures, security regressions, or data corruption.

---

## 1. Core Principles

1. **Codebase as Truth**: Always inspect the existing code before making assumptions. Do not assume third-party libraries (e.g., MediatR, AutoMapper) are present unless verified in project files.
2. **Minimal Necessary Change**: Make the smallest correct change that solves the user's requirement. Avoid broad refactorings.
3. **Preserve Business Logic & Conventions**: Retain existing naming schemes, architectural boundaries, and coding conventions.
4. **Zero Compiler Warnings**: The solution enables `TreatWarningsAsErrors=true` with `AnalysisMode=All`. Any warning is treated as an error and fails the build.

---

## 2. Mandatory Architectural Constraints

- **Solution Layering**:
  - `Solution.Domain`: Must have **NO** project references or external NuGet packages (except pure abstractions if necessary). No EF Core, no MVC, no ASP.NET dependencies.
  - `Solution.Application`: References `Solution.Domain`. Contains DTOs, interfaces, and validation contracts. Must **NOT** reference `Solution.Infrastructure` or `Solution.Web`.
  - `Solution.Infrastructure`: References `Solution.Application` and `Solution.Domain`. Implements persistence, external libraries (ClosedXML, CsvHelper), and data access. Must **NOT** reference `Solution.Web`.
  - `Solution.Web`: References `Solution.Application` and `Solution.Infrastructure`. Contains MVC Controllers, Razor views, filters, and startup configuration.
  - `tests/Solution.Tests`: References `Solution.Application`, `Solution.Infrastructure`, and `Solution.Domain`.

- **Dependency Injection**:
  - Register new services in [ServiceCollectionExtensions.cs](file:///e:/ProjectTemplate/src/Solution.Web/Extensions/ServiceCollectionExtensions.cs) or directly in `Program.cs`.
  - Maintain interface-driven contracts (`IMemberService`, `IPaymentService`, etc.).
  - Choose correct lifetimes: scoped for EF Core context-dependent services; transient for lightweight workers; singleton only for stateless utilities.

---

## 3. Strict Coding Conventions

### A. Roslyn and Code Analysis Compliance
- Always handle nullable reference types (`#nullable enable` is project-wide).
- Use `ConfigureAwait(false)` in library and infrastructure code where context synchronization is unnecessary.
- Never write empty catch blocks. Always log or propagate exceptions.
- Provide culture-invariant formatting (`CultureInfo.InvariantCulture` or `FormattableString`) when converting numbers, dates, or currencies to strings (e.g., `amount.ToString("N2", CultureInfo.InvariantCulture)`).

### B. Razor Views & CSP Nonce Compliance
- A dynamic CSP nonce is generated per-request by [SecurityHeadersMiddleware.cs](file:///e:/ProjectTemplate/src/Solution.Web/Middleware/SecurityHeadersMiddleware.cs) and stored in `ViewBag.CspNonce`.
- **CRITICAL**: Every inline `<script>` and `<style>` tag in Razor views **MUST** include the `nonce="@ViewBag.CspNonce"` attribute. Failure to do so will cause browser CSP block errors in production.
- Do not use inline event handlers (e.g., `onclick="..."`). Attach event listeners in external scripts or within nonced script blocks.

### C. EF Core & Database Integrity
- `ServiceNo` is the natural primary key for member entities. It is a string and must be trimmed and treated case-insensitively.
- Queries intended for read-only rendering or reports **MUST** use `.AsNoTracking()` to conserve memory and avoid context tracking overhead.
- Dual DbContexts exist:
  - `AppDbContext`: Manages application data (`app.` schema).
  - `AppIdentityDbContext`: Manages ASP.NET Core Identity (`auth.` schema).
  - Never mix entity registrations across these contexts.

### D. File and Report Handling
- When generating Excel workbooks using ClosedXML, worksheet names **MUST NOT exceed 31 characters**. Exceeding 31 characters throws an unhandled OpenXML exception.
- When generating CSVs using CsvHelper, always write with `CultureInfo.InvariantCulture`.
- Validate file uploads for size (max 5MB) and MIME type (`image/jpeg`, `image/png`, `application/pdf`). Never save uploaded files with user-controlled unvalidated paths.

---

## 4. Prohibited Actions (Anti-Patterns)

❌ **DO NOT**:
1. Execute database migrations or run `dotnet ef` without explicit user permission.
2. Add heavyweight dependencies (e.g., MediatR, Automapper, EntityFramework Core Proxies) when simple standard patterns already exist.
3. Commit secrets, hardcoded passwords, or API keys.
4. Suppress compiler warnings with `#pragma warning disable` without documented justification.
5. Store plaintext passwords or generate predictable initial passwords for users.
6. Write synchronous I/O methods (e.g., `File.ReadAllBytes`, `db.SaveChanges()`) in async controllers. Always use `SaveChangesAsync()`.
7. Hardcode URLs or API routes in Razor views; use `Url.Action(...)` or Tag Helpers (`asp-controller`, `asp-action`).

---

## 5. Verification Checklist Before Finishing a Task

1. Run code inspection across modified files to ensure no syntax errors or unresolved types.
2. Verify all new controller actions have appropriate `[Authorize(Roles = ...)]` attributes.
3. Verify anti-forgery validation is respected (`[AutoValidateAntiforgeryToken]` is globally active).
4. Verify nonced `<script>` tags on any new view containing scripts.
5. Ensure unit tests in `Solution.Tests` cover the new feature or bug fix.
6. Check that no git artifacts or temp files are committed.
