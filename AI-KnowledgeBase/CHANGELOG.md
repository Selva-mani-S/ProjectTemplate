# Architecture and Engineering Changelog

This document tracks significant architectural decisions, module implementations, bug fixes, and engineering milestones for the project.

---

## [2026-10-24] - AI Knowledge Base Creation & Architectural Consolidation
- **Task**: Comprehensive codebase analysis and creation of `AI-KnowledgeBase/`.
- **Artifacts Created**:
  - `README.md`: Master index and agent reading order.
  - `PROJECT_CONTEXT.md`: High-level system overview and business rules.
  - `AGENT_RULES.md`: Mandatory instructions and anti-patterns for AI agents.
  - `TECH_STACK.md`: Exact versions for .NET 10, C# 14, EF Core 10.0.12, ClosedXML 0.105.1, CsvHelper 33.1.0, Bootstrap 5.3.3.
  - `PROJECT_STRUCTURE.md`: Solution hierarchy and inward-pointing dependency graph.
  - `ARCHITECTURE.md`: Clean Architecture layer definitions and dynamic CSP middleware lifecycle.
  - `DATABASE.md`: Dual schema (`app` and `auth`) data dictionary, ER diagram, and constraints.
  - `ENTITIES_AND_MODELS.md`: Domain models, DTOs, ViewModels, and Enums catalog.
  - `API_REFERENCE.md`: Complete MVC controller actions and routing reference.
  - `BUSINESS_FLOWS.md`: Sequence diagrams for Member Import, Account Activation, Registration, and Payment Verification.
  - `AUTHENTICATION_AND_AUTHORIZATION.md`: Identity setup, password rules, `__Host-` cookies, and RBAC.
  - `FRONTEND.md`: Razor view hierarchy, LibMan assets, and dynamic nonce injection.
  - `SERVICES_AND_INTEGRATIONS.md`: Service contracts, ClosedXML 31-character sheet limit, and CsvHelper.
  - `CONFIGURATION_AND_DEPLOYMENT.md`: Connection strings, seeding, and migration guidelines.
  - `CODING_CONVENTIONS.md`: C# 14 idiomatic rules, invariant formatting, and Roslyn warnings.
  - `SECURITY_AND_PERFORMANCE.md`: Threat mitigations, CSRF, CSP, SQL injection, and `.AsNoTracking()` queries.
  - `TESTING.md`: xUnit test suite breakdown, InMemoryDb isolation, and manual CLI test commands.

---

## [2026-10-24] - Air Force Batch 1986 Reunion Management Implementation
- **Core Modules Built**:
  1. **Member Registry & Data Import**:
     - ClosedXML and CsvHelper ingestion with duplicate `ServiceNo` detection.
     - Photo binary storage support for up to 5 member photographs.
     - Downloadable Excel error workbook for failed import rows.
  2. **Account Management & Self-Activation**:
     - Tokenized invitation workflow; members establish their own password via `/Account/Activate`.
     - Password policy set to 8 characters with upper, lower, digit, and symbol.
     - Single and bulk account provisioning from registry records.
  3. **Reunion Registration**:
     - Multi-event itinerary support.
     - Transit node tracking (Airport, Chennai Central, Egmore, Tambaram, Self).
     - Air Force Mess accommodation requests and T-shirt size selections.
     - Dietary (Veg/Non-Veg) and beverage preference tracking.
  4. **Fee Collection & Verification**:
     - Payment modes: UPI, Bank Transfer, Cheque, Cash.
     - Receipt proof upload (JPG, PNG, PDF up to 5MB).
     - Duplicate transaction reference detection with visual warning badges.
     - Admin verification and rejection with feedback reasons.
  5. **14 Operational & Financial Reports**:
     - Dual-format export (`.xlsx` via ClosedXML, `.csv` via CsvHelper) for all 14 reports.
     - Enforced 31-character limit on Excel sheet names to prevent OpenXML exceptions.
  6. **Interactive Dashboards**:
     - Organizer Dashboard: real-time headcount, financial summaries, transit logistics, catering.
     - Veteran Dashboard: registration status, payment verification, announcements.
  7. **Testing Suite**:
     - Implemented `IafCoreTests.cs` validating Member CRUD, CSV import with duplicate rejection, Reunion registration, Payment approval/rejection, all 14 Excel/CSV reports, and startup database seeding.
