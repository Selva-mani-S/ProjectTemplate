namespace Solution.Web.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Logging;
using Solution.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=BaselineSecureDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        // DbContexts isolation: Separate IdentityDbContext and AppDbContext with InMemory database for dev/testing when SQL Server LocalDB isn't available
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("BaselineSecureDb"));

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseInMemoryDatabase("BaselineSecureIdentityDb"));

        // Data Protection API: Key persistence in Database
        services.AddDataProtection()
            .SetApplicationName("BaselineSecureApp")
            .PersistKeysToDbContext<AppDbContext>();

        // Custom Audit Logger
        services.AddScoped<IAuditLogger, AuditLogger>();

        return services;
    }

    public static IServiceCollection AddSecurityAndIdentityServices(
        this IServiceCollection services)
    {
        // OWASP A07: Strict Password Complexity & Account Lockout
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings (5 failed attempts, 15 minute lockout)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User & Sign-In settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<AppIdentityDbContext>()
        .AddDefaultTokenProviders();

        // OWASP A01 / A07: Secure Cookie Policy Configuration
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Name = "__Host-AppIdentity";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        // OWASP A01: Global Fallback Authorization Policy requiring authentication by default
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        // OWASP A03: Anti-Forgery Global Validation & Secure Anti-Forgery Cookie
        services.AddAntiforgery(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Name = "__Host-Antiforgery";
            options.HeaderName = "X-XSRF-TOKEN";
        });

        services.AddControllersWithViews(options =>
        {
            // Apply AutoValidateAntiforgeryToken globally
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        });

        return services;
    }
}
