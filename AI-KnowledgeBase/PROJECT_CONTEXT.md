# Project Context: IAF Batch 1986 Reunion Management & Member Registry

## 1. Executive Summary

The **IAF Batch 1986 Reunion Management and Member Registry** is a secure, responsive, enterprise-grade web application built on **.NET 10** following **Clean Architecture** principles.

The system empowers Indian Air Force (IAF) 1986 intake veterans and reunion organizers to:
1. Maintain an authoritative historical registry of Batch 1986 airmen (Service Number, trades, units, family, and photos).
2. Bulk-import and validate historical records from Excel and CSV spreadsheets.
3. Automatically provision individual member accounts with secure, tokenized self-activation.
4. Manage multi-event reunion registrations (attendance, arrival/departure transit nodes, accommodation, dietary and apparel preferences).
5. Track reunion fee collections with proof slip uploads (images/PDFs), duplicate transaction detection, and administrative verification/rejection.
6. Monitor operational readiness via live interactive dashboards (Organizer and Veteran views).
7. Generate 14 distinct operational and financial reports in `.xlsx` and `.csv` formats.

---

## 2. Core Architecture Overview

The solution adheres strictly to **Clean Architecture** separation of concerns:

```
Solution.Domain           <-- Core Entities, Enums, Zero External Dependencies
       ▲
       │ references
Solution.Application      <-- Service Interfaces, DTOs, ViewModels, Business Logic Contracts
       ▲
       │ references
Solution.Infrastructure   <-- EF Core 10 (AppDbContext, AppIdentityDbContext), Services, ClosedXML, CsvHelper
       ▲
       │ references
Solution.Web              <-- ASP.NET Core 10 MVC, Controllers, Razor Views, Security Middleware, Identity
```

Additionally, `tests/Solution.Tests` validates domain logic, data integrity, import parsing, registration rules, payment verification, and report generation using xUnit and EF Core InMemory provider.

---

## 3. High-Level Module Summary

### Module 1: Member Data Import and Registry
- **Service Number as Unique Key**: Every member is uniquely identified by their IAF `ServiceNo` (string, max 50 chars).
- **Spreadsheet Parsing**: ClosedXML (Excel `.xlsx`) and CsvHelper (CSV) stream records into `ImportMemberDto`.
- **Pre-Import Preview & Validation**: Verifies mandatory fields, formats, and flags existing/duplicate `ServiceNo` records before committing.
- **Transactional Ingestion**: Uses EF Core transaction blocks to atomically save `IAF_PersonalData`, `IAF_FamilyData`, and `IAF_AdditionalData`.
- **Exportable Error Reports**: Generates an Excel error workbook detailing row-by-row validation failures.

### Module 2: Member User Account Management
- **Identity Integration**: ASP.NET Core Identity with custom `ApplicationUser` linked 1-to-1 to `ServiceNo`.
- **Secure Activation**: Generates cryptographically secure 256-bit activation tokens (`AccountActivationToken`). Members set their own password upon verification. No predictable default passwords.
- **Self-Service Profile**: Veterans can view their military profile, update emergency contacts and family records, and manage up to 5 historical photographs.
- **Access Control**: Role-based authorization (`Admin` and `Member`). Sensitive registry details are protected: members can only view their own profile, while Admins possess full oversight.

### Module 3: Reunion Event & Attendance Management
- **Event Lifecycle**: Supports multi-day, multi-event itineraries (`IAF_ReunionEvent`) with active dates, venue locations, and base registration fees.
- **Attendance Registration**: Captures arrival/departure transit points (Airport, Central Railway Station, Egmore, Tambaram), Mess accommodation requirements, T-shirt sizes (M, L, XL, XXL), dietary choices (Veg/Non-Veg), and beverage preferences.
- **Family Accompanying Count**: Tracks spouse and child attendee counts for seating and catering estimates.

### Module 4: Fee Collection & Payment Verification
- **Payment Submission**: Veterans log payment mode (UPI, Bank Transfer, Cheque, Cash), transaction reference / UTR number, date, amount, and upload digital receipt proof.
- **Duplicate Acknowledgment Warning**: Instant visual badge and server-side flag warning administrators if a transaction reference or acknowledgment number has already been claimed.
- **Admin Verification Workflow**: Organizers review receipt attachments inline and mark payments as `Verified` or `Rejected` with recorded feedback reasons.

### Module 5: Dashboards & 14 Reports
- **Executive Dashboard**: Total intake headcount, account activation rates, reunion attendees (members + guests), confirmed payments vs. pending dues, and dietary distributions.
- **Veteran Hub**: Personal registration status, event schedule, verification state, and announcements.
- **14 Formats in Excel & CSV**:
  1. Full Member Directory
  2. Registered Members List
  3. Non-Registered Members List
  4. Account Activation Status Report
  5. Reunion Attendance Master List
  6. Arrival & Reception Schedule (Transit Node Sort)
  7. Departure & Transit Schedule
  8. Mess Accommodation Allotment List
  9. T-Shirt Size Distribution Summary
  10. Dietary & Catering Summary (Veg / Non-Veg / Drinker)
  11. Verified Payments & Collections Ledger
  12. Pending & Unverified Payments Report
  13. Rejected Payments Log
  14. Comprehensive Financial Reconciliation Report

---

## 4. Critical Business Rules

1. **Service Number Uniqueness**: `ServiceNo` is the primary key across `IAF_PersonalData`, `IAF_FamilyData`, and `IAF_AdditionalData`. It cannot be altered once created.
2. **Account Linking**: An `ApplicationUser` must strictly link to an existing `IAF_PersonalData` record via `ApplicationUser.ServiceNo`.
3. **Data Integrity Overwrites**: Importing existing members updates their record only if explicitly configured; otherwise, duplicates are skipped and logged in error reports.
4. **Receipt Uploads**: Payment proofs are validated for size (< 5MB) and mime types (`image/jpeg`, `image/png`, `application/pdf`).
5. **Report Sheet Constraints**: ClosedXML worksheet names are capped at 31 characters to avoid OpenXML corruption exceptions.
6. **Strict Strictness**: `Directory.Build.props` enforces `TreatWarningsAsErrors=true` across all projects. Any code added must have zero compiler warnings.

---

## 5. Quick Links to Detailed Documentation

- Architectural structure and pipeline: [ARCHITECTURE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/ARCHITECTURE.md)
- Database schema and tables: [DATABASE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/DATABASE.md)
- Domain entities and DTOs: [ENTITIES_AND_MODELS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/ENTITIES_AND_MODELS.md)
- Business flows and diagrams: [BUSINESS_FLOWS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/BUSINESS_FLOWS.md)
- Security and CSP configuration: [SECURITY_AND_PERFORMANCE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/SECURITY_AND_PERFORMANCE.md)
- AI Agent implementation rules: [AGENT_RULES.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/AGENT_RULES.md)
