namespace Solution.Application.DTOs;

public class AdminDashboardStatsDto
{
    // Member statistics
    public int TotalMembersInRegistry { get; set; }
    public int TotalAccountsCreated { get; set; }
    public int ActiveAccountsCount { get; set; }
    public int InactiveAccountsCount { get; set; }
    public int UnactivatedAccountsCount { get; set; }
    public int IncompleteProfilesCount { get; set; }
    public Dictionary<string, int> StateWiseMemberDistribution { get; set; } = new();
    public Dictionary<string, int> TradeWiseMemberDistribution { get; set; } = new();

    // Reunion registration statistics
    public int TotalRegisteredForReunion { get; set; }
    public int ConfirmedRegistrationsCount { get; set; }
    public int PendingRegistrationsCount { get; set; }
    public int CancelledRegistrationsCount { get; set; }
    public int UnregisteredMembersCount { get; set; }
    public int TotalExpectedAttendees { get; set; }
    public int TotalAccompanyingPersons { get; set; }
    public int AccommodationRequiredCount { get; set; }
    public Dictionary<string, int> TShirtSizeDistribution { get; set; } = new();
    public int VegetarianMealsCount { get; set; }
    public int NonVegetarianMealsCount { get; set; }
    public int DrinkerCount { get; set; }
    public int NonDrinkerCount { get; set; }
    public Dictionary<string, int> ArrivalPlaceDistribution { get; set; } = new();

    // Payment statistics
    public int MembersReportedPaid { get; set; }
    public int PaymentsVerifiedCount { get; set; }
    public int PaymentsPendingVerificationCount { get; set; }
    public int PaymentsRejectedCount { get; set; }
    public int UnpaidMembersCount { get; set; }
    public decimal TotalVerifiedAmountCollected { get; set; }
    public decimal TotalReportedAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public Dictionary<string, int> PaymentModeDistribution { get; set; } = new();
}

public class MemberDashboardStatsDto
{
    public MemberSummaryDto MemberSummary { get; set; } = new();
    public ReunionEventDto? ActiveEvent { get; set; }
    public ReunionRegistrationDto? Registration { get; set; }
    public PaymentDto? LatestPayment { get; set; }
    public List<IafAnnouncementDto> Announcements { get; set; } = new();
}

public class IafAnnouncementDto
{
    public int AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAtUtc { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}
