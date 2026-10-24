namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Solution.Application.Common.Interfaces;
using Solution.Web.Models;

[Authorize(Roles = "Administrator")]
public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly IReunionService _reunionService;

    public ReportController(IReportService reportService, IReunionService reunionService)
    {
        _reportService = reportService;
        _reunionService = reunionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? eventId = null)
    {
        var events = await _reunionService.GetAllEventsAsync();
        int activeEventId = eventId ?? events.FirstOrDefault(e => e.IsActive)?.EventId ?? events.FirstOrDefault()?.EventId ?? 1;
        var activeEvent = events.FirstOrDefault(e => e.EventId == activeEventId);

        var viewModel = new ReportListViewModel
        {
            ActiveEventId = activeEventId,
            ActiveEventTitle = activeEvent?.Title ?? "IAF 1986 Reunion",
            Reports =
            [
                new ReportItemViewModel { ReportNumber = 1, Title = "1. Complete Member Registry", Description = "Full roster of all IAF Batch 1986 members, contact info, trades, and states.", Category = "Registry" },
                new ReportItemViewModel { ReportNumber = 2, Title = "2. Members Without Activated Accounts", Description = "Members who haven't yet set their passwords or activated their online accounts.", Category = "Registry" },
                new ReportItemViewModel { ReportNumber = 3, Title = "3. Members Who Have Not Registered", Description = "List of members who have not yet submitted reunion attendance preferences.", Category = "Registry" },

                new ReportItemViewModel { ReportNumber = 4, Title = "4. Reunion Registration Summary", Description = "Consolidated summary of attendee counts, arrival dates, and registration status.", Category = "Logistics" },
                new ReportItemViewModel { ReportNumber = 5, Title = "5. Expected Attendance & Accompanying Persons", Description = "Headcount breakdown of members, spouses, children, and guests.", Category = "Logistics" },
                new ReportItemViewModel { ReportNumber = 6, Title = "6. Arrival and Departure Schedule", Description = "Travel itineraries, arrival times, and pickup/drop-off transit points.", Category = "Logistics" },
                new ReportItemViewModel { ReportNumber = 7, Title = "7. Accommodation Requirements", Description = "Rooms, guesthouses, and hotel requirements for outstation members.", Category = "Logistics" },
                new ReportItemViewModel { ReportNumber = 8, Title = "8. T-Shirt Size Distribution", Description = "T-shirt requirements breakdown (M, L, XL, XXL) by member.", Category = "Logistics" },
                new ReportItemViewModel { ReportNumber = 9, Title = "9. Food and Drink Preference Summary", Description = "Catering requirements: Vegetarian vs Non-Vegetarian, Drinkers vs Non-Drinkers.", Category = "Logistics" },

                new ReportItemViewModel { ReportNumber = 10, Title = "10. Payment Status Summary", Description = "Payment status overview (Unpaid, Pending, Verified, Rejected) by member.", Category = "Financial" },
                new ReportItemViewModel { ReportNumber = 11, Title = "11. Verified Payment Collection Report", Description = "Audited financial ledger of verified transaction IDs, modes, and dates.", Category = "Financial" },
                new ReportItemViewModel { ReportNumber = 12, Title = "12. Outstanding Fee Report", Description = "List of registered members with unpaid or pending reunion fee balances.", Category = "Financial" },
                new ReportItemViewModel { ReportNumber = 13, Title = "13. Payment Mode Summary", Description = "Total collections and transaction counts grouped by payment mode (UPI, Bank, Cash).", Category = "Financial" },
                new ReportItemViewModel { ReportNumber = 14, Title = "14. Pending & Rejected Verification Report", Description = "Submissions requiring administrator review, follow-up, or resubmission.", Category = "Financial" }
            ]
        };

        ViewBag.Events = events;
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(int reportNumber, int eventId)
    {
        byte[] bytes = await _reportService.GenerateReportExcelAsync(reportNumber, eventId);
        string name = _reportService.GetReportName(reportNumber);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{name}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(int reportNumber, int eventId)
    {
        byte[] bytes = await _reportService.GenerateReportCsvAsync(reportNumber, eventId);
        string name = _reportService.GetReportName(reportNumber);
        return File(bytes, "text/csv", $"{name}.csv");
    }
}
