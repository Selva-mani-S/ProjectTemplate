namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;
using Solution.Web.Models;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AppDbContext _db;
    private readonly IAuditLogger _auditLogger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AppDbContext db,
        IAuditLogger auditLogger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _auditLogger = auditLogger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        if (user == null)
        {
            await _auditLogger.LogSecurityEventAsync(
                "LOGIN_FAILED",
                "Anonymous",
                model.Email,
                ipAddress,
                userAgent,
                "Failed login attempt - User does not exist",
                false).ConfigureAwait(false);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!user.IsActive)
        {
            await _auditLogger.LogSecurityEventAsync(
                "LOGIN_INACTIVE_USER",
                user.Id,
                user.UserName!,
                ipAddress,
                userAgent,
                "Login rejected: Account is inactive",
                false).ConfigureAwait(false);

            ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact an organizer.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: true).ConfigureAwait(false);

        if (result.Succeeded)
        {
            user.LastLoginAtUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user).ConfigureAwait(false);

            await _auditLogger.LogSecurityEventAsync(
                "LOGIN_SUCCESS",
                user.Id,
                user.UserName!,
                ipAddress,
                userAgent,
                "User successfully logged in",
                true).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, IdentityDataSeeder.AdminRole).ConfigureAwait(false))
            {
                return RedirectToAction("Admin", "Dashboard");
            }

            return RedirectToAction("Member", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            await _auditLogger.LogSecurityEventAsync(
                "ACCOUNT_LOCKED_OUT",
                user.Id,
                user.UserName!,
                ipAddress,
                userAgent,
                "Account locked out due to multiple failed login attempts",
                false).ConfigureAwait(false);

            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
            return View(model);
        }

        await _auditLogger.LogSecurityEventAsync(
            "LOGIN_FAILED",
            user.Id,
            user.UserName!,
            ipAddress,
            userAgent,
            "Failed login attempt - Invalid password",
            false).ConfigureAwait(false);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Activate(string? token, string? email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Invalid activation request. Missing token or email.");
        }

        var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user == null || user.ActivationToken != token)
        {
            return BadRequest("Invalid or expired activation token.");
        }

        if (user.ActivationTokenExpiresAtUtc.HasValue && user.ActivationTokenExpiresAtUtc.Value < DateTime.UtcNow)
        {
            return BadRequest("Activation token has expired. Please request a new activation link from an administrator.");
        }

        string? memberName = null;
        if (!string.IsNullOrEmpty(user.ServiceNo))
        {
            var member = await _db.PersonalData.FirstOrDefaultAsync(p => p.ServiceNo == user.ServiceNo).ConfigureAwait(false);
            memberName = member?.Name;
        }

        var model = new ActivateAccountViewModel
        {
            Token = token,
            Email = email,
            ServiceNo = user.ServiceNo,
            MemberName = memberName ?? user.FirstName
        };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(ActivateAccountViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
        if (user == null || user.ActivationToken != model.Token)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired activation token.");
            return View(model);
        }

        // Remove old password and add user's chosen password
        var removePassResult = await _userManager.RemovePasswordAsync(user).ConfigureAwait(false);
        if (!removePassResult.Succeeded)
        {
            foreach (var error in removePassResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        var addPassResult = await _userManager.AddPasswordAsync(user, model.Password).ConfigureAwait(false);
        if (!addPassResult.Succeeded)
        {
            foreach (var error in addPassResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        user.EmailConfirmed = true;
        user.ActivationToken = null;
        user.ActivationTokenExpiresAtUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        await _auditLogger.LogSecurityEventAsync(
            "ACCOUNT_ACTIVATED",
            user.Id,
            user.UserName!,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Account activated with custom password for ServiceNo {user.ServiceNo}",
            true).ConfigureAwait(false);

        await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);

        TempData["SuccessMessage"] = "Your account has been activated successfully! Welcome to the IAF Batch 1986 Reunion Portal.";
        return RedirectToAction("Member", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Department = "IAF Member",
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        IdentityResult result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            const string userRole = "Member";
            if (!await _roleManager.RoleExistsAsync(userRole).ConfigureAwait(false))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = userRole }).ConfigureAwait(false);
            }

            await _userManager.AddToRoleAsync(user, userRole).ConfigureAwait(false);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            await _auditLogger.LogSecurityEventAsync(
                "MEMBER_SELF_REGISTERED",
                user.Id,
                user.UserName,
                ipAddress,
                userAgent,
                $"Member self-registered: {model.Email}",
                true).ConfigureAwait(false);

            await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);
            return RedirectToAction("Member", "Dashboard");
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User) ?? "Unknown";
        var username = User.Identity?.Name ?? "Unknown";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        await _signInManager.SignOutAsync().ConfigureAwait(false);

        await _auditLogger.LogSecurityEventAsync(
            "LOGOUT",
            userId,
            username,
            ipAddress,
            userAgent,
            "User logged out",
            true).ConfigureAwait(false);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
