namespace Solution.Domain.Entities;

public class IafAdditionalData
{
    public string ServiceNo { get; set; } = string.Empty;
    public byte[]? Photo1 { get; set; }
    public byte[]? Photo2 { get; set; }
    public byte[]? Photo3 { get; set; }
    public byte[]? Photo4 { get; set; }
    public byte[]? Photo5 { get; set; }

    public IafPersonalData? PersonalData { get; set; }
}
