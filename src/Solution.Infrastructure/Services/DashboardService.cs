namespace Solution.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Enums;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemberService _memberService;
    private readonly IReunionService _reunionService;

    public DashboardService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IMemberService memberService,
        IReunionService reunionService)
    {
        _db = db;
        _userManager = userManager;
        _memberService = memberService;
        _reunionService = reunionService;
    }

    public async Task<AdminDashboardStatsDto> GetAdminStatsAsync(int eventId, CancellationToken ct = default)
    {
        var stats = new AdminDashboardStatsDto();

        // 1. Member stats
        stats.TotalMembersInRegistry = await _db.PersonalData.CountAsync(ct);

        var users = await _userManager.Users.ToListAsync(ct);
        stats.TotalAccountsCreated = users.Count(u => u.ServiceNo != null);
        stats.ActiveAccountsCount = users.Count(u => u.ServiceNo != null && u.IsActive);
        stats.InactiveAccountsCount = users.Count(u => u.ServiceNo != null && !u.IsActive);
        stats.UnactivatedAccountsCount = users.Count(u => u.ServiceNo != null && !u.EmailConfirmed);

        stats.IncompleteProfilesCount = await _db.PersonalData
            .CountAsync(p => string.IsNullOrWhiteSpace(p.MobileNo) ||
                             string.IsNullOrWhiteSpace(p.TradeId) ||
                             string.IsNullOrWhiteSpace(p.StateId), ct);

        stats.StateWiseMemberDistribution = await _db.PersonalData
            .Where(p => p.State != null)
            .GroupBy(p => p.State!.StateName)
            .Select(g => new { State = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.State, x => x.Count, ct);

        stats.TradeWiseMemberDistribution = await _db.PersonalData
            .Where(p => p.Trade != null)
            .GroupBy(p => p.Trade!.TradeName)
            .Select(g => new { Trade = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.Trade, x => x.Count, ct);

        // 2. Reunion registration stats
        var eventObj = await _db.ReunionEvents.FindAsync([eventId], ct);
        decimal baseFee = eventObj?.BaseFeeAmount ?? 5000.00m;

        var registrations = await _db.Reunions
            .Where(r => r.EventId == eventId)
            .ToListAsync(ct);

        stats.TotalRegisteredForReunion = registrations.Count;
        stats.ConfirmedRegistrationsCount = registrations.Count(r => r.RegistrationStatus == RegistrationStatus.Confirmed);
        stats.PendingRegistrationsCount = registrations.Count(r => r.RegistrationStatus == RegistrationStatus.Pending || r.RegistrationStatus == RegistrationStatus.Submitted);
        stats.CancelledRegistrationsCount = registrations.Count(r => r.RegistrationStatus == RegistrationStatus.Cancelled);
        stats.UnregisteredMembersCount = Math.Max(0, stats.TotalMembersInRegistry - stats.TotalRegisteredForReunion);

        stats.TotalExpectedAttendees = registrations.Sum(r => Math.Max(1, r.NoOfPersons));
        stats.TotalAccompanyingPersons = registrations.Sum(r => Math.Max(0, r.NoOfPersons - 1));
        stats.AccommodationRequiredCount = registrations.Count(r => !string.IsNullOrWhiteSpace(r.Accommodation) && r.Accommodation != "None");

        stats.TShirtSizeDistribution = registrations
            .Where(r => !string.IsNullOrWhiteSpace(r.TShirt))
            .GroupBy(r => r.TShirt!)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.VegetarianMealsCount = registrations.Count(r => r.VegNonVeg == 0);
        stats.NonVegetarianMealsCount = registrations.Count(r => r.VegNonVeg == 1);
        stats.DrinkerCount = registrations.Count(r => r.Drinker == 1);
        stats.NonDrinkerCount = registrations.Count(r => r.Drinker == 0);

        stats.ArrivalPlaceDistribution = registrations
            .Where(r => !string.IsNullOrWhiteSpace(r.PlaceOfArrival))
            .GroupBy(r => r.PlaceOfArrival!)
            .ToDictionary(g => g.Key, g => g.Count());

        // 3. Payment stats
        var payments = await _db.Payments
            .Where(p => p.EventId == eventId)
            .ToListAsync(ct);

        stats.MembersReportedPaid = payments.Count(p => p.PaymentStatus != PaymentStatus.Unpaid && p.AmountPaid > 0);
        stats.PaymentsVerifiedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Verified);
        stats.PaymentsPendingVerificationCount = payments.Count(p => p.PaymentStatus == PaymentStatus.PendingVerification);
        stats.PaymentsRejectedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Rejected);
        stats.UnpaidMembersCount = stats.TotalMembersInRegistry - stats.PaymentsVerifiedCount;

        stats.TotalVerifiedAmountCollected = payments
            .Where(p => p.PaymentStatus == PaymentStatus.Verified)
            .Sum(p => p.AmountPaid);

        stats.TotalReportedAmount = payments
            .Where(p => p.PaymentStatus != PaymentStatus.Unpaid && p.PaymentStatus != PaymentStatus.Rejected)
            .Sum(p => p.AmountPaid);

        decimal totalExpectedFromRegistered = registrations.Count * baseFee;
        stats.TotalOutstandingAmount = Math.Max(0, totalExpectedFromRegistered - stats.TotalVerifiedAmountCollected);

        stats.PaymentModeDistribution = payments
            .Where(p => p.PaymentStatus == PaymentStatus.Verified)
            .GroupBy(p => p.PaymentMode.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    public async Task<MemberDashboardStatsDto> GetMemberStatsAsync(string serviceNo, CancellationToken ct = default)
    {
        var memberDetails = await _memberService.GetMemberDetailsAsync(serviceNo, ct);
        var activeEvent = await _reunionService.GetActiveEventAsync(ct);

        var dto = new MemberDashboardStatsDto();

        if (memberDetails != null)
        {
            dto.MemberSummary = new MemberSummaryDto
            {
                ServiceNo = memberDetails.ServiceNo,
                Name = memberDetails.Name,
                TradeId = memberDetails.TradeId,
                TradeName = memberDetails.TradeName,
                StateId = memberDetails.StateId,
                StateName = memberDetails.StateName,
                MobileNo = memberDetails.MobileNo,
                HasUserAccount = memberDetails.HasAccount,
                IsAccountActive = memberDetails.IsAccountActive,
                HasRegisteredForReunion = memberDetails.CurrentRegistration != null,
                RegistrationStatus = memberDetails.CurrentRegistration?.RegistrationStatus.ToString() ?? "Not Registered",
                PaymentStatus = memberDetails.Payments.FirstOrDefault()?.PaymentStatus.ToString() ?? "Unpaid"
            };
        }

        if (activeEvent != null)
        {
            dto.ActiveEvent = new ReunionEventDto
            {
                EventId = activeEvent.EventId,
                Title = activeEvent.Title,
                Batch = activeEvent.Batch,
                EventDate = activeEvent.EventDate,
                EventEndDate = activeEvent.EventEndDate,
                Venue = activeEvent.Venue,
                RegistrationDeadline = activeEvent.RegistrationDeadline,
                BaseFeeAmount = activeEvent.BaseFeeAmount,
                IsActive = activeEvent.IsActive,
                Description = activeEvent.Description
            };

            dto.Registration = await _reunionService.GetRegistrationAsync(activeEvent.EventId, serviceNo, ct);
            dto.LatestPayment = memberDetails?.Payments.FirstOrDefault(p => p.EventId == activeEvent.EventId);
        }

        dto.Announcements = await _db.Announcements
            .Where(a => a.IsPublished && (a.EventId == null || (activeEvent != null && a.EventId == activeEvent.EventId)))
            .OrderByDescending(a => a.PublishedAtUtc)
            .Take(5)
            .Select(a => new IafAnnouncementDto
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                PublishedAtUtc = a.PublishedAtUtc,
                AuthorName = a.AuthorName
            })
            .ToListAsync(ct);

        return dto;
    }
}
