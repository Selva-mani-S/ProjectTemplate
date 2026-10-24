using Microsoft.AspNetCore.Server.Kestrel.Core;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;
using Solution.Web.Extensions;
using Solution.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Remove Kestrel Server Header (OWASP A05: Security Misconfiguration)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Configure Services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddSecurityAndIdentityServices();
builder.Services.AddApplicationServices();

var app = builder.Build();

// Initialize Database & Seed Master Data
await DbInitializer.InitializeAsync(app.Services).ConfigureAwait(false);
await IdentityDataSeeder.SeedAdminUserAsync(app.Services).ConfigureAwait(false);

// OWASP A09: Custom StatusCodePages and Error Pipeline hiding stack traces and internal metadata
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// OWASP Security Headers Middleware (Dynamic CSP Nonce, HSTS, X-Frame-Options, etc.)
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
