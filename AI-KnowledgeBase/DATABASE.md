# Database Architecture and Schema Specification

This document details the database schemas, tables, relationships, constraints, indexes, and initialization seeding logic implemented in the application.

---

## 1. Schema Overview

The database uses two dedicated schemas to partition tables logically:
- **`app` Schema**: Contains all Indian Air Force 1986 intake domain tables (members, families, photos, events, registrations, fees, trades, and states).
- **`auth` Schema**: Contains all ASP.NET Core Identity authentication and authorization tables.

---

## 2. Entity-Relationship (ER) Diagram

```mermaid
erDiagram
    Trade ||--o{ IAF_PersonalData : "trade_ref"
    State ||--o{ IAF_PersonalData : "state_ref"
    IAF_PersonalData ||--|| IAF_FamilyData : "has"
    IAF_PersonalData ||--|| IAF_AdditionalData : "has"
    IAF_PersonalData ||--o{ IAF_Reunion : "registers"
    IAF_PersonalData ||--o{ IAF_Payment : "pays"
    IAF_ReunionEvent ||--o{ IAF_Reunion : "contains"
    IAF_Reunion ||--o{ IAF_Payment : "funded_by"

    IAF_PersonalData {
        string ServiceNo PK
        string MemberName
        string EmailId
        string MobileNo
        string Rank
        string TradeCode FK
        string StateCode FK
        string PresentAddress
        string PermanentAddress
    }

    IAF_FamilyData {
        string ServiceNo PK,FK
        string MaritalStatus
        string SpouseName
        int ChildrenCount
        string ChildrenDetails
    }

    IAF_AdditionalData {
        string ServiceNo PK,FK
        string TechnicalSkills
        string PlacesWorked
        string Achievements
        string VrsReason
        byte[] Photo1
        byte[] Photo2
        byte[] Photo3
        byte[] Photo4
        byte[] Photo5
    }

    IAF_ReunionEvent {
        int EventId PK
        string Title
        string Description
        datetime StartDate
        datetime EndDate
        string Venue
        decimal RegistrationFee
        bool IsActive
    }

    IAF_Reunion {
        int ReunionId PK
        int EventId FK
        string ServiceNo FK
        bool Attending
        int TotalAttendees
        string ArrivalTransitPoint
        string DepartureTransitPoint
        bool RequiresMessAccommodation
        string TShirtSize
        string DietaryPreference
        string BeveragePreference
        datetime RegisteredAt
    }

    IAF_Payment {
        int PaymentId PK
        string ServiceNo FK
        int ReunionId FK
        decimal Amount
        string PaymentMode
        string ReferenceNumber
        string AcknowledgmentNumber
        datetime PaymentDate
        string Status
        byte[] ReceiptProof
        string ReceiptProofContentType
        datetime? VerifiedAt
        string VerifiedBy
        string RejectionReason
    }
```

---

## 3. Data Dictionary: `app` Schema Tables

### Table: `app.IAF_PersonalData`
Primary entity storing personal and military service records of the airman.
- **`ServiceNo`** (VARCHAR(50), PK, NOT NULL): The airman's unique IAF Service Number. Natural key.
- **`MemberName`** (NVARCHAR(150), NOT NULL): Full legal name of the member.
- **`EmailId`** (NVARCHAR(150), NULL): Email address used for communications and account invitations.
- **`MobileNo`** (VARCHAR(20), NULL): Primary contact telephone number.
- **`Rank`** (NVARCHAR(50), NULL): Final military rank held (e.g., Sergeant, JWO, WO, MWO).
- **`TradeCode`** (VARCHAR(20), FK, NULL): Foreign key referencing `app.Trade.TradeCode`.
- **`StateCode`** (VARCHAR(10), FK, NULL): Foreign key referencing `app.State.StateCode`.
- **`PresentAddress`** (NVARCHAR(500), NULL): Current residential address.
- **`PermanentAddress`** (NVARCHAR(500), NULL): Permanent native address.
- **`DateOfBirth`** (DATETIME2, NULL): Birth date.
- **`EnrolmentDate`** (DATETIME2, NULL): Intake enrollment date (1986).
- **`DischargeDate`** (DATETIME2, NULL): Date of retirement or discharge.
- **`CreatedAt`** (DATETIME2, NOT NULL): Timestamp of record creation.
- **`UpdatedAt`** (DATETIME2, NULL): Timestamp of last modification.

