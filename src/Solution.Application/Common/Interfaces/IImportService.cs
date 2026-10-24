namespace Solution.Application.Common.Interfaces;

using Solution.Application.DTOs;

public interface IImportService
{
    Task<ImportPreviewDto> ParseAndValidateFileAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<ImportResultDto> CommitImportAsync(List<ImportRowDto> validRows, CancellationToken ct = default);
    Task<byte[]> GenerateErrorReportExcelAsync(List<ImportRowDto> invalidRows, CancellationToken ct = default);
}
