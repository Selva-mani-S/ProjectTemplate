namespace Solution.Infrastructure.Services;

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Domain.Enums;
using Solution.Infrastructure.Persistence;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _auditLogger;

    public PaymentService(AppDbContext db, IAuditLogger auditLogger)
    {
        _db = db;
        _auditLogger = auditLogger;
    }

    public async Task<(bool Success, string Message, int PaymentId)> SubmitPaymentAsync(SubmitPaymentDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (!dto.HasPaid)
        {
            var unpaidRecord = new IafPayment
            {
                EventId = dto.EventId,
                ServiceNo = dto.ServiceNo,
                AmountPaid = 0,
                PaymentMode = PaymentMode.Other,
                PaymentDate = DateTime.Today,
                PaymentStatus = PaymentStatus.Unpaid,
                AdditionalRemarks = "Member reported fee not paid yet.",
                SubmittedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Payments.Add(unpaidRecord);
            await _db.SaveChangesAsync(ct);
            return (true, "Payment status recorded as Unpaid.", unpaidRecord.PaymentId);
        }

        if (dto.AmountPaid <= 0)
        {
            return (false, "Amount paid must be greater than zero.", 0);
        }

        if (dto.PaymentMode != PaymentMode.Cash && string.IsNullOrWhiteSpace(dto.AcknowledgementNumber))
        {
            return (false, "Acknowledgement / Transaction Number is required for this payment mode.", 0);
        }

        string? cleanAck = dto.AcknowledgementNumber?.Trim();

        var payment = new IafPayment
        {
            EventId = dto.EventId,
            ServiceNo = dto.ServiceNo,
            AmountPaid = dto.AmountPaid,
            PaymentMode = dto.PaymentMode,
            AcknowledgementNumber = cleanAck,
            PaymentDate = dto.PaymentDate,
            PaymentStatus = PaymentStatus.PendingVerification,
            PaymentProofPath = dto.PaymentProofPath,
            AdditionalRemarks = dto.AdditionalRemarks?.Trim(),
            SubmittedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        await _auditLogger.LogSecurityEventAsync(
            "PaymentSubmitted",
            dto.ServiceNo,
            dto.ServiceNo,
            "Internal",
            "WebApp",
            string.Format(CultureInfo.InvariantCulture, "Payment submitted for Event {0}: {1} {2} INR, Ack: {3}", dto.EventId, dto.PaymentMode, dto.AmountPaid, cleanAck),
            true,
            ct);

        return (true, "Payment details submitted successfully. Awaiting organizer verification.", payment.PaymentId);
    }

    public async Task<List<PaymentDto>> GetPaymentsAsync(
        int eventId,
        PaymentStatus? status = null,
        PaymentMode? mode = null,
        string? search = null,
        CancellationToken ct = default)
    {
        IQueryable<IafPayment> query = _db.Payments
            .Include(p => p.Event)
            .Include(p => p.PersonalData)
            .Where(p => p.EventId == eventId);

        if (status.HasValue)
        {
            query = query.Where(p => p.PaymentStatus == status.Value);
        }

        if (mode.HasValue)
        {
            query = query.Where(p => p.PaymentMode == mode.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim();
            query = query.Where(p =>
                p.ServiceNo.Contains(s) ||
                p.PersonalData!.Name.Contains(s) ||
                (p.AcknowledgementNumber != null && p.AcknowledgementNumber.Contains(s)));
        }

        var list = await query.OrderByDescending(p => p.SubmittedAtUtc).ToListAsync(ct);

        // Find duplicate acknowledgement numbers across all payments
        var duplicateAcks = list
            .Where(p => !string.IsNullOrWhiteSpace(p.AcknowledgementNumber))
            .GroupBy(p => p.AcknowledgementNumber!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return list.Select(p => new PaymentDto
        {
            PaymentId = p.PaymentId,
            EventId = p.EventId,
            EventTitle = p.Event?.Title ?? "Reunion",
            ServiceNo = p.ServiceNo,
            MemberName = p.PersonalData?.Name ?? string.Empty,
            AmountPaid = p.AmountPaid,
            PaymentMode = p.PaymentMode,
            AcknowledgementNumber = p.AcknowledgementNumber,
            PaymentDate = p.PaymentDate,
            PaymentStatus = p.PaymentStatus,
            PaymentProofPath = p.PaymentProofPath,
            AdditionalRemarks = p.AdditionalRemarks,
            SubmittedAtUtc = p.SubmittedAtUtc,
            VerifiedAtUtc = p.VerifiedAtUtc,
            VerifiedBy = p.VerifiedBy,
            RejectionReason = p.RejectionReason,
            IsDuplicateAckNumber = !string.IsNullOrWhiteSpace(p.AcknowledgementNumber) &&
                                  duplicateAcks.Contains(p.AcknowledgementNumber.Trim())
        }).ToList();
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId, CancellationToken ct = default)
    {
        var p = await _db.Payments
            .Include(x => x.Event)
            .Include(x => x.PersonalData)
            .FirstOrDefaultAsync(x => x.PaymentId == paymentId, ct);

        if (p == null) return null;

        bool isDup = false;
        if (!string.IsNullOrWhiteSpace(p.AcknowledgementNumber))
        {
            isDup = await _db.Payments.AnyAsync(x =>
                x.PaymentId != paymentId &&
                x.AcknowledgementNumber == p.AcknowledgementNumber, ct);
        }

        return new PaymentDto
        {
            PaymentId = p.PaymentId,
            EventId = p.EventId,
            EventTitle = p.Event?.Title ?? "Reunion",
            ServiceNo = p.ServiceNo,
            MemberName = p.PersonalData?.Name ?? string.Empty,
            AmountPaid = p.AmountPaid,
            PaymentMode = p.PaymentMode,
            AcknowledgementNumber = p.AcknowledgementNumber,
            PaymentDate = p.PaymentDate,
            PaymentStatus = p.PaymentStatus,
            PaymentProofPath = p.PaymentProofPath,
            AdditionalRemarks = p.AdditionalRemarks,
            SubmittedAtUtc = p.SubmittedAtUtc,
            VerifiedAtUtc = p.VerifiedAtUtc,
            VerifiedBy = p.VerifiedBy,
            RejectionReason = p.RejectionReason,
            IsDuplicateAckNumber = isDup
        };
    }

    public async Task<PaymentDto?> GetMemberPaymentAsync(int eventId, string serviceNo, CancellationToken ct = default)
    {
        var p = await _db.Payments
            .Include(x => x.Event)
            .Include(x => x.PersonalData)
            .Where(x => x.EventId == eventId && x.ServiceNo == serviceNo)
            .OrderByDescending(x => x.PaymentId)
            .FirstOrDefaultAsync(ct);

        if (p == null) return null;

        return new PaymentDto
        {
            PaymentId = p.PaymentId,
            EventId = p.EventId,
            EventTitle = p.Event?.Title ?? "Reunion",
            ServiceNo = p.ServiceNo,
            MemberName = p.PersonalData?.Name ?? string.Empty,
            AmountPaid = p.AmountPaid,
            PaymentMode = p.PaymentMode,
            AcknowledgementNumber = p.AcknowledgementNumber,
            PaymentDate = p.PaymentDate,
            PaymentStatus = p.PaymentStatus,
            PaymentProofPath = p.PaymentProofPath,
            AdditionalRemarks = p.AdditionalRemarks,
            SubmittedAtUtc = p.SubmittedAtUtc,
            VerifiedAtUtc = p.VerifiedAtUtc,
            VerifiedBy = p.VerifiedBy,
            RejectionReason = p.RejectionReason
        };
    }

    public async Task<(bool Success, string Message)> VerifyPaymentAsync(VerifyPaymentDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var payment = await _db.Payments.FindAsync([dto.PaymentId], ct);
        if (payment == null)
        {
            return (false, "Payment record not found.");
        }

        if (dto.IsApproved)
        {
            payment.PaymentStatus = PaymentStatus.Verified;
            payment.RejectionReason = null;
        }
        else
        {
            payment.PaymentStatus = PaymentStatus.Rejected;
            payment.RejectionReason = string.IsNullOrWhiteSpace(dto.RejectionReason)
                ? "Payment details could not be confirmed."
                : dto.RejectionReason.Trim();
        }

        payment.VerifiedAtUtc = DateTime.UtcNow;
        payment.VerifiedBy = dto.VerifierName;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _auditLogger.LogSecurityEventAsync(
            "PaymentVerification",
            dto.VerifierName,
            dto.VerifierName,
            "Internal",
            "WebApp",
            string.Format(CultureInfo.InvariantCulture, "Payment {0} for {1} set to {2}. Reason: {3}", payment.PaymentId, payment.ServiceNo, payment.PaymentStatus, payment.RejectionReason),
            true,
            ct);

        return (true, string.Format(CultureInfo.InvariantCulture, "Payment has been marked as {0}.", payment.PaymentStatus));
    }

    public async Task<bool> CheckDuplicateAckNumberAsync(string ackNumber, int? excludePaymentId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ackNumber)) return false;

        string clean = ackNumber.Trim();
        return await _db.Payments.AnyAsync(p =>
            (!excludePaymentId.HasValue || p.PaymentId != excludePaymentId.Value) &&
            p.AcknowledgementNumber != null &&
            p.AcknowledgementNumber == clean, ct);
    }
}