### Table: `app.IAF_FamilyData`
Maintains family and dependents associated with an airman.
- **`ServiceNo`** (VARCHAR(50), PK, FK, NOT NULL): 1-to-1 foreign key to `app.IAF_PersonalData.ServiceNo`.
- **`MaritalStatus`** (NVARCHAR(30), NULL): Married, Single, Widowed.
- **`SpouseName`** (NVARCHAR(150), NULL): Name of spouse.
- **`SpouseDateOfBirth`** (DATETIME2, NULL): Spouse birth date.
- **`ChildrenCount`** (INT, NOT NULL, DEFAULT 0): Total number of children.
- **`ChildrenDetails`** (NVARCHAR(1000), NULL): Serialized summary of children's names, ages, and professions.

### Table: `app.IAF_AdditionalData`
Maintains military accomplishments, post-retirement career, and 5 binary photo slots.
- **`ServiceNo`** (VARCHAR(50), PK, FK, NOT NULL): 1-to-1 foreign key to `app.IAF_PersonalData.ServiceNo`.
- **`TechnicalSkills`** (NVARCHAR(1000), NULL): Specializations and certifications.
- **`PlacesWorked`** (NVARCHAR(1000), NULL): Historical postings and stations (e.g., Tambaram, Halwara, Pune).
- **`Achievements`** (NVARCHAR(1000), NULL): Commendations, medals, or distinctions.
- **`VrsReason`** (NVARCHAR(500), NULL): Reason for voluntary retirement or transition notes.
- **`Photo1`** .. **`Photo5`** (VARBINARY(MAX), NULL): Raw binary storage for up to 5 member photographs.
- **`Photo1ContentType`** .. **`Photo5ContentType`** (VARCHAR(50), NULL): MIME types (`image/jpeg`, `image/png`).

### Table: `app.IAF_ReunionEvent`
Manages reunion iterations and details.
- **`EventId`** (INT, PK, IDENTITY, NOT NULL): Unique auto-incrementing ID.
- **`Title`** (NVARCHAR(200), NOT NULL): Reunion event title (e.g., "Batch 1986 40th Anniversary Grand Reunion").
- **`Description`** (NVARCHAR(MAX), NULL): Detailed itinerary, instructions, and schedule.
- **`StartDate`** (DATETIME2, NOT NULL): Event commencement date.
- **`EndDate`** (DATETIME2, NOT NULL): Event conclusion date.
- **`Venue`** (NVARCHAR(300), NOT NULL): Physical venue location (e.g., "Air Force Station Tambaram Officers Mess").
- **`RegistrationFee`** (DECIMAL(18,2), NOT NULL, DEFAULT 0): Base fee per registering veteran.
- **`IsActive`** (BIT, NOT NULL, DEFAULT 1): Flag indicating if registration is open.

### Table: `app.IAF_Reunion`
Stores veteran event registration choices and preferences.
- **`ReunionId`** (INT, PK, IDENTITY, NOT NULL): Unique auto-incrementing registration ID.
- **`EventId`** (INT, FK, NOT NULL): Foreign key referencing `app.IAF_ReunionEvent.EventId`.
- **`ServiceNo`** (VARCHAR(50), FK, NOT NULL): Foreign key referencing `app.IAF_PersonalData.ServiceNo`.
- **`Attending`** (BIT, NOT NULL, DEFAULT 1): Confirmation of attendance.
- **`TotalAttendees`** (INT, NOT NULL, DEFAULT 1): Total headcount (Veteran + Spouse + Children).
- **`ArrivalTransitPoint`** (NVARCHAR(50), NULL): Airport, Chennai Central, Egmore, Tambaram, or Self.
- **`DepartureTransitPoint`** (NVARCHAR(50), NULL): Transit node for departure drop-off.
- **`RequiresMessAccommodation`** (BIT, NOT NULL, DEFAULT 0): Air Force Mess accommodation request.
- **`TShirtSize`** (VARCHAR(10), NULL): Size selection (`M`, `L`, `XL`, `XXL`, `XXXL`).
- **`DietaryPreference`** (VARCHAR(20), NOT NULL): `Vegetarian` or `NonVegetarian`.
- **`BeveragePreference`** (VARCHAR(20), NOT NULL): `NonDrinker` or `Drinker`.
- **`RegisteredAt`** (DATETIME2, NOT NULL): Timestamp of submission.

