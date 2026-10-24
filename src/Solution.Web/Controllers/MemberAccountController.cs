namespace Solution.Web.Controllers;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;
using Solution.Web.Models;

[Authorize(Roles = "Administrator")]
public class MemberAccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogger _auditLogger;

    public MemberAccountController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IAuditLogger auditLogger)
    {
        _db = db;
        _userManager = userManager;
        _auditLogger = auditLogger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var members = await _db.PersonalData
            .Include(p => p.Trade)
            .OrderBy(p => p.ServiceNo)
            .ToListAsync();

        var users = await _userManager.Users.ToListAsync();
        var userMap = users
            .Where(u => u.ServiceNo != null)
            .ToDictionary(u => u.ServiceNo!, u => u, StringComparer.OrdinalIgnoreCase);

        var list = new List<MemberAccountItemDto>();

        foreach (var m in members)
        {
            bool hasAcc = userMap.TryGetValue(m.ServiceNo, out var u);
            string? actLink = null;
            if (hasAcc && !u!.EmailConfirmed && !string.IsNullOrEmpty(u.ActivationToken))
            {
                actLink = Url.Action("Activate", "Account", new { token = u.ActivationToken, email = u.Email }, Request.Scheme);
            }

            list.Add(new MemberAccountItemDto
            {
                ServiceNo = m.ServiceNo,
                MemberName = m.Name,
                MobileNo = m.MobileNo,
                MemberEmailId = m.EmailId,
                TradeName = m.Trade?.TradeName,
                HasAccount = hasAcc,
                Email = u?.Email,
                IsActive = u?.IsActive ?? false,
                IsActivated = u?.EmailConfirmed ?? false,
                LastLoginAtUtc = u?.LastLoginAtUtc,
                ActivationLink = actLink
            });
        }

        return View(new MemberAccountListViewModel { Accounts = list });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProvisionAccount(ProvisionAccountViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide a valid email address.";
            return RedirectToAction(nameof(Index));
        }

        var member = await _db.PersonalData.FirstOrDefaultAsync(p => p.ServiceNo == model.ServiceNo);
        if (member == null) return NotFound();

        // Check if an account already exists for this ServiceNo
        var existingForServiceNo = await _userManager.Users.FirstOrDefaultAsync(u => u.ServiceNo == model.ServiceNo);
        if (existingForServiceNo != null)
        {
            TempData["ErrorMessage"] = $"An account already exists for Service Number {model.ServiceNo}.";
            return RedirectToAction(nameof(Index));
        }

        // Generate cryptographically secure activation token
        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim().ToLowerInvariant(),
            Email = model.Email.Trim().ToLowerInvariant(),
            FirstName = member.Name,
            LastName = $"({member.ServiceNo})",
            Department = "IAF 1986 Batch",
            ServiceNo = member.ServiceNo,
            EmailConfirmed = false, // Must be activated by user setting their password
            IsActive = true,
            ActivationToken = token,
            ActivationTokenExpiresAtUtc = DateTime.UtcNow.AddDays(30),
            CreatedAtUtc = DateTime.UtcNow
        };

        // Create user with a secure randomized temporary initial password
        string tempPass = "Tmp!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + "9aA";
        var result = await _userManager.CreateAsync(user, tempPass);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _userManager.AddToRoleAsync(user, IdentityDataSeeder.MemberRole);

        await _auditLogger.LogSecurityEventAsync(
            "MemberAccountProvisioned",
            User.Identity?.Name ?? "Admin",
            User.Identity?.Name ?? "Admin",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Account provisioned for ServiceNo {member.ServiceNo} with Email {model.Email}",
            true);

        TempData["SuccessMessage"] = $"Account provisioned for {member.Name} ({member.ServiceNo}). Activation link generated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkProvisionAccounts()
    {
        var members = await _db.PersonalData.ToListAsync();
        var existingUsers = await _userManager.Users.ToListAsync();
        var existingServiceNos = existingUsers
            .Where(u => u.ServiceNo != null)
            .Select(u => u.ServiceNo!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int count = 0;
        foreach (var m in members)
        {
            if (existingServiceNos.Contains(m.ServiceNo)) continue;

            string email = !string.IsNullOrWhiteSpace(m.EmailId)
                ? m.EmailId.Trim().ToLowerInvariant()
                : $"{m.ServiceNo.ToLowerInvariant()}@iaf1986.local";
            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            string tempPass = "Tmp!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + "9aA";

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = m.Name,
                LastName = $"({m.ServiceNo})",
                Department = "IAF 1986 Batch",
                ServiceNo = m.ServiceNo,
                EmailConfirmed = false,
                IsActive = true,
                ActivationToken = token,
                ActivationTokenExpiresAtUtc = DateTime.UtcNow.AddDays(30),
                CreatedAtUtc = DateTime.UtcNow
            };

            var res = await _userManager.CreateAsync(user, tempPass);
            if (res.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, IdentityDataSeeder.MemberRole);
                count++;
            }
        }

        await _auditLogger.LogSecurityEventAsync(
            "BulkAccountsProvisioned",
            User.Identity?.Name ?? "Admin",
            User.Identity?.Name ?? "Admin",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Bulk provisioned {count} member accounts.",
            true);

        TempData["SuccessMessage"] = $"Bulk account generation completed. Created {count} accounts.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string serviceNo)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.ServiceNo == serviceNo);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = $"Account for {serviceNo} is now {(user.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Index));
    }
}
