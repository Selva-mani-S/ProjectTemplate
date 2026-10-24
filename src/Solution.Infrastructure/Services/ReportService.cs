namespace Solution.Infrastructure.Services;

using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Domain.Enums;
using Solution.Infrastructure.Identity;
using Solution.Infrastructure.Persistence;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public string GetReportName(int reportNumber)
    {
        return reportNumber switch
        {
            1 => "Complete_Member_Registry",
            2 => "Members_Without_Activated_Accounts",
            3 => "Members_Who_Have_Not_Registered",
            4 => "Reunion_Registration_Summary",
            5 => "Expected_Attendance_Accompanying_Persons",
            6 => "Arrival_Departure_Schedule",
            7 => "Accommodation_Requirements",
            8 => "TShirt_Size_Distribution",
            9 => "Food_And_Drink_Preference_Summary",
            10 => "Payment_Status_Summary",
            11 => "Verified_Payment_Collection_Report",
            12 => "Outstanding_Fee_Report",
            13 => "Payment_Mode_Summary",
            14 => "Pending_And_Rejected_Payments_Report",
            _ => "IAF_Report"
        };
    }

    public async Task<byte[]> GenerateReportExcelAsync(int reportNumber, int eventId, CancellationToken ct = default)
    {
        var (headers, rows) = await GetReportDataAsync(reportNumber, eventId, ct);

        using var workbook = new XLWorkbook();
        string reportName = GetReportName(reportNumber);
        string sheetName = reportName.Length > 31 ? reportName[..31] : reportName;
        var worksheet = workbook.Worksheets.Add(sheetName);

        for (int col = 0; col < headers.Count; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(13, 110, 253);
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < rows[r].Count; c++)
            {
                worksheet.Cell(r + 2, c + 1).Value = rows[r][c]?.ToString() ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateReportCsvAsync(int reportNumber, int eventId, CancellationToken ct = default)
    {
        var (headers, rows) = await GetReportDataAsync(reportNumber, eventId, ct);

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            foreach (var h in headers)
            {
                csv.WriteField(h);
            }
            csv.NextRecord();

            foreach (var row in rows)
            {
                foreach (var cell in row)
                {
                    csv.WriteField(cell);
                }
                csv.NextRecord();
            }
        }

        return memoryStream.ToArray();
    }

    private async Task<(List<string> Headers, List<List<object?>> Rows)> GetReportDataAsync(int reportNumber, int eventId, CancellationToken ct)
    {
        var headers = new List<string>();
        var rows = new List<List<object?>>();

        var eventObj = await _db.ReunionEvents.FindAsync([eventId], ct);
        decimal baseFee = eventObj?.BaseFeeAmount ?? 5000.00m;

        switch (reportNumber)
        {
            case 1: // Complete Member Registry
                headers.AddRange(["Service No", "Name", "Trade", "State", "Mobile No", "Technical Skills", "Places Worked", "VRS Reason"]);
                var members = await _db.PersonalData
                    .Include(p => p.Trade)
                    .Include(p => p.State)
                    .OrderBy(p => p.ServiceNo)
                    .ToListAsync(ct);
                foreach (var m in members)
                {
                    rows.Add([m.ServiceNo, m.Name, m.Trade?.TradeName, m.State?.StateName, m.MobileNo, m.TechnicalSkills, m.PlacesWorked, m.VrsReason]);
                }
                break;

            case 2: // Members Without Activated Accounts
                headers.AddRange(["Service No", "Name", "Trade", "Mobile No", "Account Created", "Activation Status"]);
                var allMembers = await _db.PersonalData.Include(p => p.Trade).OrderBy(p => p.ServiceNo).ToListAsync(ct);
                var users = await _userManager.Users.ToListAsync(ct);
                var userMap = users
                    .Where(u => u.ServiceNo != null)
                    .ToDictionary(u => u.ServiceNo!, u => u, StringComparer.OrdinalIgnoreCase);

                foreach (var m in allMembers)
                {
                    bool hasAcc = userMap.TryGetValue(m.ServiceNo, out var u);
                    bool isActivated = hasAcc && u!.EmailConfirmed;
                    if (!isActivated)
                    {
                        rows.Add([m.ServiceNo, m.Name, m.Trade?.TradeName, m.MobileNo, hasAcc ? "Yes" : "No", isActivated ? "Activated" : "Not Activated"]);
                    }
                }
                break;

            case 3: // Members Who Have Not Registered
                headers.AddRange(["Service No", "Name", "Trade", "State", "Mobile No"]);
                var registeredServiceNos = await _db.Reunions
                    .Where(r => r.EventId == eventId)
                    .Select(r => r.ServiceNo)
                    .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);
                var unregistered = await _db.PersonalData
                    .Include(p => p.Trade)
                    .Include(p => p.State)
                    .Where(p => !registeredServiceNos.Contains(p.ServiceNo))
                    .OrderBy(p => p.ServiceNo)
                    .ToListAsync(ct);
                foreach (var m in unregistered)
                {
                    rows.Add([m.ServiceNo, m.Name, m.Trade?.TradeName, m.State?.StateName, m.MobileNo]);
                }
                break;

            case 4: // Reunion Registration Summary
                headers.AddRange(["Service No", "Member Name", "Total Attendees", "Arrival Date", "Arrival Place", "T-Shirt Size", "Registration Status"]);
                var regs = await _db.Reunions
                    .Include(r => r.PersonalData)
                    .Where(r => r.EventId == eventId)
                    .OrderBy(r => r.ServiceNo)
                    .ToListAsync(ct);
                foreach (var r in regs)
                {
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, r.NoOfPersons, r.DateOfArrival?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), r.PlaceOfArrival, r.TShirt, r.RegistrationStatus.ToString()]);
                }
                break;

            case 5: // Expected Attendance and Accompanying Persons
                headers.AddRange(["Service No", "Member Name", "Total Attendees", "Accompanying Persons Count", "Accompanying Details"]);
                var attendance = await _db.Reunions
                    .Include(r => r.PersonalData)
                    .Where(r => r.EventId == eventId)
                    .OrderBy(r => r.ServiceNo)
                    .ToListAsync(ct);
                foreach (var r in attendance)
                {
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, r.NoOfPersons, Math.Max(0, r.NoOfPersons - 1), r.AccompDetails]);
                }
                break;

            case 6: // Arrival and Departure Schedule
                headers.AddRange(["Service No", "Member Name", "Arrival Date/Time", "Place of Arrival", "Departure Date/Time", "Place of Departure"]);
                var schedule = await _db.Reunions
                    .Include(r => r.PersonalData)
                    .Where(r => r.EventId == eventId)
                    .OrderBy(r => r.DateOfArrival)
                    .ToListAsync(ct);
                foreach (var r in schedule)
                {
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, r.DateOfArrival?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), r.PlaceOfArrival, r.DateOfDeparture?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), r.PlaceOfDeparture]);
                }
                break;

            case 7: // Accommodation Requirements
                headers.AddRange(["Service No", "Member Name", "Total Attendees", "Accommodation Requirement"]);
                var accomm = await _db.Reunions
                    .Include(r => r.PersonalData)
                    .Where(r => r.EventId == eventId && !string.IsNullOrWhiteSpace(r.Accommodation) && !string.Equals(r.Accommodation, "None", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.ServiceNo)
                    .ToListAsync(ct);
                foreach (var r in accomm)
                {
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, r.NoOfPersons, r.Accommodation]);
                }
                break;

            case 8: // T-shirt Size Distribution
                headers.AddRange(["Service No", "Member Name", "T-Shirt Size"]);
                var tshirts = await _db.Reunions
                    .Include(r => r.PersonalData)
                    .Where(r => r.EventId == eventId && !string.IsNullOrWhiteSpace(r.TShirt))
                    .OrderBy(r => r.TShirt)
                    .ThenBy(r => r.ServiceNo)
                    .ToListAsync(ct);
                foreach (var r in tshirts)
                {
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, r.TShirt]);
                }
                break;

            case 9: // Food and Drink Preference Summary
                headers.AddRange(["Service No", "Member Name", "Meal Preference", "Drinking Preference"]);
                var prefs = await _db.Reunions
                    .Include(r => r.PersonalData)
                    .Where(r => r.EventId == eventId)
                    .OrderBy(r => r.ServiceNo)
                    .ToListAsync(ct);
                foreach (var r in prefs)
                {
                    string meal = r.VegNonVeg == 0 ? "Vegetarian" : "Non-Vegetarian";
                    string drink = r.Drinker == 1 ? "Drinker" : "Non-Drinker";
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, meal, drink]);
                }
                break;

            case 10: // Payment Status Summary
                headers.AddRange(["Service No", "Member Name", "Registration Status", "Payment Status", "Amount Paid"]);
                var regList = await _db.Reunions.Include(r => r.PersonalData).Where(r => r.EventId == eventId).ToListAsync(ct);
                var pmtList = await _db.Payments.Where(p => p.EventId == eventId).ToListAsync(ct);
                var pMap = pmtList.GroupBy(p => p.ServiceNo, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.PaymentId).First(), StringComparer.OrdinalIgnoreCase);

                foreach (var r in regList)
                {
                    pMap.TryGetValue(r.ServiceNo, out var p);
                    rows.Add([r.ServiceNo, r.PersonalData?.Name, r.RegistrationStatus.ToString(), p?.PaymentStatus.ToString() ?? "Unpaid", p?.AmountPaid ?? 0]);
                }
                break;

            case 11: // Verified Payment Collection Report
                headers.AddRange(["Service No", "Member Name", "Payment Mode", "Acknowledgement No", "Payment Date", "Amount (INR)", "Verified By", "Verified At"]);
                var verifiedPmts = await _db.Payments
                    .Include(p => p.PersonalData)
                    .Where(p => p.EventId == eventId && p.PaymentStatus == PaymentStatus.Verified)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync(ct);
                foreach (var p in verifiedPmts)
                {
                    rows.Add([p.ServiceNo, p.PersonalData?.Name, p.PaymentMode.ToString(), p.AcknowledgementNumber, p.PaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), p.AmountPaid, p.VerifiedBy, p.VerifiedAtUtc?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)]);
                }
                break;

            case 12: // Outstanding Fee Report
                headers.AddRange(["Service No", "Member Name", "Attendees", "Expected Fee (INR)", "Verified Paid (INR)", "Outstanding Balance (INR)"]);
                var registered = await _db.Reunions.Include(r => r.PersonalData).Where(r => r.EventId == eventId).ToListAsync(ct);
                var verifiedList = await _db.Payments
                    .Where(p => p.EventId == eventId && p.PaymentStatus == PaymentStatus.Verified)
                    .ToListAsync(ct);
                var verifiedByServiceNo = verifiedList
                    .GroupBy(p => p.ServiceNo, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.AmountPaid), StringComparer.OrdinalIgnoreCase);

                foreach (var r in registered)
                {
                    decimal paid = verifiedByServiceNo.TryGetValue(r.ServiceNo, out var val) ? val : 0;
                    decimal expected = baseFee; // Base fee per member
                    decimal balance = Math.Max(0, expected - paid);
                    if (balance > 0)
                    {
                        rows.Add([r.ServiceNo, r.PersonalData?.Name, r.NoOfPersons, expected, paid, balance]);
                    }
                }
                break;

            case 13: // Payment Mode Summary
                headers.AddRange(["Payment Mode", "Verified Transactions Count", "Total Amount Collected (INR)"]);
                var modeGroup = await _db.Payments
                    .Where(p => p.EventId == eventId && p.PaymentStatus == PaymentStatus.Verified)
                    .GroupBy(p => p.PaymentMode)
                    .Select(g => new { Mode = g.Key, Count = g.Count(), Total = g.Sum(x => x.AmountPaid) })
                    .ToListAsync(ct);
                foreach (var g in modeGroup)
                {
                    rows.Add([g.Mode.ToString(), g.Count, g.Total]);
                }
                break;

            case 14: // Pending and Rejected Payment Verification Report
                headers.AddRange(["Service No", "Member Name", "Payment Mode", "Acknowledgement No", "Payment Date", "Amount (INR)", "Status", "Rejection Reason"]);
                var pendingOrRejected = await _db.Payments
                    .Include(p => p.PersonalData)
                    .Where(p => p.EventId == eventId && (p.PaymentStatus == PaymentStatus.PendingVerification || p.PaymentStatus == PaymentStatus.Rejected))
                    .OrderByDescending(p => p.SubmittedAtUtc)
                    .ToListAsync(ct);
                foreach (var p in pendingOrRejected)
                {
                    rows.Add([p.ServiceNo, p.PersonalData?.Name, p.PaymentMode.ToString(), p.AcknowledgementNumber, p.PaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), p.AmountPaid, p.PaymentStatus.ToString(), p.RejectionReason]);
                }
                break;
        }

        return (headers, rows);
    }
}
