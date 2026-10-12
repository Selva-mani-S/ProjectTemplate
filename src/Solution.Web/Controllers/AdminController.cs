namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Infrastructure.Identity;
using Solution.Web.Models;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditLogger _auditLogger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IAuditLogger auditLogger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditLogger = auditLogger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? departmentFilter = null)
    {
        IQueryable<ApplicationUser> query = _userManager.Users;

        if (!string.IsNullOrWhiteSpace(departmentFilter))
        {
            query = query.Where(u => u.Department == departmentFilter);
        }

        List<ApplicationUser> users = await query.ToListAsync().ConfigureAwait(false);
        var userDtos = new List<UserSummaryDto>();

        foreach (var u in users)
        {
            IList<string> roles = await _userManager.GetRolesAsync(u).ConfigureAwait(false);
            userDtos.Add(new UserSummaryDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Department = string.IsNullOrWhiteSpace(u.Department) ? "Unassigned" : u.Department,
                Roles = string.Join(", ", roles),
                EmailConfirmed = u.EmailConfirmed,
                CreatedAtUtc = u.CreatedAtUtc
            });
        }

        List<string> allDepartments = await _userManager.Users
            .Where(u => !string.IsNullOrEmpty(u.Department))
            .Select(u => u.Department)
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);

        var viewModel = new UserManagementViewModel
        {
            Users = userDtos,
            SelectedDepartmentFilter = departmentFilter,
            AvailableDepartments = allDepartments
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
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
            Department = model.Department,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        IdentityResult result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            string targetRole = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role;
            if (!await _roleManager.RoleExistsAsync(targetRole).ConfigureAwait(false))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = targetRole }).ConfigureAwait(false);
            }

            await _userManager.AddToRoleAsync(user, targetRole).ConfigureAwait(false);

            var adminId = _userManager.GetUserId(User) ?? "Unknown";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            await _auditLogger.LogSecurityEventAsync(
                "USER_CREATED_BY_ADMIN",
                adminId,
                User.Identity?.Name ?? "Admin",
                ipAddress,
                userAgent,
                $"Admin created user {model.Email} assigned to department {model.Department} and role {targetRole}",
                true).ConfigureAwait(false);

            TempData["SuccessMessage"] = $"User {model.Email} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDepartment(EditUserDepartmentViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(model.UserId).ConfigureAwait(false);
        if (user == null)
        {
            return NotFound();
        }

        string oldDept = user.Department;
        user.Department = model.Department;
        IdentityResult result = await _userManager.UpdateAsync(user).ConfigureAwait(false);

        if (result.Succeeded)
        {
            var adminId = _userManager.GetUserId(User) ?? "Unknown";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            await _auditLogger.LogSecurityEventAsync(
                "USER_DEPARTMENT_UPDATED",
                adminId,
                User.Identity?.Name ?? "Admin",
                ipAddress,
                userAgent,
                $"Updated user {user.Email} department from '{oldDept}' to '{model.Department}'",
                true).ConfigureAwait(false);

            TempData["SuccessMessage"] = $"Department updated for {user.Email}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
