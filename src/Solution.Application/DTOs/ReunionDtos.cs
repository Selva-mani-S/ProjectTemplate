namespace Solution.Application.DTOs;

using Solution.Domain.Enums;

public class ReunionEventDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public DateTime RegistrationDeadline { get; set; }
    public decimal BaseFeeAmount { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public class ReunionRegistrationDto
{
    public int ReunionId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
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
    public RegistrationStatus RegistrationStatus { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Linked payment status
    public string PaymentStatus { get; set; } = "Unpaid";
    public decimal AmountPaid { get; set; }
}

public class SubmitReunionRegistrationDto
{
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
}
