namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Domain.Enums;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;
using Solution.Web.Models;

[Authorize]
public class ReunionController : Controller
{
    private readonly IReunionService _reunionService;
    private readonly IMemberService _memberService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IAuditLogger _auditLogger;

    public ReunionController(
        IReunionService reunionService,
        IMemberService memberService,
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IAuditLogger auditLogger)
    {
        _reunionService = reunionService;
        _memberService = memberService;
        _userManager = userManager;
        _db = db;
        _auditLogger = auditLogger;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        string? serviceNo = user.ServiceNo;
        if (string.IsNullOrWhiteSpace(serviceNo))
        {
            if (User.IsInRole(IdentityDataSeeder.AdminRole))
            {
                return RedirectToAction(nameof(AdminList));
            }
            TempData["ErrorMessage"] = "Your account is not linked to an IAF Service Number.";
            return RedirectToAction("Index", "Home");
        }

        var activeEvent = await _reunionService.GetActiveEventAsync();
        if (activeEvent == null)
        {
            TempData["ErrorMessage"] = "No active reunion event is currently open for registration.";
            return RedirectToAction("Member", "Dashboard");
        }

        var member = await _memberService.GetMemberDetailsAsync(serviceNo);
        var existingReg = await _reunionService.GetRegistrationAsync(activeEvent.EventId, serviceNo);

        var viewModel = new ReunionRegistrationViewModel
        {
            EventId = activeEvent.EventId,
            EventTitle = activeEvent.Title,
            EventDate = activeEvent.EventDate,
            Venue = activeEvent.Venue,
            BaseFeeAmount = activeEvent.BaseFeeAmount,
            RegistrationDeadline = activeEvent.RegistrationDeadline,
            ServiceNo = serviceNo,
            MemberName = member?.Name ?? user.FirstName,
            NoOfPersons = existingReg?.NoOfPersons ?? 1,
            AccompDetails = existingReg?.AccompDetails,
            DateOfArrival = existingReg?.DateOfArrival ?? activeEvent.EventDate.Date.AddHours(10),
            PlaceOfArrival = existingReg?.PlaceOfArrival ?? "Airport",
            Accommodation = existingReg?.Accommodation ?? "Required",
            DateOfDeparture = existingReg?.DateOfDeparture ?? (activeEvent.EventEndDate ?? activeEvent.EventDate.AddDays(2)).Date.AddHours(14),
            PlaceOfDeparture = existingReg?.PlaceOfDeparture ?? "Airport",
            TShirt = existingReg?.TShirt ?? "L",
            Drinker = existingReg?.Drinker ?? 0,
            VegNonVeg = existingReg?.VegNonVeg ?? 1,
            IsClosed = DateTime.UtcNow.Date > activeEvent.RegistrationDeadline.Date,
            CurrentStatus = existingReg?.RegistrationStatus ?? RegistrationStatus.Pending
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(ReunionRegistrationViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Enforce member can only register their own ServiceNo unless admin
        if (!User.IsInRole(IdentityDataSeeder.AdminRole) && !string.Equals(user.ServiceNo, model.ServiceNo, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var dto = new SubmitReunionRegistrationDto
        {
            EventId = model.EventId,
            ServiceNo = model.ServiceNo,
            NoOfPersons = model.NoOfPersons,
            AccompDetails = model.AccompDetails,
            DateOfArrival = model.DateOfArrival,
            PlaceOfArrival = model.PlaceOfArrival,
            Accommodation = model.Accommodation,
            DateOfDeparture = model.DateOfDeparture,
            PlaceOfDeparture = model.PlaceOfDeparture,
            TShirt = model.TShirt,
            Drinker = model.Drinker,
            VegNonVeg = model.VegNonVeg
        };

        var (success, message) = await _reunionService.RegisterOrUpdateAsync(dto);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        await _auditLogger.LogSecurityEventAsync(
            "ReunionPreferencesSaved",
            user.Id,
            user.UserName!,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Reunion preferences saved for Event {model.EventId}, ServiceNo {model.ServiceNo}, Attendees {model.NoOfPersons}",
            true);

        TempData["SuccessMessage"] = "Reunion preferences saved successfully! Please proceed to fee payment details.";
        return RedirectToAction("Submit", "Payment", new { eventId = model.EventId });
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AdminList(int? eventId = null)
    {
        var events = await _reunionService.GetAllEventsAsync();
        int selectedEventId = eventId ?? events.FirstOrDefault(e => e.IsActive)?.EventId ?? events.FirstOrDefault()?.EventId ?? 1;

        var list = await _reunionService.GetAllRegistrationsAsync(selectedEventId);
        ViewBag.Events = events;
        ViewBag.SelectedEventId = selectedEventId;

        return View(list);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int reunionId, RegistrationStatus status, int eventId)
    {
        await _reunionService.UpdateRegistrationStatusAsync(reunionId, status);
        TempData["SuccessMessage"] = $"Registration status updated to {status}.";
        return RedirectToAction(nameof(AdminList), new { eventId });
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Events()
    {
        var events = await _reunionService.GetAllEventsAsync();
        return View(events);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditEvent(int? id)
    {
        if (id.HasValue && id.Value > 0)
        {
            var ev = await _db.ReunionEvents.FindAsync([id.Value]);
            if (ev == null) return NotFound();

            return View(new ReunionEventViewModel
            {
                EventId = ev.EventId,
                Title = ev.Title,
                Batch = ev.Batch,
                EventDate = ev.EventDate,
                EventEndDate = ev.EventEndDate,
                Venue = ev.Venue,
                RegistrationDeadline = ev.RegistrationDeadline,
                BaseFeeAmount = ev.BaseFeeAmount,
                IsActive = ev.IsActive,
                Description = ev.Description
            });
        }

        return View(new ReunionEventViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(ReunionEventViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _reunionService.SaveEventAsync(new ReunionEventDto
        {
            EventId = model.EventId,
            Title = model.Title,
            Batch = model.Batch,
            EventDate = model.EventDate,
            EventEndDate = model.EventEndDate,
            Venue = model.Venue,
            RegistrationDeadline = model.RegistrationDeadline,
            BaseFeeAmount = model.BaseFeeAmount,
            IsActive = model.IsActive,
            Description = model.Description
        });

        TempData["SuccessMessage"] = "Event configuration saved successfully.";
        return RedirectToAction(nameof(Events));
    }
}
