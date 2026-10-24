namespace Solution.Domain.Entities;

using Solution.Domain.Enums;

public class IafReunion
{
    public int ReunionId { get; set; }
    public int EventId { get; set; }
    public string ServiceNo { get; set; } = string.Empty;
    public int NoOfPersons { get; set; } = 1;
    public string? AccompDetails { get; set; }
    public DateTime? DateOfArrival { get; set; }
    public string? PlaceOfArrival { get; set; }
    public string? Accommodation { get; set; }
    public DateTime? DateOfDeparture { get; set; }
    public string? PlaceOfDeparture { get; set; }
    public string? TShirt { get; set; }
    public int Drinker { get; set; }
    public int VegNonVeg { get; set; }
    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public IafReunionEvent? Event { get; set; }
    public IafPersonalData? PersonalData { get; set; }
}
