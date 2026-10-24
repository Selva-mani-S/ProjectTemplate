# Domain Entities, DTOs, ViewModels, and Enums

This document catalogs all core domain entities, data transfer objects (DTOs), ViewModels, and enumerations implemented in `Solution.Domain` and `Solution.Application`.

---

## 1. Domain Entities (`Solution.Domain.Entities`)

All entities are pure C# classes without ORM or MVC attributes. Entity configurations are fluently mapped in `AppDbContext.OnModelCreating`.

### Member Aggregate
- **`IafPersonalData`**:
  ```csharp
  public class IafPersonalData
  {
      public string ServiceNo { get; set; } = string.Empty;
      public string MemberName { get; set; } = string.Empty;
      public string? EmailId { get; set; }
      public string? MobileNo { get; set; }
      public string? Rank { get; set; }
      public string? TradeCode { get; set; }
      public string? StateCode { get; set; }
      public string? PresentAddress { get; set; }
      public string? PermanentAddress { get; set; }
      public DateTime? DateOfBirth { get; set; }
      public DateTime? EnrolmentDate { get; set; }
      public DateTime? DischargeDate { get; set; }
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      
      // Navigations
      public Trade? Trade { get; set; }
      public State? State { get; set; }
      public IafFamilyData? FamilyData { get; set; }
      public IafAdditionalData? AdditionalData { get; set; }
      public ICollection<IafReunion> ReunionRegistrations { get; set; } = new List<IafReunion>();
      public ICollection<IafPayment> Payments { get; set; } = new List<IafPayment>();
  }
  ```

- **`IafFamilyData`**:
  ```csharp
  public class IafFamilyData
  {
      public string ServiceNo { get; set; } = string.Empty;
      public string? MaritalStatus { get; set; }
      public string? SpouseName { get; set; }
      public DateTime? SpouseDateOfBirth { get; set; }
      public int ChildrenCount { get; set; }
      public string? ChildrenDetails { get; set; }
      public IafPersonalData PersonalData { get; set; } = null!;
  }
  ```

- **`IafAdditionalData`**:
  ```csharp
  public class IafAdditionalData
  {
      public string ServiceNo { get; set; } = string.Empty;
      public string? TechnicalSkills { get; set; }
      public string? PlacesWorked { get; set; }
      public string? Achievements { get; set; }
      public string? VrsReason { get; set; }
      public byte[]? Photo1 { get; set; }
      public string? Photo1ContentType { get; set; }
      public byte[]? Photo2 { get; set; }
      public string? Photo2ContentType { get; set; }
      public byte[]? Photo3 { get; set; }
      public string? Photo3ContentType { get; set; }
      public byte[]? Photo4 { get; set; }
      public string? Photo4ContentType { get; set; }
      public byte[]? Photo5 { get; set; }
      public string? Photo5ContentType { get; set; }
      public IafPersonalData PersonalData { get; set; } = null!;
  }
  ```

### Reunion & Financial Entities
- **`IafReunionEvent`**: Event metadata (`EventId`, `Title`, `Description`, `StartDate`, `EndDate`, `Venue`, `RegistrationFee`, `IsActive`).
- **`IafReunion`**: Registration record (`ReunionId`, `EventId`, `ServiceNo`, `Attending`, `TotalAttendees`, `ArrivalTransitPoint`, `DepartureTransitPoint`, `RequiresMessAccommodation`, `TShirtSize`, `DietaryPreference`, `BeveragePreference`, `RegisteredAt`).
- **`IafPayment`**: Payment record (`PaymentId`, `ServiceNo`, `ReunionId`, `Amount`, `PaymentMode`, `ReferenceNumber`, `AcknowledgmentNumber`, `PaymentDate`, `Status`, `ReceiptProof`, `ReceiptProofContentType`, `VerifiedAt`, `VerifiedBy`, `RejectionReason`).
- **`IafAnnouncement`**: Bulletin message (`AnnouncementId`, `Title`, `Content`, `PublishedAt`, `IsPinned`, `Author`).

### Master Tables
- **`Trade`**: `TradeCode` (string), `TradeName` (string).
- **`State`**: `StateCode` (string), `StateName` (string).

---

## 2. Enumerations (`Solution.Domain.Enums`)

### `PaymentMode`
```csharp
public enum PaymentMode
{
    UPI = 1,
    BankTransfer = 2,
    Cheque = 3,
    Cash = 4
}
```

### `PaymentStatus`
```csharp
public enum PaymentStatus
{
    Pending = 1,
    Verified = 2,
    Rejected = 3
}
```

### `TransitPoint`
```csharp
public enum TransitPoint
{
    Airport = 1,
    ChennaiCentral = 2,
    Egmore = 3,
    Tambaram = 4,
    SelfArranged = 5
}
```

### `TShirtSize`
```csharp
public enum TShirtSize
{
    M = 1,
    L = 2,
    XL = 3,
    XXL = 4,
    XXXL = 5
}
```

### `DietaryPreference` & `BeveragePreference`
```csharp
public enum DietaryPreference
{
    Vegetarian = 1,
    NonVegetarian = 2
}

public enum BeveragePreference
{
    NonDrinker = 1,
    Drinker = 2
}
```

---

## 3. Data Transfer Objects (`Solution.Application.DTOs`)

### Member Registry DTOs
- **`MemberDto`**: Complete member profile with personal, family, military, and photo counts.
- **`MemberSummaryDto`**: Lightweight record for paginated grids and search listings (ServiceNo, Name, Rank, Trade, Mobile, State, HasAccount, HasRegistered, HasPaid).
- **`MemberFilterRequest`**: Search filters (`SearchTerm`, `TradeCode`, `StateCode`, `HasAccountOnly`, `Page`, `PageSize`).
- **`MemberCreateEditDto`**: Input model for creating or editing members via web forms.

### Import DTOs
- **`ImportMemberDto`**: Row-level mapping from Excel or CSV files.
- **`ImportResultDto`**: High-level result containing `TotalRecords`, `SuccessfulRecords`, `SkippedRecords`, `Errors`, and downloadable error Excel token.
- **`ImportRowError`**: Granular error item (`RowNumber`, `ServiceNo`, `MemberName`, `ErrorMessage`).

### Reunion DTOs
- **`ReunionEventDto`**: Read-only display of reunion event details and fee schedule.
- **`ReunionRegistrationDto`**: Form model for submitting attendance preferences and transit details.
- **`ReunionSummaryDto`**: Summary of an individual member's registration status.

### Payment DTOs
- **`PaymentSubmissionDto`**: Form model for veterans submitting fee payment proof.
- **`PaymentDto`**: Display DTO showing payment status, mode, UTR, date, verification status, and duplicate flag.
- **`PaymentVerificationDto`**: Admin input for approving or rejecting a payment with feedback reason.

### Dashboard DTOs
- **`AdminDashboardDto`**: Aggregated KPIs (Total Members, Active Accounts, Total Attendees, Total Confirmed Fees, Total Pending Dues, Transit Breakdown, T-Shirt Counts).
- **`MemberDashboardDto`**: Individual veteran view (Registration status, payment status, active announcements, profile completeness).
- **`DietaryStatsDto`**: Catering aggregates (Veg vs Non-Veg, Drinker vs Non-Drinker).
