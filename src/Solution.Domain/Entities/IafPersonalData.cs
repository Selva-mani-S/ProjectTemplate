namespace Solution.Domain.Entities;

public class IafPersonalData
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

    public Trade? Trade { get; set; }
    public State? State { get; set; }
    public IafAdditionalData? AdditionalData { get; set; }

    public ICollection<IafFamilyData> FamilyMembers { get; set; } = new List<IafFamilyData>();
    public ICollection<IafReunion> Reunions { get; set; } = new List<IafReunion>();
    public ICollection<IafPayment> Payments { get; set; } = new List<IafPayment>();
}
