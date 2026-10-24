namespace Solution.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;

public class MemberService : IMemberService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MemberService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<(List<MemberSummaryDto> Members, int TotalCount)> GetMembersAsync(
        string? search,
        string? tradeId,
        string? stateId,
        bool? unactivatedOnly,
        bool? unregisteredOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<IafPersonalData> query = _db.PersonalData
            .Include(p => p.Trade)
            .Include(p => p.State)
            .Include(p => p.Reunions)
            .Include(p => p.Payments);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim();
            query = query.Where(p =>
                p.ServiceNo.Contains(s) ||
                p.Name.Contains(s) ||
                (p.MobileNo != null && p.MobileNo.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(tradeId))
        {
            query = query.Where(p => p.TradeId == tradeId);
        }

        if (!string.IsNullOrWhiteSpace(stateId))
        {
            query = query.Where(p => p.StateId == stateId);
        }

        if (unregisteredOnly == true)
        {
            query = query.Where(p => !p.Reunions.Any());
        }

        int totalCount = await query.CountAsync(ct);

        List<IafPersonalData> rawMembers = await query
            .OrderBy(p => p.ServiceNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Fetch users linked by ServiceNo
        var serviceNos = rawMembers.Select(m => m.ServiceNo).ToList();
        var users = await _userManager.Users.ToListAsync(ct);
        var linkedUsers = users
            .Where(u => u.ServiceNo != null && serviceNos.Contains(u.ServiceNo, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(u => u.ServiceNo!, u => u, StringComparer.OrdinalIgnoreCase);

        var result = new List<MemberSummaryDto>();

        foreach (var m in rawMembers)
        {
            bool hasUser = linkedUsers.TryGetValue(m.ServiceNo, out var user);
            bool isActive = hasUser && user!.IsActive;
            bool isActivated = hasUser && user!.EmailConfirmed;

            if (unactivatedOnly == true && isActivated)
            {
                continue;
            }

            var latestReunion = m.Reunions.OrderByDescending(r => r.ReunionId).FirstOrDefault();
            var latestPayment = m.Payments.OrderByDescending(p => p.PaymentId).FirstOrDefault();

            result.Add(new MemberSummaryDto
            {
                ServiceNo = m.ServiceNo,
                Name = m.Name,
                TradeId = m.TradeId,
                TradeName = m.Trade?.TradeName,
                StateId = m.StateId,
                StateName = m.State?.StateName,
                MobileNo = m.MobileNo,
                HasUserAccount = hasUser,
                IsAccountActive = isActive,
                HasRegisteredForReunion = latestReunion != null,
                RegistrationStatus = latestReunion?.RegistrationStatus.ToString() ?? "Not Registered",
                PaymentStatus = latestPayment?.PaymentStatus.ToString() ?? "Unpaid"
            });
        }

        return (result, totalCount);
    }

    public async Task<MemberDetailsDto?> GetMemberDetailsAsync(string serviceNo, CancellationToken ct = default)
    {
        var m = await _db.PersonalData
            .Include(p => p.Trade)
            .Include(p => p.State)
            .Include(p => p.FamilyMembers)
            .Include(p => p.AdditionalData)
            .Include(p => p.Reunions).ThenInclude(r => r.Event)
            .Include(p => p.Payments).ThenInclude(p => p.Event)
            .FirstOrDefaultAsync(p => p.ServiceNo == serviceNo, ct);

        if (m == null)
        {
            return null;
        }

        var users = await _userManager.Users.ToListAsync(ct);
        var user = users.FirstOrDefault(u => string.Equals(u.ServiceNo, serviceNo, StringComparison.OrdinalIgnoreCase));

        var dto = new MemberDetailsDto
        {
            ServiceNo = m.ServiceNo,
            Name = m.Name,
            Dob = m.Dob,
            Dod = m.Dod,
            TradeId = m.TradeId,
            TradeName = m.Trade?.TradeName,
            Address1 = m.Address1,
            Address2 = m.Address2,
            StateId = m.StateId,
            StateName = m.State?.StateName,
            MobileNo = m.MobileNo,
            TechnicalSkills = m.TechnicalSkills,
            PlacesWorked = m.PlacesWorked,
            AdditionalDetails = m.AdditionalDetails,
            Achievements = m.Achievements,
            VrsReason = m.VrsReason,
            HasAccount = user != null,
            IsAccountActive = user?.IsActive ?? false,
            AccountEmail = user?.Email,
            LastLoginAtUtc = user?.LastLoginAtUtc,
            FamilyMembers = m.FamilyMembers.Select(f => new FamilyMemberDto
            {
                FamilyMemberId = f.FamilyMemberId,
                ServiceNo = f.ServiceNo,
                MemberName = f.MemberName,
                Relationship = f.Relationship,
                Gender = f.Gender,
                Occupation = f.Occupation,
                Remarks = f.Remarks
            }).ToList()
        };

        if (m.AdditionalData != null)
        {
            if (m.AdditionalData.Photo1 != null) dto.Photos.Add(m.AdditionalData.Photo1);
            if (m.AdditionalData.Photo2 != null) dto.Photos.Add(m.AdditionalData.Photo2);
            if (m.AdditionalData.Photo3 != null) dto.Photos.Add(m.AdditionalData.Photo3);
            if (m.AdditionalData.Photo4 != null) dto.Photos.Add(m.AdditionalData.Photo4);
            if (m.AdditionalData.Photo5 != null) dto.Photos.Add(m.AdditionalData.Photo5);
        }

        var reg = m.Reunions.OrderByDescending(r => r.ReunionId).FirstOrDefault();
        if (reg != null)
        {
            var pmt = m.Payments.Where(p => p.EventId == reg.EventId).OrderByDescending(p => p.PaymentId).FirstOrDefault();
            dto.CurrentRegistration = new ReunionRegistrationDto
            {
                ReunionId = reg.ReunionId,
                EventId = reg.EventId,
                EventTitle = reg.Event?.Title ?? "Reunion",
                ServiceNo = reg.ServiceNo,
                MemberName = m.Name,
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

        dto.Payments = m.Payments.OrderByDescending(p => p.PaymentId).Select(p => new PaymentDto
        {
            PaymentId = p.PaymentId,
            EventId = p.EventId,
            EventTitle = p.Event?.Title ?? "Reunion",
            ServiceNo = p.ServiceNo,
            MemberName = m.Name,
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
        }).ToList();

        return dto;
    }

    public async Task<bool> CreateMemberAsync(CreateMemberDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (await _db.PersonalData.AnyAsync(p => p.ServiceNo == dto.ServiceNo, ct))
        {
            return false;
        }

        var member = new IafPersonalData
        {
            ServiceNo = dto.ServiceNo.Trim(),
            Name = dto.Name.Trim(),
            Dob = dto.Dob,
            Dod = dto.Dod,
            TradeId = dto.TradeId,
            Address1 = dto.Address1,
            Address2 = dto.Address2,
            StateId = dto.StateId,
            MobileNo = dto.MobileNo,
            TechnicalSkills = dto.TechnicalSkills,
            PlacesWorked = dto.PlacesWorked,
            AdditionalDetails = dto.AdditionalDetails,
            Achievements = dto.Achievements,
            VrsReason = dto.VrsReason
        };

        _db.PersonalData.Add(member);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateMemberAsync(CreateMemberDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var m = await _db.PersonalData.FirstOrDefaultAsync(p => p.ServiceNo == dto.ServiceNo, ct);
        if (m == null)
        {
            return false;
        }

        m.Name = dto.Name.Trim();
        m.Dob = dto.Dob;
        m.Dod = dto.Dod;
        m.TradeId = dto.TradeId;
        m.Address1 = dto.Address1;
        m.Address2 = dto.Address2;
        m.StateId = dto.StateId;
        m.MobileNo = dto.MobileNo;
        m.TechnicalSkills = dto.TechnicalSkills;
        m.PlacesWorked = dto.PlacesWorked;
        m.AdditionalDetails = dto.AdditionalDetails;
        m.Achievements = dto.Achievements;
        m.VrsReason = dto.VrsReason;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteMemberAsync(string serviceNo, CancellationToken ct = default)
    {
        var m = await _db.PersonalData.FirstOrDefaultAsync(p => p.ServiceNo == serviceNo, ct);
        if (m == null)
        {
            return false;
        }

        _db.PersonalData.Remove(m);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AddFamilyMemberAsync(FamilyMemberDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var fam = new IafFamilyData
        {
            ServiceNo = dto.ServiceNo,
            MemberName = dto.MemberName.Trim(),
            Relationship = dto.Relationship,
            Gender = dto.Gender,
            Occupation = dto.Occupation,
            Remarks = dto.Remarks
        };

        _db.FamilyData.Add(fam);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveFamilyMemberAsync(int familyMemberId, CancellationToken ct = default)
    {
        var fam = await _db.FamilyData.FindAsync([familyMemberId], ct);
        if (fam == null)
        {
            return false;
        }

        _db.FamilyData.Remove(fam);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SavePhotoAsync(string serviceNo, int photoIndex, byte[] photoBytes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(photoBytes);

        var addData = await _db.AdditionalData.FirstOrDefaultAsync(a => a.ServiceNo == serviceNo, ct);
        if (addData == null)
        {
            addData = new IafAdditionalData { ServiceNo = serviceNo };
            _db.AdditionalData.Add(addData);
        }

        switch (photoIndex)
        {
            case 1: addData.Photo1 = photoBytes; break;
            case 2: addData.Photo2 = photoBytes; break;
            case 3: addData.Photo3 = photoBytes; break;
            case 4: addData.Photo4 = photoBytes; break;
            case 5: addData.Photo5 = photoBytes; break;
            default: return false;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<byte[]?> GetPhotoAsync(string serviceNo, int photoIndex, CancellationToken ct = default)
    {
        var addData = await _db.AdditionalData.FirstOrDefaultAsync(a => a.ServiceNo == serviceNo, ct);
        if (addData == null) return null;

        return photoIndex switch
        {
            1 => addData.Photo1,
            2 => addData.Photo2,
            3 => addData.Photo3,
            4 => addData.Photo4,
            5 => addData.Photo5,
            _ => null
        };
    }

    public async Task<List<Trade>> GetTradesAsync(CancellationToken ct = default)
    {
        return await _db.Trades.OrderBy(t => t.TradeName).ToListAsync(ct);
    }

    public async Task<List<State>> GetStatesAsync(CancellationToken ct = default)
    {
        return await _db.States.OrderBy(s => s.StateName).ToListAsync(ct);
    }
}
