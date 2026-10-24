namespace Solution.Domain.Entities;

public class IafFamilyData
{
    public int FamilyMemberId { get; set; }
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Remarks { get; set; }

    public IafPersonalData? PersonalData { get; set; }
}
