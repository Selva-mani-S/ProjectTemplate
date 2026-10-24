# Authentication and Authorization Architecture

This document describes the security implementation for user authentication, role-based access control (RBAC), password policies, cookie storage, and lockout protection.

---

## 1. ASP.NET Core Identity Configuration

The application configures ASP.NET Core Identity in [ServiceCollectionExtensions.cs](file:///e:/ProjectTemplate/src/Solution.Web/Extensions/ServiceCollectionExtensions.cs) and [Program.cs](file:///e:/ProjectTemplate/src/Solution.Web/Program.cs).

### User Model: `ApplicationUser`
`ApplicationUser` extends `IdentityUser` with domain-specific properties:
- **`ServiceNo`**: String property mapping the Identity account directly to `app.IAF_PersonalData.ServiceNo`.
- **`FullName`**: Display name of the user.
- **`AccountActivationToken`**: Cryptographic string token used during the self-activation flow.
- **`IsActivated`**: Boolean flag indicating if the member has completed password establishment.
- **`CreatedAt`** & **`LastLoginAt`**: Activity audit timestamps.

---

## 2. Password and Lockout Policies

To balance veteran usability with enterprise security standards, the password policy is defined as:

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User requirements
    options.User.RequireUniqueEmail = false; // Multiple members without email or sharing family emails
    options.SignIn.RequireConfirmedEmail = false;
});
```

---

## 3. Cookie Hardening & Prefixing

The authentication cookie is hardened against interception and cross-site leaks:

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host-AppIdentity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
```

> **Why `__Host-` Prefix?**  
> Browsers will only accept cookies prefixed with `__Host-` if they are sent over HTTPS (`Secure`), have path `/`, and originate from the exact host (not a subdomain). This prevents subdomain cookie-tossing attacks.

---

## 4. Role-Based Access Control (RBAC)

Two distinct roles are seeded by [IdentityDataSeeder.cs](file:///e:/ProjectTemplate/src/Solution.Infrastructure/Persistence/IdentityDataSeeder.cs):

| Role | Target Users | Permissions & Access Scope |
| :--- | :--- | :--- |
| **`Admin`** | Reunion Committee & Organizers | Full access to Member Registry (CRUD, Import, Error Logs), Account Provisioning, Reunion Event Management, Payment Verification, Admin Dashboard, and all 14 Reports. |
| **`Member`** | Verified IAF Batch 1986 Veterans | Access to Personal Member Profile (view and edit self), Event Registration, Payment Proof Submission, and Member Dashboard. |

### Controller Protection Examples:
- **Admin-Only Controller**:
  ```csharp
  [Authorize(Roles = "Admin")]
  public class MemberAccountController : Controller { ... }
  ```
- **Shared Access with Record-Level Ownership**:
  ```csharp
  [Authorize(Roles = "Admin,Member")]
  public async Task<IActionResult> Details(string id)
  {
      if (User.IsInRole("Member"))
      {
          var userServiceNo = User.FindFirst("ServiceNo")?.Value;
          if (!string.Equals(userServiceNo, id, StringComparison.OrdinalIgnoreCase))
          {
              return Forbid(); // Prevents members from viewing other veterans' records
          }
      }
      ...
  }
  ```

---

## 5. Anti-Forgery Protection (CSRF)

All state-changing operations (HTTP POST, PUT, DELETE) are protected by ASP.NET Core Anti-Forgery:
- Enabled globally via `AutoValidateAntiforgeryTokenAttribute` in MVC options:
  ```csharp
  services.AddControllersWithViews(options =>
  {
      options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
  });
  ```
- Cookie name is set to `__Host-Antiforgery` with `SameSiteMode.Strict` and `SecurePolicy = CookieSecurePolicy.Always`.
- Every Razor `<form>` automatically includes the hidden `__RequestVerificationToken` field via ASP.NET Core Tag Helpers.
