# Web API and Controller Endpoint Reference

This document catalogs all MVC controllers, action methods, URL route templates, HTTP methods, authorization requirements, and return types.

---

## 1. Authentication & Security: `AccountController`

| Action & Verb | Route Pattern | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET Login` | `/Account/Login` | Anonymous | Displays the user login form with return URL support. |
| `POST Login` | `/Account/Login` | Anonymous (Anti-CSRF) | Authenticates username/email and password against ASP.NET Identity. |
| `POST Logout` | `/Account/Logout` | Authorized | Signs the user out, invalidating the auth cookie. |
| `GET AccessDenied` | `/Account/AccessDenied` | Anonymous | Renders friendly access denied page when role authorization fails. |
| `GET ForgotPassword` | `/Account/ForgotPassword` | Anonymous | Form to request password recovery. |
| `POST ForgotPassword` | `/Account/ForgotPassword` | Anonymous (Anti-CSRF) | Dispatches password reset token. |
| `GET ResetPassword` | `/Account/ResetPassword` | Anonymous | Renders password reset form with token and email parameters. |
| `POST ResetPassword` | `/Account/ResetPassword` | Anonymous (Anti-CSRF) | Sets new password if token is valid. |

---

## 2. Member Registry: `MemberRegistryController`

| Action & Verb | Route Pattern | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET Index` | `/MemberRegistry` | `Admin` | Lists all members with pagination, trade, state, and search filters. |
| `GET Details` | `/MemberRegistry/Details/{serviceNo}` | `Admin, Member` | Full member profile view. Members are restricted to viewing their own `ServiceNo`. |
| `GET Create` | `/MemberRegistry/Create` | `Admin` | Form to manually add a new airman record. |
| `POST Create` | `/MemberRegistry/Create` | `Admin` (Anti-CSRF) | Persists new member record to `app.IAF_PersonalData`. |
| `GET Edit` | `/MemberRegistry/Edit/{serviceNo}` | `Admin, Member` | Form to edit member details. Members can only edit their own contact/family data. |
| `POST Edit` | `/MemberRegistry/Edit/{serviceNo}` | `Admin, Member` (Anti-CSRF) | Saves updated member profile. |
| `GET Photo` | `/MemberRegistry/Photo/{serviceNo}/{photoIndex}` | `Admin, Member` | Streams binary image from `Photo1` .. `Photo5` with proper MIME type. |
| `GET Import` | `/MemberRegistry/Import` | `Admin` | Interface to upload Excel/CSV member data files. |
| `POST Import` | `/MemberRegistry/Import` | `Admin` (Anti-CSRF) | Parses, validates, and commits spreadsheet rows. |
| `GET DownloadImportErrors` | `/MemberRegistry/DownloadImportErrors` | `Admin` | Downloads an Excel report of rows that failed during the last import. |

---

## 3. Account Provisioning & Activation: `MemberAccountController`

| Action & Verb | Route Pattern | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET Index` | `/MemberAccount` | `Admin` | Overview of accounts: activated vs unactivated airmen. |
| `POST ProvisionSingle` | `/MemberAccount/ProvisionSingle` | `Admin` (Anti-CSRF) | Generates an activation token and creates Identity user for a single `ServiceNo`. |
| `POST ProvisionBulk` | `/MemberAccount/ProvisionBulk` | `Admin` (Anti-CSRF) | Bulk-generates activation tokens for all unprovisioned members. |
| `GET Activate` | `/Account/Activate?serviceNo=...&token=...` | Anonymous | Activation page where member establishes their initial password. |
| `POST Activate` | `/Account/Activate` | Anonymous (Anti-CSRF) | Validates 256-bit activation token, sets password, and marks account active. |

---

## 4. Reunion Events & Attendance: `ReunionController`

| Action & Verb | Route Pattern | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET Index` | `/Reunion` | `Admin, Member` | Redirects to `Events` or active event registration. |
| `GET Events` | `/Reunion/Events` | `Admin, Member` | Displays list of upcoming and past reunion events. |
| `GET Register` | `/Reunion/Register/{eventId?}` | `Admin, Member` | Form to register attendance, transit points, accommodation, and apparel size. |
| `POST Register` | `/Reunion/Register` | `Admin, Member` (Anti-CSRF) | Validates and saves event registration to `app.IAF_Reunion`. |
| `GET Registrations` | `/Reunion/Registrations/{eventId?}` | `Admin` | Master list of all registered attendees for an event. |

