# End-to-End Business Workflows

This document describes the core operational workflows of the application, complete with step-by-step logic and sequence diagrams.

---

## 1. Member Spreadsheet Import Workflow

Administrators can bulk-import historical airmen records from Excel (`.xlsx`) or CSV files.

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Reunion Admin
    participant Ctrl as MemberRegistryController
    participant Svc as ImportService (ClosedXML / CsvHelper)
    participant DB as SQL Server (AppDbContext)

    Admin->>Ctrl: Upload .xlsx or .csv file
    Ctrl->>Svc: ParseStreamAsync(fileStream, fileExtension)
    Note over Svc: Read rows into List<ImportMemberDto><br/>Validate required fields (ServiceNo, Name)
    Svc->>DB: Check existing ServiceNo in IAF_PersonalData
    alt Duplicates Found & Overwrite Disabled
        Svc-->>Ctrl: Flag duplicate rows as Skipped/Errors
    end
    Svc->>DB: Begin Transaction
    Svc->>DB: Bulk Insert valid records into IAF_PersonalData,<br/>IAF_FamilyData, IAF_AdditionalData
    DB-->>Svc: Commit Transaction
    Svc-->>Ctrl: Return ImportResultDto (Success, Skipped, Errors)
    Ctrl-->>Admin: Display summary & link to download error Excel
```

### Import Validation Rules:
1. **Service Number (`ServiceNo`)**: Mandatory, trimmed, uppercase. Must be unique.
2. **Member Name**: Mandatory, max 150 characters.
3. **Trade & State Codes**: Validated against `app.Trade` and `app.State` tables; if not found, stored as null or logged as non-fatal warning.
4. **Error Log Workbook**: If invalid rows exist, an in-memory ClosedXML workbook is generated and cached in `IMemoryCache` for 15 minutes, allowing immediate admin download.

---

## 2. Member Account Provisioning & Activation

To ensure security without emailing predictable default passwords, accounts use tokenized self-activation.

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Reunion Admin
    actor Member as IAF Veteran
    participant MACtrl as MemberAccountController
    participant AccCtrl as AccountController
    participant IdMgr as UserManager<ApplicationUser>
    participant DB as AppDbContext & Identity

    Admin->>MACtrl: Click "Provision Accounts" (Single or Bulk)
    MACtrl->>DB: Query members without linked AspNetUsers
    loop For each unlinked member
        MACtrl->>MACtrl: Generate 256-bit cryptographically secure Token
        MACtrl->>IdMgr: Create ApplicationUser(ServiceNo, Email, Token, IsActive=False)
        MACtrl->>IdMgr: AddToRoleAsync(user, "Member")
    end
    MACtrl-->>Admin: Display generated Activation Links (can be shared via Email/WhatsApp)
    
    Member->>AccCtrl: Open /Account/Activate?serviceNo=...&token=...
    AccCtrl->>DB: Verify token matches unactivated user
    AccCtrl-->>Member: Display "Set Your Password" screen
    Member->>AccCtrl: Submit password (min 8 chars, mixed case, digit, symbol)
    AccCtrl->>IdMgr: ResetPassword / SetPasswordAsync
    AccCtrl->>DB: Mark user IsActive=True, clear activation token
    AccCtrl-->>Member: Redirect to Login with success message
```

---

## 3. Reunion Registration & Preference Selection

Once authenticated, members register their attendance and logistics preferences.

```mermaid
sequenceDiagram
    autonumber
    actor Member as IAF Veteran
    participant RCtrl as ReunionController
    participant RSvc as ReunionService
    participant DB as AppDbContext

    Member->>RCtrl: GET /Reunion/Register
    RCtrl->>RSvc: GetActiveEventAsync() & GetRegistrationAsync(serviceNo)
    RSvc-->>RCtrl: Return event details + existing registration (if any)
    RCtrl-->>Member: Render Registration Form
    Member->>RCtrl: POST /Reunion/Register (Attendees, Transit, Mess, T-Shirt, Food)
    RCtrl->>RSvc: SaveRegistrationAsync(serviceNo, registrationDto)
    RSvc->>DB: Upsert app.IAF_Reunion
    DB-->>RSvc: Saved
    RSvc-->>RCtrl: Success
    RCtrl-->>Member: Redirect to /Payment/Submit with payment prompt
```

---

## 4. Payment Submission & Administrative Verification

Fee tracking accommodates diverse Indian payment modes (UPI, NEFT, Cheque, Cash) and flags duplicate transaction references.

```mermaid
sequenceDiagram
    autonumber
    actor Member as IAF Veteran
    actor Admin as Reunion Admin
    participant PCtrl as PaymentController
    participant PSvc as PaymentService
    participant DB as AppDbContext

    Member->>PCtrl: POST /Payment/Submit (Amount, Mode, UTR/RefNo, Receipt File)
    PCtrl->>PSvc: Validate file (size < 5MB, JPG/PNG/PDF)
    PSvc->>DB: Query if ReferenceNumber or AcknowledgmentNumber exists
    alt Reference Already Claimed
        PSvc->>DB: Insert Payment with DuplicateAckWarning = true, Status = Pending
    else Unique Reference
        PSvc->>DB: Insert Payment with Status = Pending
    end
    DB-->>PCtrl: Payment Recorded
    PCtrl-->>Member: Show "Payment Under Verification"

    Admin->>PCtrl: GET /Payment/VerificationQueue
    PCtrl->>PSvc: GetPendingPaymentsAsync()
    PSvc-->>Admin: Render list with [Duplicate Warning] badge if flagged
    Admin->>PCtrl: GET /Payment/ReceiptProof/{id} (Inspect receipt)
    alt Approved
        Admin->>PCtrl: POST /Payment/Verify { paymentId }
        PCtrl->>PSvc: Mark Verified, set VerifiedBy and VerifiedAt
    else Rejected
        Admin->>PCtrl: POST /Payment/Reject { paymentId, rejectionReason }
        PCtrl->>PSvc: Mark Rejected with recorded reason
    end
    PCtrl-->>Admin: Refresh queue
```

---

## 5. Report Generation & Streaming

Administrators can download any of the 14 reports in either `.xlsx` or `.csv` format:

1. **Request**: Admin navigates to `/Report` and clicks an export link (e.g., `/Report/ExportArrivalSchedule?format=xlsx`).
2. **Data Assembly**: `ReportService` queries `AppDbContext` using `.AsNoTracking()` and projects required fields.
3. **Stream Building**:
   - For **XLSX**: A `XLWorkbook` is created, styled with blue headers, data rows added, worksheet name sanitized to **≤ 31 characters**, and saved into a `MemoryStream`.
   - For **CSV**: A `StreamWriter` with `CultureInfo.InvariantCulture` writes header and row values via `CsvHelper`.
4. **HTTP Delivery**: The controller returns `File(stream.ToArray(), contentType, fileName)`.
