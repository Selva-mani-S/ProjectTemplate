# Configuration, Database Initialization, and Deployment

This document covers application configuration keys, connection strings, database migration procedures, and deployment requirements.

---

## 1. Application Configuration: `appsettings.json`

The root configuration file is located at `src/Solution.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IafReunionDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AdminAccount": {
    "Email": "admin@iaf1986.local",
    "FullName": "Chief Reunion Coordinator",
    "ServiceNo": "ADMIN001"
  }
}
```

### Environment-Specific Overrides:
- For production deployments, database credentials and sensitive secrets should be supplied via environment variables:
  `ConnectionStrings__DefaultConnection="Server=tcp:sqlserver.iaf1986.internal...;Database=IafReunionDb;User Id=...;Password=...;"`

---

## 2. Database Migrations and Schema Updates

The repository uses Entity Framework Core Code-First migrations with migrations housed in `Solution.Infrastructure/Persistence/Migrations`.

### Manual Migration Commands (For Human Developers)
> [!NOTE]  
> AI coding agents must not execute these commands directly without user authorization. The following commands should be executed manually in a terminal if schema changes are introduced:

1. **Add a New Migration**:
   ```powershell
   dotnet ef migrations add <MigrationName> --project src/Solution.Infrastructure --startup-project src/Solution.Web --context AppDbContext
   ```

2. **Apply Migrations to Target Database**:
   ```powershell
   dotnet ef database update --project src/Solution.Infrastructure --startup-project src/Solution.Web --context AppDbContext
   ```

3. **Generate an Idempotent SQL Script for Production DBA Execution**:
   ```powershell
   dotnet ef migrations script --idempotent --project src/Solution.Infrastructure --startup-project src/Solution.Web --context AppDbContext --output database-migration.sql
   ```

---

## 3. Database Initialization & Seeding on Startup

During application startup in [Program.cs](file:///e:/ProjectTemplate/src/Solution.Web/Program.cs), the application automatically ensures the database is initialized:

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var appDb = services.GetRequiredService<AppDbContext>();
    var identityDb = services.GetRequiredService<AppIdentityDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // 1. Ensure schema exists
        await appDb.Database.MigrateAsync();
        await identityDb.Database.MigrateAsync();

        // 2. Seed identity roles and default admin
        await IdentityDataSeeder.SeedAsync(roleManager, userManager, builder.Configuration);

        // 3. Seed domain masters (Trades, States, Reunion Event, Sample data)
        await DbInitializer.InitializeAsync(appDb);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
```

---

## 4. Production Deployment Guidelines

1. **Reverse Proxy Configuration (IIS, Nginx, or Kestrel behind Azure App Service)**:
   - Configure `ForwardedHeadersOptions` so that HTTPS protocol and client IP addresses are correctly forwarded.
2. **SSL / TLS Certificate**:
   - The application relies on HTTPS to enforce `__Host-` cookie prefixes and HSTS. Unencrypted HTTP will reject authentication cookies.
3. **Storage & Binary File Persistence**:
   - Uploaded receipt proofs and member photos are currently stored in `VARBINARY(MAX)` columns within SQL Server.
   - For ultra-high volume scaling, consider implementing a cloud blob storage provider (e.g., MinIO or Azure Blob Storage) implementing a shared storage interface.
4. **Health Checks**:
   - Register ASP.NET Core Health Checks on SQL Server connectivity:
     `builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();`
