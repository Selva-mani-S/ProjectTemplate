namespace Solution.Infrastructure.Services;

using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Infrastructure.Persistence;

public class ImportService : IImportService
{
    private readonly AppDbContext _db;

    public ImportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ImportPreviewDto> ParseAndValidateFileAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var preview = new ImportPreviewDto { FileName = fileName };
        var rawRows = new List<ImportRowDto>();

        try
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                rawRows = ParseExcel(fileStream);
            }
            else if (string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                rawRows = ParseCsv(fileStream);
            }
            else
            {
                preview.FileErrors.Add("Unsupported file format. Please upload an Excel (.xlsx) or CSV (.csv) file.");
                return preview;
            }
        }
        catch (Exception ex)
        {
            preview.FileErrors.Add($"Failed to read file: {ex.Message}");
            return preview;
        }

        if (rawRows.Count == 0)
        {
            preview.FileErrors.Add("The uploaded file does not contain any data rows.");
            return preview;
        }

        // Existing ServiceNos in DB
        var existingServiceNos = await _db.PersonalData
            .Select(p => p.ServiceNo)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

        var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rawRows)
        {
            // Validate ServiceNo
            if (string.IsNullOrWhiteSpace(row.ServiceNo))
            {
                row.IsValid = false;
                row.ValidationErrors.Add("Service Number is required.");
            }
            else
            {
                row.ServiceNo = row.ServiceNo.Trim();
                if (row.ServiceNo.Length > 6)
                {
                    row.IsValid = false;
                    row.ValidationErrors.Add("Service Number must be 6 characters or fewer.");
                }

                if (existingServiceNos.Contains(row.ServiceNo))
                {
                    row.IsDuplicate = true;
                    row.ValidationErrors.Add($"Service Number '{row.ServiceNo}' already exists in the database.");
                }
                else if (seenInFile.Contains(row.ServiceNo))
                {
                    row.IsDuplicate = true;
                    row.ValidationErrors.Add($"Duplicate Service Number '{row.ServiceNo}' found in file.");
                }
                else
                {
                    seenInFile.Add(row.ServiceNo);
                }
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(row.Name))
            {
                row.IsValid = false;
                row.ValidationErrors.Add("Name is required.");
            }
            else
            {
                row.Name = row.Name.Trim();
                if (row.Name.Length > 100)
                {
                    row.IsValid = false;
                    row.ValidationErrors.Add("Name exceeds 100 characters.");
                }
            }

            // Validate Mobile
            if (!string.IsNullOrWhiteSpace(row.MobileNo) && row.MobileNo.Trim().Length > 13)
            {
                row.IsValid = false;
                row.ValidationErrors.Add("Mobile number exceeds 13 characters.");
            }

            if (row.ValidationErrors.Count > 0)
            {
                row.IsValid = false;
            }

            preview.Rows.Add(row);
        }

        preview.TotalRecords = preview.Rows.Count;
        preview.ValidCount = preview.Rows.Count(r => r.IsValid && !r.IsDuplicate);
        preview.DuplicateCount = preview.Rows.Count(r => r.IsDuplicate);
        preview.InvalidCount = preview.Rows.Count(r => !r.IsValid && !r.IsDuplicate);

        return preview;
    }

    public async Task<ImportResultDto> CommitImportAsync(List<ImportRowDto> validRows, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(validRows);

        var result = new ImportResultDto
        {
            TotalProcessed = validRows.Count
        };

        var filteredRows = validRows.Where(r => r.IsValid && !r.IsDuplicate).ToList();
        if (filteredRows.Count == 0)
        {
            result.Success = false;
            result.ErrorMessages.Add("No valid records available to import.");
            return result;
        }

        var existingServiceNos = await _db.PersonalData
            .Select(p => p.ServiceNo)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

        var entitiesToAdd = new List<IafPersonalData>();

        foreach (var r in filteredRows)
        {
            if (existingServiceNos.Contains(r.ServiceNo))
            {
                result.SkippedDuplicates++;
                continue;
            }

            entitiesToAdd.Add(new IafPersonalData
            {
                ServiceNo = r.ServiceNo.Trim(),
                Name = r.Name.Trim(),
                Dob = r.Dob,
                Dod = r.Dod,
                TradeId = string.IsNullOrWhiteSpace(r.TradeId) ? null : r.TradeId.Trim().ToUpperInvariant(),
                Address1 = string.IsNullOrWhiteSpace(r.Address1) ? null : r.Address1.Trim(),
                Address2 = string.IsNullOrWhiteSpace(r.Address2) ? null : r.Address2.Trim(),
                StateId = string.IsNullOrWhiteSpace(r.StateId) ? null : r.StateId.Trim().ToUpperInvariant(),
                MobileNo = string.IsNullOrWhiteSpace(r.MobileNo) ? null : r.MobileNo.Trim(),
                EmailId = string.IsNullOrWhiteSpace(r.EmailId) ? null : r.EmailId.Trim(),
                TechnicalSkills = string.IsNullOrWhiteSpace(r.TechnicalSkills) ? null : r.TechnicalSkills.Trim(),
                PlacesWorked = string.IsNullOrWhiteSpace(r.PlacesWorked) ? null : r.PlacesWorked.Trim(),
                AdditionalDetails = string.IsNullOrWhiteSpace(r.AdditionalDetails) ? null : r.AdditionalDetails.Trim(),
                Achievements = string.IsNullOrWhiteSpace(r.Achievements) ? null : r.Achievements.Trim(),
                VrsReason = string.IsNullOrWhiteSpace(r.VrsReason) ? null : r.VrsReason.Trim()
            });
        }

        try
        {
            await _db.PersonalData.AddRangeAsync(entitiesToAdd, ct);
            await _db.SaveChangesAsync(ct);

            result.Success = true;
            result.SuccessfullyImported = entitiesToAdd.Count;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.FailedCount = entitiesToAdd.Count;
            result.ErrorMessages.Add($"Database save error: {ex.Message}");
        }

        return result;
    }

    public Task<byte[]> GenerateErrorReportExcelAsync(List<ImportRowDto> invalidRows, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(invalidRows);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Import Errors");

        // Headers
        worksheet.Cell(1, 1).Value = "Row Index";
        worksheet.Cell(1, 2).Value = "Service Number";
        worksheet.Cell(1, 3).Value = "Name";
        worksheet.Cell(1, 4).Value = "Mobile";
        worksheet.Cell(1, 5).Value = "Validation Errors";

        worksheet.Row(1).Style.Font.Bold = true;

        int rowNum = 2;
        foreach (var row in invalidRows)
        {
            worksheet.Cell(rowNum, 1).Value = row.RowIndex;
            worksheet.Cell(rowNum, 2).Value = row.ServiceNo;
            worksheet.Cell(rowNum, 3).Value = row.Name;
            worksheet.Cell(rowNum, 4).Value = row.MobileNo;
            worksheet.Cell(rowNum, 5).Value = string.Join("; ", row.ValidationErrors);
            rowNum++;
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return Task.FromResult(memoryStream.ToArray());
    }

    private static List<ImportRowDto> ParseExcel(Stream stream)
    {
        var rows = new List<ImportRowDto>();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return rows;

        var headerRow = worksheet.Row(1);
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            string colName = cell.GetString().Trim().Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();
            columnMap[colName] = cell.Address.ColumnNumber;
        }

        int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = 2; r <= lastRow; r++)
        {
            var row = worksheet.Row(r);
            if (row.IsEmpty()) continue;

            string GetCol(string name)
            {
                string key = name.Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();
                if (columnMap.TryGetValue(key, out int colIdx))
                {
                    return row.Cell(colIdx).GetString().Trim();
                }
                return string.Empty;
            }

            DateTime? ParseDate(string val)
            {
                if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
                if (DateTime.TryParse(val, out var dtLocal)) return dtLocal;
                return null;
            }

            rows.Add(new ImportRowDto
            {
                RowIndex = r,
                ServiceNo = GetCol("ServiceNo"),
                Name = GetCol("Name"),
                Dob = ParseDate(GetCol("DOB")),
                Dod = ParseDate(GetCol("DOD")),
                TradeId = GetCol("TradeID"),
                Address1 = GetCol("Address1"),
                Address2 = GetCol("Address2"),
                StateId = GetCol("StateID"),
                MobileNo = GetCol("MobileNo"),
                EmailId = GetCol("EmailId") ?? GetCol("Email"),
                TechnicalSkills = GetCol("TechnicalSkills"),
                PlacesWorked = GetCol("PlacesWorked"),
                AdditionalDetails = GetCol("AdditionalDetails"),
                Achievements = GetCol("Achievements"),
                VrsReason = GetCol("VRSReason")
            });
        }

        return rows;
    }

    private static List<ImportRowDto> ParseCsv(Stream stream)
    {
        var rows = new List<ImportRowDto>();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim().Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).ToLowerInvariant()
        });

        if (!csv.Read() || !csv.ReadHeader()) return rows;

        int rowIndex = 2;
        while (csv.Read())
        {
            string GetField(string name)
            {
                string key = name.Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();
                return csv.TryGetField<string>(key, out var val) && val != null ? val.Trim() : string.Empty;
            }

            DateTime? ParseDate(string val)
            {
                if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
                if (DateTime.TryParse(val, out var dtLocal)) return dtLocal;
                return null;
            }

            rows.Add(new ImportRowDto
            {
                RowIndex = rowIndex++,
                ServiceNo = GetField("ServiceNo"),
                Name = GetField("Name"),
                Dob = ParseDate(GetField("DOB")),
                Dod = ParseDate(GetField("DOD")),
                TradeId = GetField("TradeID"),
                Address1 = GetField("Address1"),
                Address2 = GetField("Address2"),
                StateId = GetField("StateID"),
                MobileNo = GetField("MobileNo"),
                EmailId = GetField("EmailId") ?? GetField("Email"),
                TechnicalSkills = GetField("TechnicalSkills"),
                PlacesWorked = GetField("PlacesWorked"),
                AdditionalDetails = GetField("AdditionalDetails"),
                Achievements = GetField("Achievements"),
                VrsReason = GetField("VRSReason")
            });
        }

        return rows;
    }
}
