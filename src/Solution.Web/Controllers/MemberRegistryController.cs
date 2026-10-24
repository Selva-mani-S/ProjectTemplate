namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Web.Models;

[Authorize(Roles = "Administrator")]
public class MemberRegistryController : Controller
{
    private readonly IMemberService _memberService;
    private readonly IImportService _importService;
    private readonly IAuditLogger _auditLogger;

    public MemberRegistryController(
        IMemberService memberService,
        IImportService importService,
        IAuditLogger auditLogger)
    {
        _memberService = memberService;
        _importService = importService;
        _auditLogger = auditLogger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search = null,
        string? tradeId = null,
        string? stateId = null,
        bool? unactivatedOnly = null,
        bool? unregisteredOnly = null,
        int page = 1)
    {
        const int pageSize = 15;
        var (members, totalCount) = await _memberService.GetMembersAsync(
            search, tradeId, stateId, unactivatedOnly, unregisteredOnly, page, pageSize);

        var viewModel = new MemberRegistryIndexViewModel
        {
            Members = members,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Search = search,
            TradeId = tradeId,
            StateId = stateId,
            UnactivatedOnly = unactivatedOnly,
            UnregisteredOnly = unregisteredOnly,
            Trades = await _memberService.GetTradesAsync(),
            States = await _memberService.GetStatesAsync()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var member = await _memberService.GetMemberDetailsAsync(id);
        if (member == null) return NotFound();

        return View(member);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new MemberFormViewModel
        {
            IsEdit = false,
            AvailableTrades = await _memberService.GetTradesAsync(),
            AvailableStates = await _memberService.GetStatesAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberFormViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            model.AvailableTrades = await _memberService.GetTradesAsync();
            model.AvailableStates = await _memberService.GetStatesAsync();
            return View(model);
        }

        var dto = new CreateMemberDto
        {
            ServiceNo = model.ServiceNo.Trim(),
            Name = model.Name.Trim(),
            Dob = model.Dob,
            Dod = model.Dod,
            TradeId = model.TradeId,
            Address1 = model.Address1,
            Address2 = model.Address2,
            StateId = model.StateId,
            MobileNo = model.MobileNo,
            EmailId = model.EmailId,
            TechnicalSkills = model.TechnicalSkills,
            PlacesWorked = model.PlacesWorked,
            AdditionalDetails = model.AdditionalDetails,
            Achievements = model.Achievements,
            VrsReason = model.VrsReason
        };

        bool created = await _memberService.CreateMemberAsync(dto);
        if (!created)
        {
            ModelState.AddModelError(nameof(model.ServiceNo), "A member with this Service Number already exists.");
            model.AvailableTrades = await _memberService.GetTradesAsync();
            model.AvailableStates = await _memberService.GetStatesAsync();
            return View(model);
        }

        await _auditLogger.LogSecurityEventAsync(
            "MemberCreated",
            User.Identity?.Name ?? "Admin",
            User.Identity?.Name ?? "Admin",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Member created: {dto.ServiceNo} ({dto.Name})",
            true);

        TempData["SuccessMessage"] = $"Member {dto.ServiceNo} ({dto.Name}) created successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.ServiceNo });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var m = await _memberService.GetMemberDetailsAsync(id);
        if (m == null) return NotFound();

        var model = new MemberFormViewModel
        {
            IsEdit = true,
            ServiceNo = m.ServiceNo,
            Name = m.Name,
            Dob = m.Dob,
            Dod = m.Dod,
            TradeId = m.TradeId,
            Address1 = m.Address1,
            Address2 = m.Address2,
            StateId = m.StateId,
            MobileNo = m.MobileNo,
            EmailId = m.EmailId,
            TechnicalSkills = m.TechnicalSkills,
            PlacesWorked = m.PlacesWorked,
            AdditionalDetails = m.AdditionalDetails,
            Achievements = m.Achievements,
            VrsReason = m.VrsReason,
            AvailableTrades = await _memberService.GetTradesAsync(),
            AvailableStates = await _memberService.GetStatesAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MemberFormViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            model.AvailableTrades = await _memberService.GetTradesAsync();
            model.AvailableStates = await _memberService.GetStatesAsync();
            return View(model);
        }

        var dto = new CreateMemberDto
        {
            ServiceNo = model.ServiceNo.Trim(),
            Name = model.Name.Trim(),
            Dob = model.Dob,
            Dod = model.Dod,
            TradeId = model.TradeId,
            Address1 = model.Address1,
            Address2 = model.Address2,
            StateId = model.StateId,
            MobileNo = model.MobileNo,
            EmailId = model.EmailId,
            TechnicalSkills = model.TechnicalSkills,
            PlacesWorked = model.PlacesWorked,
            AdditionalDetails = model.AdditionalDetails,
            Achievements = model.Achievements,
            VrsReason = model.VrsReason
        };

        bool updated = await _memberService.UpdateMemberAsync(dto);
        if (!updated)
        {
            return NotFound();
        }

        await _auditLogger.LogSecurityEventAsync(
            "MemberUpdated",
            User.Identity?.Name ?? "Admin",
            User.Identity?.Name ?? "Admin",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Member updated: {dto.ServiceNo} ({dto.Name})",
            true);

        TempData["SuccessMessage"] = $"Member {dto.ServiceNo} updated successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.ServiceNo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string serviceNo)
    {
        if (string.IsNullOrWhiteSpace(serviceNo)) return BadRequest();

        bool deleted = await _memberService.DeleteMemberAsync(serviceNo);
        if (!deleted) return NotFound();

        await _auditLogger.LogSecurityEventAsync(
            "MemberDeleted",
            User.Identity?.Name ?? "Admin",
            User.Identity?.Name ?? "Admin",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Member deleted: {serviceNo}",
            true);

        TempData["SuccessMessage"] = $"Member {serviceNo} deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View(new MemberImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(MemberImportViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.File == null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Please select a valid Excel or CSV file to import.");
            return View(model);
        }

        using var stream = model.File.OpenReadStream();
        var preview = await _importService.ParseAndValidateFileAsync(stream, model.File.FileName);
        model.Preview = preview;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport([FromBody] List<ImportRowDto> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            return BadRequest(new { success = false, message = "No records to import." });
        }

        var result = await _importService.CommitImportAsync(rows);

        await _auditLogger.LogSecurityEventAsync(
            "BulkMemberImport",
            User.Identity?.Name ?? "Admin",
            User.Identity?.Name ?? "Admin",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Request.Headers.UserAgent.ToString(),
            $"Imported {result.SuccessfullyImported} members, skipped {result.SkippedDuplicates} duplicates, failed {result.FailedCount}",
            result.Success);

        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadErrorReport([FromBody] List<ImportRowDto> invalidRows)
    {
        if (invalidRows == null || invalidRows.Count == 0) return BadRequest();

        byte[] bytes = await _importService.GenerateErrorReportExcelAsync(invalidRows);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Member_Import_Errors.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFamily(AddFamilyMemberViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid family member details.";
            return RedirectToAction(nameof(Details), new { id = model.ServiceNo });
        }

        await _memberService.AddFamilyMemberAsync(new FamilyMemberDto
        {
            ServiceNo = model.ServiceNo,
            MemberName = model.MemberName,
            Relationship = model.Relationship,
            Gender = model.Gender,
            Occupation = model.Occupation,
            Remarks = model.Remarks
        });

        TempData["SuccessMessage"] = "Family member added successfully.";
        return RedirectToAction(nameof(Details), new { id = model.ServiceNo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFamily(int familyMemberId, string serviceNo)
    {
        await _memberService.RemoveFamilyMemberAsync(familyMemberId);
        TempData["SuccessMessage"] = "Family member removed.";
        return RedirectToAction(nameof(Details), new { id = serviceNo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(string serviceNo, int photoIndex, IFormFile? photoFile)
    {
        if (photoFile == null || photoFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select an image file.";
            return RedirectToAction(nameof(Details), new { id = serviceNo });
        }

        if (photoFile.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Image must be under 5 MB.";
            return RedirectToAction(nameof(Details), new { id = serviceNo });
        }

        using var ms = new MemoryStream();
        await photoFile.CopyToAsync(ms);
        await _memberService.SavePhotoAsync(serviceNo, photoIndex, ms.ToArray());

        TempData["SuccessMessage"] = $"Photograph {photoIndex} uploaded successfully.";
        return RedirectToAction(nameof(Details), new { id = serviceNo });
    }

    [HttpGet]
    public async Task<IActionResult> Photo(string serviceNo, int index)
    {
        byte[]? bytes = await _memberService.GetPhotoAsync(serviceNo, index);
        if (bytes == null || bytes.Length == 0) return NotFound();

        return File(bytes, "image/jpeg");
    }
}