### Table: `app.IAF_Payment`
Stores fee transactions, payment proofs, and verification audits.
- **`PaymentId`** (INT, PK, IDENTITY, NOT NULL): Unique auto-incrementing ID.
- **`ServiceNo`** (VARCHAR(50), FK, NOT NULL): Paying member's Service Number.
- **`ReunionId`** (INT, FK, NULL): Linked reunion event registration.
- **`Amount`** (DECIMAL(18,2), NOT NULL): Payment amount in INR (₹).
- **`PaymentMode`** (VARCHAR(30), NOT NULL): `UPI`, `BankTransfer`, `Cheque`, `Cash`.
- **`ReferenceNumber`** (NVARCHAR(100), NOT NULL): UTR, Transaction ID, or Cheque Number.
- **`AcknowledgmentNumber`** (NVARCHAR(100), NULL): Internal or banking receipt acknowledgement.
- **`PaymentDate`** (DATETIME2, NOT NULL): Date transaction occurred.
- **`Status`** (VARCHAR(30), NOT NULL): `Pending`, `Verified`, `Rejected`.
- **`ReceiptProof`** (VARBINARY(MAX), NULL): Digital receipt image or PDF file.
- **`ReceiptProofContentType`** (VARCHAR(50), NULL): MIME type (`image/jpeg`, `image/png`, `application/pdf`).
- **`VerifiedAt`** (DATETIME2, NULL): Timestamp when verified by admin.
- **`VerifiedBy`** (NVARCHAR(100), NULL): Identity of reviewing administrator.
- **`RejectionReason`** (NVARCHAR(500), NULL): Recorded reason if rejected.

### Table: `app.Trade` & `app.State`
Reference master tables:
- **`app.Trade`**: `TradeCode` (PK, VARCHAR(20)), `TradeName` (NVARCHAR(100)). Pre-seeded with 12 IAF trades (Radar Fitter, Engine Fitter, Airframe Fitter, Electrical Fitter, Radio Fitter, etc.).
- **`app.State`**: `StateCode` (PK, VARCHAR(10)), `StateName` (NVARCHAR(100)). Pre-seeded with all 36 Indian States and Union Territories.

---

## 4. `auth` Schema (ASP.NET Core Identity)

- **`auth.AspNetUsers`**: Linked to `ServiceNo` via custom column `ServiceNo` (VARCHAR(50)).
- **`auth.AspNetRoles`**: Contains standard roles: `Admin` and `Member`.
- **`auth.AspNetUserRoles`**: Cross-reference mapping users to roles.
- Standard supporting tables: `auth.AspNetUserClaims`, `auth.AspNetUserLogins`, `auth.AspNetUserTokens`, `auth.AspNetRoleClaims`.

---

## 5. Indexes and Key Constraints

1. **Unique Primary Keys**:
   - `IAF_PersonalData`: Clustered PK on `ServiceNo`.
   - `IAF_FamilyData`: Clustered PK on `ServiceNo`.
   - `IAF_AdditionalData`: Clustered PK on `ServiceNo`.
2. **Composite / Unique Constraints**:
   - `IAF_Reunion`: Unique constraint on `(EventId, ServiceNo)` preventing double-registration for the same event.
3. **Foreign Keys**:
   - `IAF_FamilyData(ServiceNo) -> IAF_PersonalData(ServiceNo)` (Cascade Delete).
   - `IAF_AdditionalData(ServiceNo) -> IAF_PersonalData(ServiceNo)` (Cascade Delete).
   - `IAF_Reunion(ServiceNo) -> IAF_PersonalData(ServiceNo)`.
   - `IAF_Payment(ServiceNo) -> IAF_PersonalData(ServiceNo)`.
4. **Performance Indexes**:
   - Index on `IAF_Payment(ReferenceNumber)` and `IAF_Payment(AcknowledgmentNumber)` to expedite duplicate payment detection.
   - Index on `IAF_PersonalData(MemberName, MobileNo, EmailId)` for directory filtering.

---

## 6. Seed Data & Initialization Logic

Automatic database initialization is managed by:
- **`DbInitializer.cs`**:
  - Seeds 12 IAF trades if `app.Trade` is empty.
  - Seeds 36 Indian states/UTs if `app.State` is empty.
  - Seeds the active 40th Anniversary Reunion Event if `app.IAF_ReunionEvent` has no records.
  - Seeds initial sample member records if the registry is empty.
- **`IdentityDataSeeder.cs`**:
  - Creates the `Admin` and `Member` roles if missing.
  - Provisions the default administrator user (`admin@iaf1986.local` or configurable in `appsettings.json`) and assigns the `Admin` role.
