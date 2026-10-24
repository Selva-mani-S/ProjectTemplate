namespace Solution.Application.DTOs;

public class ImportRowDto
{
    public int RowIndex { get; set; }
    public string ServiceNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? Dob { get; set; }
    public DateTime? Dod { get; set; }
    public string? TradeId { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? StateId { get; set; }
    public string? MobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? TechnicalSkills { get; set; }
    public string? PlacesWorked { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? Achievements { get; set; }
    public string? VrsReason { get; set; }

    public bool IsValid { get; set; } = true;
    public bool IsDuplicate { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

public class ImportPreviewDto
{
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int ValidCount { get; set; }
    public int InvalidCount { get; set; }
    public int DuplicateCount { get; set; }
    public List<ImportRowDto> Rows { get; set; } = new();
    public List<string> FileErrors { get; set; } = new();
}

public class ImportResultDto
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessfullyImported { get; set; }
    public int SkippedDuplicates { get; set; }
    public int FailedCount { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}