---

## 5. Fee Collection & Payment Verification: `PaymentController`

| Action & Verb | Route Pattern | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET Index` | `/Payment` | `Admin, Member` | Route dispatcher: directs Admin to Verification Queue and Member to Payment Status. |
| `GET Submit` | `/Payment/Submit/{eventId?}` | `Admin, Member` | Form for veterans to record payment details and upload receipt proof. |
| `POST Submit` | `/Payment/Submit` | `Admin, Member` (Anti-CSRF) | Persists payment record and receipt proof binary. |
| `GET VerificationQueue` | `/Payment/VerificationQueue` | `Admin` | Organizer queue displaying all submitted payments with duplicate reference alerts. |
| `GET ReceiptProof` | `/Payment/ReceiptProof/{paymentId}` | `Admin` | Streams uploaded receipt image or PDF file to the browser. |
| `POST Verify` | `/Payment/Verify` | `Admin` (Anti-CSRF) | Approves a payment and updates status to `Verified`. |
| `POST Reject` | `/Payment/Reject` | `Admin` (Anti-CSRF) | Rejects a payment with mandatory `rejectionReason`. |

---

## 6. Dashboards: `DashboardController`

| Action & Verb | Route Pattern | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET Index` | `/Dashboard` | `Admin, Member` | Dispatches user based on role: `/Dashboard/Admin` or `/Dashboard/Member`. |
| `GET Admin` | `/Dashboard/Admin` | `Admin` | High-level KPI dashboard: registration counts, revenue, logistics, catering. |
| `GET Member` | `/Dashboard/Member` | `Member` | Personalized veteran dashboard: event dates, pass status, payment verification. |

---

## 7. Reports & Data Export: `ReportController`

All report actions require `Admin` authorization and stream binary files (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` or `text/csv`).

| Action | Route Pattern | Output Format | Report Title |
| :--- | :--- | :--- | :--- |
| `ExportMembers` | `/Report/ExportMembers?format=xlsx\|csv` | XLSX / CSV | Full Member Directory |
| `ExportRegisteredMembers` | `/Report/ExportRegisteredMembers?format=xlsx\|csv` | XLSX / CSV | Registered Members List |
| `ExportNonRegisteredMembers` | `/Report/ExportNonRegisteredMembers?format=xlsx\|csv` | XLSX / CSV | Non-Registered Members List |
| `ExportAccountStatus` | `/Report/ExportAccountStatus?format=xlsx\|csv` | XLSX / CSV | Account Activation Status Report |
| `ExportReunionAttendees` | `/Report/ExportReunionAttendees?format=xlsx\|csv` | XLSX / CSV | Reunion Attendance Master List |
| `ExportArrivalSchedule` | `/Report/ExportArrivalSchedule?format=xlsx\|csv` | XLSX / CSV | Arrival & Reception Transit Schedule |
| `ExportDepartureSchedule` | `/Report/ExportDepartureSchedule?format=xlsx\|csv` | XLSX / CSV | Departure & Transit Schedule |
| `ExportMessAccommodation` | `/Report/ExportMessAccommodation?format=xlsx\|csv` | XLSX / CSV | Mess Accommodation Allotment List |
| `ExportTShirtSizes` | `/Report/ExportTShirtSizes?format=xlsx\|csv` | XLSX / CSV | T-Shirt Size Distribution Summary |
| `ExportDietaryPreferences` | `/Report/ExportDietaryPreferences?format=xlsx\|csv` | XLSX / CSV | Catering & Dietary Breakdown |
| `ExportVerifiedPayments` | `/Report/ExportVerifiedPayments?format=xlsx\|csv` | XLSX / CSV | Verified Payments & Collections Ledger |
| `ExportPendingPayments` | `/Report/ExportPendingPayments?format=xlsx\|csv` | XLSX / CSV | Pending Payments Report |
| `ExportRejectedPayments` | `/Report/ExportRejectedPayments?format=xlsx\|csv` | XLSX / CSV | Rejected Payments Log |
| `ExportFinancialReconciliation` | `/Report/ExportFinancialReconciliation?format=xlsx\|csv` | XLSX / CSV | Financial Reconciliation Master Report |
