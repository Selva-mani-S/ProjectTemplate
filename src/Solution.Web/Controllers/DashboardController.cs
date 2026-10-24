namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Solution.Application.Common.Interfaces;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IReunionService _reunionService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public DashboardController(
        IDashboardService dashboardService,
        IReunionService reunionService,
        UserManager<ApplicationUser> userManager,
        AppDbContext db)
    {
        _dashboardService = dashboardService;
        _reunionService = reunionService;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Admin(int? eventId = null)
    {
        var events = await _reunionService.GetAllEventsAsync();
        int selectedEventId = eventId ?? events.FirstOrDefault(e => e.IsActive)?.EventId ?? events.FirstOrDefault()?.EventId ?? 1;

        var stats = await _dashboardService.GetAdminStatsAsync(selectedEventId);
        ViewBag.Events = events;
        ViewBag.SelectedEventId = selectedEventId;

        return View(stats);
    }

    [HttpGet]
    public async Task<IActionResult> Member()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        string? serviceNo = user.ServiceNo;
        if (string.IsNullOrWhiteSpace(serviceNo))
        {
            if (User.IsInRole(IdentityDataSeeder.AdminRole))
            {
                return RedirectToAction(nameof(Admin));
            }
            TempData["ErrorMessage"] = "Your user account is not linked to any member Service Number.";
            return RedirectToAction("Index", "Home");
        }

        var stats = await _dashboardService.GetMemberStatsAsync(serviceNo);
        return View(stats);
    }
}
