# Security Architecture, Threat Mitigation, and Performance

This document describes the application's security safeguards, defenses against common web vulnerabilities, query optimization patterns, and memory management strategies.

---

## 1. Security Architecture & Threat Mitigations

### A. Content Security Policy (CSP) & XSS Mitigation
- **Mechanism**: Every incoming request passes through [SecurityHeadersMiddleware.cs](file:///e:/ProjectTemplate/src/Solution.Web/Middleware/SecurityHeadersMiddleware.cs), which generates a unique 128-bit cryptographic nonce:
  ```csharp
  byte[] nonceBytes = RandomNumberGenerator.GetBytes(16);
  string nonce = Convert.ToBase64String(nonceBytes);
  context.Items["CspNonce"] = nonce;
  ```
- **Header Enforced**:
  ```http
  Content-Security-Policy: default-src 'self'; script-src 'self' 'nonce-{NONCE}'; style-src 'self' 'nonce-{NONCE}'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';
  ```
- **Protection**: Completely prevents Stored and Reflected Cross-Site Scripting (XSS), as any injected script tag lacks the per-request cryptographic nonce.

### B. Cross-Site Request Forgery (CSRF) Mitigation
- **AutoValidateAntiforgeryToken**: Globally applied to all controller actions in MVC filters.
- **Form Tag Helpers**: Every Razor `<form>` automatically generates a hidden `__RequestVerificationToken` verified against the encrypted `__Host-Antiforgery` cookie.
- **Strict SameSite**: Cookies set with `SameSiteMode.Strict` prevent browsers from sending authentication cookies during cross-origin requests.

### C. SQL Injection Mitigation
- **Entity Framework Core Parameterization**: All database queries are executed using EF Core LINQ queries, which compile to parameterized SQL queries (`@p0`, `@p1`).
- **Raw SQL Restriction**: Raw string interpolation in `FromSqlRaw` is strictly forbidden. If raw SQL is ever necessary, `FromSqlInterpolated` must be used.

### D. File Upload Protection (Unrestricted File Upload Mitigation)
Uploaded payment receipts and photos are restricted by:
1. **Size Limitation**: Hard ceiling of 5 MB (`5 * 1024 * 1024` bytes) enforced in controller and service logic.
2. **MIME Type Whitelist**: Only `image/jpeg`, `image/png`, and `application/pdf` are accepted.
3. **Storage Isolation**: Uploaded byte arrays are stored in the database (`VARBINARY(MAX)`) or isolated storage with synthesized, sanitized file names—never stored directly in web-accessible directories using user-provided names.

### E. Account Enumeration & Brute-Force Protection
- Login returns generic error messages (*"Invalid login attempt"*) to prevent username enumeration.
- Account lockout triggers after 5 consecutive failed attempts for 15 minutes.

---

## 2. Performance & Query Optimization

### A. Read-Only Queries: `.AsNoTracking()`
All queries used for data export, reports, search lists, and dashboard aggregations use `.AsNoTracking()` to avoid EF Core snapshot tracking:

```csharp
// High-performance read-only query
var members = await _dbContext.IafPersonalData
    .AsNoTracking()
    .Include(m => m.Trade)
    .Include(m => m.State)
    .Where(m => m.TradeCode == tradeCode)
    .ToListAsync(cancellationToken)
    .ConfigureAwait(false);
```

### B. Projection with `Select`
Avoid fetching full entity graphs when only a subset of fields is required:

```csharp
var attendeeSummaries = await _dbContext.Reunions
    .AsNoTracking()
    .Select(r => new { r.ServiceNo, r.TotalAttendees, r.DietaryPreference })
    .ToListAsync(cancellationToken)
    .ConfigureAwait(false);
```

### C. Database Indexing Strategy
- **Natural Primary Keys**: Clustered index on `ServiceNo`.
- **Foreign Key Indexes**: Automatic indexes on foreign keys (`TradeCode`, `StateCode`, `EventId`).
- **Transaction Reference Index**: Non-clustered index on `IAF_Payment.ReferenceNumber` and `AcknowledgmentNumber` for constant-time lookup during duplicate checks.

### D. Memory Management in File Exports
When generating Excel workbooks via ClosedXML or CSVs via CsvHelper:
- Use in-memory buffer pools or streams disposed immediately after writing to the HTTP response.
- Do not retain large dataset collections in memory longer than the request lifetime.
- Paginate member registry listings with default page size of 25 to 50 records.
