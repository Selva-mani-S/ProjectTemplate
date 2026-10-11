namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Solution.Application.Common.Interfaces;
using Solution.Infrastructure.Identity;
using Solution.Web.Models;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogger _auditLogger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuditLogger auditLogger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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
                "Failed login attempt - User not found",
                false).ConfigureAwait(false);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true).ConfigureAwait(false);

        if (result.Succeeded)
        {
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

            return RedirectToAction("Index", "Home");
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
