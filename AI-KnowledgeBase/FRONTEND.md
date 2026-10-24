# Frontend and User Interface Guidelines

This document details the Razor View structure, UI conventions, Bootstrap 5 styling, LibMan assets, and dynamic Content Security Policy (CSP) integration.

---

## 1. UI Philosophy & Theme

The user interface is designed specifically for **Indian Air Force Batch 1986 Veterans**:
- **Design Language**: Air Force Military Theme (deep navy blues `#0B2545`, air force gold accents `#E0A96D`, clean whites, and slate grays).
- **Usability**: High contrast, large legible typography, clear call-to-action buttons, intuitive mobile cards for smartphones, and responsive desktop tables.
- **Tone**: Respectful, ceremonial, and organized.

---

## 2. Directory Layout & Razor Views

Views are located under `src/Solution.Web/Views/`:

```
Views/
├── Account/                (Login, Activate, ForgotPassword, ResetPassword, AccessDenied)
├── Dashboard/              (Admin Dashboard, Member Dashboard)
├── Home/                   (Landing Page, Privacy, Error)
├── MemberAccount/          (Provisioning overview, Activation link generation)
├── MemberRegistry/         (Directory, Member Details, Create/Edit Form, Import Spreadsheet)
├── Payment/                (Veteran Payment Submission, Verification Queue)
├── Report/                 (Report Center: 14 Download Tiles with XLSX/CSV buttons)
├── Reunion/                (Event Itinerary, Veteran Attendance Registration)
├── Shared/
│   ├── _Layout.cshtml      (Master layout with navbar, alerts, footer, and script nonces)
│   ├── _LoginPartial.cshtml(Authentication header with Member/Admin badge & avatar)
│   └── _ValidationScriptsPartial.cshtml (jQuery validate & unobtrusive scripts)
├── _ViewImports.cshtml     (Global tag helpers and namespace imports)
└── _ViewStart.cshtml       (Sets _Layout.cshtml as default)
```

---

## 3. Dynamic CSP Nonce Integration

The application enforces a strict Content Security Policy (CSP) where inline scripts and styles must provide a cryptographically generated nonce.

### Master Layout: `_Layout.cshtml`
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - IAF Batch 1986 Reunion</title>
    
    <!-- Bootstrap 5 CSS -->
    <link rel="stylesheet" href="~/lib/bootstrap/css/bootstrap.min.css" />
    <!-- Bootstrap Icons -->
    <link rel="stylesheet" href="~/lib/bootstrap-icons/font/bootstrap-icons.min.css" />
    <!-- Site CSS -->
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    ...
    <!-- Scripts -->
    <script src="~/lib/jquery/jquery.min.js"></script>
    <script src="~/lib/bootstrap/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Implementing Inline Scripts in Views:
Any view that contains `<script>` or `<style>` **MUST** include the `nonce="@ViewBag.CspNonce"` attribute:

```html
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script nonce="@ViewBag.CspNonce">
        $(function () {
            $('#toggleSpouseDetails').on('change', function () {
                $('#spouseSection').toggle(this.checked);
            });
        });
    </script>
}
```

---

## 4. Client-Side Libraries & LibMan

Frontend dependencies are tracked and restored via [libman.json](file:///e:/ProjectTemplate/src/Solution.Web/libman.json):

```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": [
    {
      "library": "bootstrap@5.3.3",
      "destination": "wwwroot/lib/bootstrap/"
    },
    {
      "library": "bootstrap-icons@1.11.3",
      "destination": "wwwroot/lib/bootstrap-icons/"
    },
    {
      "library": "jquery@3.7.1",
      "destination": "wwwroot/lib/jquery/"
    },
    {
      "library": "jquery-validate@1.21.0",
      "destination": "wwwroot/lib/jquery-validate/"
    },
    {
      "library": "jquery-validation-unobtrusive@4.0.0",
      "destination": "wwwroot/lib/jquery-validation-unobtrusive/"
    }
  ]
}
```

---

## 5. UI Components & Patterns

1. **Photo Carousel / Viewer**:
   - Member profile displays up to 5 historical photographs using a Bootstrap 5 carousel or thumbnail gallery, streaming images directly from `/MemberRegistry/Photo/{serviceNo}/{index}`.
2. **Duplicate Payment Warning Badge**:
   - In `/Payment/VerificationQueue`, rows where `DuplicateAckWarning == true` display a highlighted amber badge `<span class="badge bg-warning text-dark"><i class="bi bi-exclamation-triangle-fill"></i> Duplicate Ref</span>`.
3. **Data Export Tiles**:
   - The `/Report/Index` dashboard presents 14 distinct cards, each with two prominent download buttons: `<a class="btn btn-outline-success">Excel (.xlsx)</a>` and `<a class="btn btn-outline-primary">CSV (.csv)</a>`.
4. **Responsive Tables**:
   - All grids are wrapped in `<div class="table-responsive">` to ensure smooth horizontal scrolling on mobile displays.
