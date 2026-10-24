namespace Solution.Application.Common.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateReportExcelAsync(int reportNumber, int eventId, CancellationToken ct = default);
    Task<byte[]> GenerateReportCsvAsync(int reportNumber, int eventId, CancellationToken ct = default);
    string GetReportName(int reportNumber);
}
