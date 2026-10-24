# Coding Conventions and Engineering Standards

This document establishes the mandatory C# coding standards, Roslyn analyzer rules, asynchronous best practices, and error-handling conventions enforced in this repository.

---

## 1. Compiler and Roslyn Configuration

The build system enforces strict compliance via [Directory.Build.props](file:///e:/ProjectTemplate/Directory.Build.props):

- **Target Framework**: `net10.0`
- **Language Version**: `14` (preview/latest)
- **Nullable Reference Types**: `<Nullable>enable</Nullable>`
- **Implicit Usings**: `<ImplicitUsings>enable</ImplicitUsings>`
- **Treat Warnings As Errors**: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- **Analysis Mode**: `<AnalysisMode>All</AnalysisMode>`
- **Analysis Level**: `<AnalysisLevel>latest-recommended</AnalysisLevel>`

> [!CAUTION]  
> Any code that produces a compiler warning will break the build. AI agents must resolve all warnings at the source rather than suppressing them.

---

## 2. C# 14 & .NET 10 Idioms

### A. Primary Constructors
Use primary constructors on classes and records where dependencies are injected, avoiding repetitive private readonly fields:

```csharp
public class MemberService(AppDbContext dbContext, ILogger<MemberService> logger) : IMemberService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<MemberService> _logger = logger;
    ...
}
```

### B. Pattern Matching & Switch Expressions
Prefer concise switch expressions over verbose `switch-case` blocks:

```csharp
public static string FormatTransitDescription(TransitPoint transit) => transit switch
{
    TransitPoint.Airport => "Chennai International Airport (MAA)",
    TransitPoint.ChennaiCentral => "Chennai Central Railway Station (MAS)",
    TransitPoint.Egmore => "Chennai Egmore Railway Station (MS)",
    TransitPoint.Tambaram => "Tambaram Railway Station (TBM)",
    TransitPoint.SelfArranged => "Self-Arranged Local Transit",
    _ => "Unspecified Transit Node"
};
```

### C. Collection Expressions
Use C# collection expressions `[...]` instead of `new List<T>()` or `new T[] { ... }`:

```csharp
List<string> requiredHeaders = ["ServiceNo", "MemberName", "Rank", "Trade", "MobileNo"];
```

---

## 3. Formatting, Culture, and Localization

1. **Culture-Invariant Formatting**:
   Always specify `CultureInfo.InvariantCulture` when converting numerical, currency, or date values to strings:
   ```csharp
   // Correct
   string formattedAmount = payment.Amount.ToString("N2", CultureInfo.InvariantCulture);
   
   // Incorrect - depends on host server locale
   string formattedAmount = payment.Amount.ToString("N2");
   ```
2. **Date Storage**:
   Always store timestamps in UTC (`DateTime.UtcNow`). Convert to Indian Standard Time (IST, UTC+5:30) only at the presentation layer when rendering in views.

---

## 4. Asynchronous Programming Standards

- **Async All The Way**: Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on asynchronous operations.
- **Cancellation Tokens**: Accept `CancellationToken cancellationToken = default` on public service methods and pass them to EF Core calls (`SaveChangesAsync`, `ToListAsync`, etc.).
- **ConfigureAwait**:
  - In `Solution.Infrastructure` and class libraries: Use `.ConfigureAwait(false)` to avoid capturing synchronization contexts.
  - In `Solution.Web` (Controllers): Standard `await` without `ConfigureAwait(false)` is acceptable.

---

## 5. Defensive Programming & Null Handling

1. **Explicit Guard Clauses**:
   Validate arguments at method entry:
   ```csharp
   ArgumentException.ThrowIfNullOrWhiteSpace(serviceNo);
   ArgumentNullException.ThrowIfNull(dto);
   ```
2. **Nullable Reference Types**:
   Honor nullability annotations. If a database property can be null in the schema, mark it as nullable in the entity (`string? MobileNo`). If an entity navigation is required, initialize it or annotate with `null!`:
   ```csharp
   public IafPersonalData PersonalData { get; set; } = null!;
   ```

---

## 6. Error Handling and Logging

1. **No Silent Swallowing**:
   Never catch an exception without logging or rethrowing.
   ```csharp
   // Prohibited
   try { ... } catch { }
   
   // Mandatory
   try { ... }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Failed to process payment verification for ID {PaymentId}", paymentId);
       throw;
   }
   ```
2. **Structured Logging**:
   Use structured logging templates with named parameters rather than string concatenation or interpolation:
   ```csharp
   // Correct
   _logger.LogInformation("Member {ServiceNo} activated their account successfully.", serviceNo);

   // Incorrect
   _logger.LogInformation($"Member {serviceNo} activated their account successfully.");
   ```
