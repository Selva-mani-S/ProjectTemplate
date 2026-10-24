namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Enums;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;
using Solution.Web.Models;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IReunionService _reunionService;
    private readonly IMemberService _memberService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _db;
    private readonly IAuditLogger _auditLogger;

    public PaymentController(
        IPaymentService paymentService,
        IReunionService reunionService,
        IMemberService memberService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        AppDbContext db,
        IAuditLogger auditLogger)
    {
        _paymentService = paymentService;
        _reunionService = reunionService;
        _memberService = memberService;
        _userManager = userManager;
        _env = env;
        _db = db;
        _auditLogger = auditLogger;
    }

    [HttpGet]
    public async Task<IActionResult> Submit(int? eventId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        string? serviceNo = user.ServiceNo;
        if (string.IsNullOrWhiteSpace(serviceNo))
        {
            TempData["ErrorMessage"] = "Your account is not linked to an IAF Service Number.";
            return RedirectToAction("Index", "Home");
        }

        var activeEvent = eventId.HasValue
            ? await _db.ReunionEvents.FindAsync([eventId.Value])
            : await _reunionService.GetActiveEventAsync();

        if (activeEvent == null)
        {
            TempData["ErrorMessage"] = "No active reunion event was found.";
            return RedirectToAction("Member", "Dashboard");
        }

        var member = await _memberService.GetMemberDetailsAsync(serviceNo);
        var existingPmt = await _paymentService.GetMemberPaymentAsync(activeEvent.EventId, serviceNo);

        var model = new PaymentSubmissionViewModel
        {
            EventId = activeEvent.EventId,
            EventTitle = activeEvent.Title,
            ConfiguredFeeAmount = activeEvent.BaseFeeAmount,
            ServiceNo = serviceNo,
            MemberName = member?.Name ?? user.FirstName,
            AmountPaid = existingPmt?.AmountPaid > 0 ? existingPmt.AmountPaid : activeEvent.BaseFeeAmount,
            PaymentDate = existingPmt?.PaymentDate ?? DateTime.Today,
            PaymentMode = existingPmt?.PaymentMode ?? PaymentMode.UPI,
            AcknowledgementNumber = existingPmt?.AcknowledgementNumber,
            AdditionalRemarks = existingPmt?.AdditionalRemarks,
            HasPaidResponse = (existingPmt != null && existingPmt.PaymentStatus != PaymentStatus.Unpaid) ? "Yes" : "No",
            ExistingPayment = existingPmt
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(PaymentSubmissionViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!User.IsInRole(IdentityDataSeeder.AdminRole) && !string.Equals(user.ServiceNo, model.ServiceNo, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        bool hasPaid = string.Equals(model.HasPaidResponse, "Yes", StringComparison.OrdinalIgnoreCase);

        string? proofPath = null;
        if (model.PaymentProofFile != null && model.PaymentProofFile.Length > 0)
        {
            // Security: validate extension
            string ext = Path.GetExtension(model.PaymentProofFile.FileName).ToLowerInvariant();
            string[] allowedExts = [".jpg", ".jpeg", ".png", ".pdf"];
            if (!allowedExts.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.PaymentProofFile), "Allowed proof formats: JPG, PNG, PDF.");
                return View(model);
            }

            if (model.PaymentProofFile.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.PaymentProofFile), "Proof file must be under 10 MB.");
                return View(model);
            }

            string uploadsDir = Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "proofs");
            Directory.CreateDirectory(uploadsDir);

            string safeFileName = $"{model.ServiceNo}_{model.EventId}_{Guid.NewGuid():N}{ext}";
            string fullPath = Path.Combine(uploadsDir, safeFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await model.PaymentProofFile.CopyToAsync(stream);
            }

            proofPath = safeFileName;
        }

        var dto = new SubmitPaymentDto
        {
            EventId = model.EventId,
            ServiceNo = model.ServiceNo,
            HasPaid = hasPaid,
            AmountPaid = hasPaid ? model.AmountPaid : 0,
            PaymentMode = model.PaymentMode,
            AcknowledgementNumber = model.AcknowledgementNumber,
            PaymentDate = model.PaymentDate,
            PaymentProofPath = proofPath,
            AdditionalRemarks = model.AdditionalRemarks
        };

        var (success, message, paymentId) = await _paymentService.SubmitPaymentAsync(dto);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        TempData["SuccessMessage"] = message;
        return RedirectToAction("Member", "Dashboard");
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AdminList(
        int? eventId = null,
        PaymentStatus? status = null,
        PaymentMode? mode = null,
        string? search = null)
    {
        var events = await _reunionService.GetAllEventsAsync();
        int selectedEventId = eventId ?? events.FirstOrDefault(e => e.IsActive)?.EventId ?? events.FirstOrDefault()?.EventId ?? 1;

        var payments = await _paymentService.GetPaymentsAsync(selectedEventId, status, mode, search);

        var viewModel = new PaymentAdminIndexViewModel
        {
            EventId = selectedEventId,
            Payments = payments,
            SelectedStatus = status,
            SelectedMode = mode,
            Search = search,
            TotalVerifiedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Verified),
            TotalPendingCount = payments.Count(p => p.PaymentStatus == PaymentStatus.PendingVerification),
            TotalRejectedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Rejected),
            TotalVerifiedAmount = payments.Where(p => p.PaymentStatus == PaymentStatus.Verified).Sum(p => p.AmountPaid)
        };

        ViewBag.Events = events;
        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(VerifyPaymentSubmissionModel model, int eventId)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = new VerifyPaymentDto
        {
            PaymentId = model.PaymentId,
            IsApproved = model.IsApproved,
            RejectionReason = model.RejectionReason,
            VerifierName = User.Identity?.Name ?? "Administrator"
        };

        var (success, message) = await _paymentService.VerifyPaymentAsync(dto);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(AdminList), new { eventId });
    }

    [HttpGet]
    public async Task<IActionResult> Proof(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null || string.IsNullOrWhiteSpace(payment.PaymentProofPath))
        {
            return NotFound();
        }

        // Authorization check: User must be Admin or the owner of the payment
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!User.IsInRole(IdentityDataSeeder.AdminRole) && !string.Equals(user.ServiceNo, payment.ServiceNo, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        string fullPath = Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "proofs", payment.PaymentProofPath);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound("Proof file not found on disk.");
        }

        string ext = Path.GetExtension(fullPath).ToLowerInvariant();
        string contentType = ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        return PhysicalFile(fullPath, contentType);
    }
}
