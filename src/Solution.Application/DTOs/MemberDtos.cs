namespace Solution.Application.DTOs;

public class MemberSummaryDto
{
    public string ServiceNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? StateId { get; set; }
    public string? StateName { get; set; }
    public string? MobileNo { get; set; }
    public string? EmailId { get; set; }
    public bool HasUserAccount { get; set; }
    public bool IsAccountActive { get; set; }
    public bool HasRegisteredForReunion { get; set; }
    public string RegistrationStatus { get; set; } = "Not Registered";
    public string PaymentStatus { get; set; } = "Unpaid";
}

public class MemberDetailsDto
{
    public string ServiceNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? Dob { get; set; }
    public DateTime? Dod { get; set; }
    public string? TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? StateId { get; set; }
    public string? StateName { get; set; }
    public string? MobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? TechnicalSkills { get; set; }
    public string? PlacesWorked { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? Achievements { get; set; }
    public string? VrsReason { get; set; }

    public bool HasAccount { get; set; }
    public bool IsAccountActive { get; set; }
    public string? AccountEmail { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }

    public List<FamilyMemberDto> FamilyMembers { get; set; } = new();
    public List<byte[]> Photos { get; set; } = new();

    public ReunionRegistrationDto? CurrentRegistration { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
}

public class FamilyMemberDto
{
    public int FamilyMemberId { get; set; }
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Remarks { get; set; }
}

public class CreateMemberDto
{
    public string ServiceNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? Dob { get; set; }
    public DateTime? Dod { get; set; }
    public string? TradeId { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? StateId { get; set; }
    public string? MobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? TechnicalSkills { get; set; }
    public string? PlacesWorked { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? Achievements { get; set; }
    public string? VrsReason { get; set; }
}
