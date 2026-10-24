namespace Solution.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Domain.Enums;
using Solution.Infrastructure.Persistence;

public class ReunionService : IReunionService
{
    private readonly AppDbContext _db;

    public ReunionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IafReunionEvent?> GetActiveEventAsync(CancellationToken ct = default)
    {
        return await _db.ReunionEvents
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.EventDate)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ReunionEventDto>> GetAllEventsAsync(CancellationToken ct = default)
    {
        return await _db.ReunionEvents
            .OrderByDescending(e => e.EventDate)
            .Select(e => new ReunionEventDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Batch = e.Batch,
                EventDate = e.EventDate,
                EventEndDate = e.EventEndDate,
                Venue = e.Venue,
                RegistrationDeadline = e.RegistrationDeadline,
                BaseFeeAmount = e.BaseFeeAmount,
                IsActive = e.IsActive,
                Description = e.Description
            })
            .ToListAsync(ct);
    }

    public async Task<bool> SaveEventAsync(ReunionEventDto dto, CancellationToken ct = default)
    {
        if (dto.EventId > 0)
        {
            var ev = await _db.ReunionEvents.FindAsync([dto.EventId], ct);
            if (ev == null) return false;

            ev.Title = dto.Title.Trim();
            ev.Batch = dto.Batch.Trim();
            ev.EventDate = dto.EventDate;
            ev.EventEndDate = dto.EventEndDate;
            ev.Venue = dto.Venue.Trim();
            ev.RegistrationDeadline = dto.RegistrationDeadline;
            ev.BaseFeeAmount = dto.BaseFeeAmount;
            ev.IsActive = dto.IsActive;
            ev.Description = dto.Description?.Trim();
        }
        else
        {
            var ev = new IafReunionEvent
            {
                Title = dto.Title.Trim(),
                Batch = dto.Batch.Trim(),
                EventDate = dto.EventDate,
                EventEndDate = dto.EventEndDate,
                Venue = dto.Venue.Trim(),
                RegistrationDeadline = dto.RegistrationDeadline,
                BaseFeeAmount = dto.BaseFeeAmount,
                IsActive = dto.IsActive,
                Description = dto.Description?.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.ReunionEvents.Add(ev);
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ReunionRegistrationDto?> GetRegistrationAsync(int eventId, string serviceNo, CancellationToken ct = default)
    {
        var reg = await _db.Reunions
            .Include(r => r.Event)
            .Include(r => r.PersonalData)
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.ServiceNo == serviceNo, ct);

        if (reg == null) return null;

        var pmt = await _db.Payments
            .Where(p => p.EventId == eventId && p.ServiceNo == serviceNo)
            .OrderByDescending(p => p.PaymentId)
            .FirstOrDefaultAsync(ct);

        return new ReunionRegistrationDto
        {
            ReunionId = reg.ReunionId,
            EventId = reg.EventId,
            EventTitle = reg.Event?.Title ?? "Reunion",
            ServiceNo = reg.ServiceNo,
            MemberName = reg.PersonalData?.Name ?? string.Empty,
            NoOfPersons = reg.NoOfPersons,
            AccompDetails = reg.AccompDetails,
            DateOfArrival = reg.DateOfArrival,
            PlaceOfArrival = reg.PlaceOfArrival,
            Accommodation = reg.Accommodation,
            DateOfDeparture = reg.DateOfDeparture,
            PlaceOfDeparture = reg.PlaceOfDeparture,
            TShirt = reg.TShirt,
            Drinker = reg.Drinker,
            VegNonVeg = reg.VegNonVeg,
            RegistrationStatus = reg.RegistrationStatus,
            CreatedAtUtc = reg.CreatedAtUtc,
            UpdatedAtUtc = reg.UpdatedAtUtc,
            PaymentStatus = pmt?.PaymentStatus.ToString() ?? "Unpaid",
            AmountPaid = pmt?.AmountPaid ?? 0
        };
    }

    public async Task<(bool Success, string Message)> RegisterOrUpdateAsync(SubmitReunionRegistrationDto dto, CancellationToken ct = default)
    {
        var ev = await _db.ReunionEvents.FindAsync([dto.EventId], ct);
        if (ev == null)
        {
            return (false, "The selected reunion event was not found.");
        }

        if (DateTime.UtcNow.Date > ev.RegistrationDeadline.Date)
        {
            return (false, "The registration deadline for this event has passed.");
        }

        var reg = await _db.Reunions
            .FirstOrDefaultAsync(r => r.EventId == dto.EventId && r.ServiceNo == dto.ServiceNo, ct);

        if (reg == null)
        {
            reg = new IafReunion
            {
                EventId = dto.EventId,
                ServiceNo = dto.ServiceNo,
                NoOfPersons = Math.Max(1, dto.NoOfPersons),
                AccompDetails = dto.AccompDetails?.Trim(),
                DateOfArrival = dto.DateOfArrival,
                PlaceOfArrival = dto.PlaceOfArrival,
                Accommodation = dto.Accommodation?.Trim(),
                DateOfDeparture = dto.DateOfDeparture,
                PlaceOfDeparture = dto.PlaceOfDeparture?.Trim(),
                TShirt = dto.TShirt,
                Drinker = dto.Drinker,
                VegNonVeg = dto.VegNonVeg,
                RegistrationStatus = RegistrationStatus.Submitted,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Reunions.Add(reg);
        }
        else
        {
            reg.NoOfPersons = Math.Max(1, dto.NoOfPersons);
            reg.AccompDetails = dto.AccompDetails?.Trim();
            reg.DateOfArrival = dto.DateOfArrival;
            reg.PlaceOfArrival = dto.PlaceOfArrival;
            reg.Accommodation = dto.Accommodation?.Trim();
            reg.DateOfDeparture = dto.DateOfDeparture;
            reg.PlaceOfDeparture = dto.PlaceOfDeparture?.Trim();
            reg.TShirt = dto.TShirt;
            reg.Drinker = dto.Drinker;
            reg.VegNonVeg = dto.VegNonVeg;
            reg.RegistrationStatus = RegistrationStatus.Submitted;
            reg.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return (true, "Reunion preferences saved successfully.");
    }

    public async Task<List<ReunionRegistrationDto>> GetAllRegistrationsAsync(int eventId, CancellationToken ct = default)
    {
        var list = await _db.Reunions
            .Include(r => r.Event)
            .Include(r => r.PersonalData)
            .Where(r => r.EventId == eventId)
            .OrderBy(r => r.PersonalData!.Name)
            .ToListAsync(ct);

        var payments = await _db.Payments
            .Where(p => p.EventId == eventId)
            .ToListAsync(ct);

        var paymentMap = payments
            .GroupBy(p => p.ServiceNo)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.PaymentId).First());

        return list.Select(reg =>
        {
            paymentMap.TryGetValue(reg.ServiceNo, out var pmt);
            return new ReunionRegistrationDto
            {
                ReunionId = reg.ReunionId,
                EventId = reg.EventId,
                EventTitle = reg.Event?.Title ?? "Reunion",
                ServiceNo = reg.ServiceNo,
                MemberName = reg.PersonalData?.Name ?? string.Empty,
                NoOfPersons = reg.NoOfPersons,
                AccompDetails = reg.AccompDetails,
                DateOfArrival = reg.DateOfArrival,
                PlaceOfArrival = reg.PlaceOfArrival,
                Accommodation = reg.Accommodation,
                DateOfDeparture = reg.DateOfDeparture,
                PlaceOfDeparture = reg.PlaceOfDeparture,
                TShirt = reg.TShirt,
                Drinker = reg.Drinker,
                VegNonVeg = reg.VegNonVeg,
                RegistrationStatus = reg.RegistrationStatus,
                CreatedAtUtc = reg.CreatedAtUtc,
                UpdatedAtUtc = reg.UpdatedAtUtc,
                PaymentStatus = pmt?.PaymentStatus.ToString() ?? "Unpaid",
                AmountPaid = pmt?.AmountPaid ?? 0
            };
        }).ToList();
    }

    public async Task<bool> UpdateRegistrationStatusAsync(int reunionId, RegistrationStatus status, CancellationToken ct = default)
    {
        var reg = await _db.Reunions.FindAsync([reunionId], ct);
        if (reg == null) return false;

        reg.RegistrationStatus = status;
        reg.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
