namespace Solution.Application.Common.Interfaces;

public interface IAuditLogger
{
    Task LogSecurityEventAsync(
        string eventType,
        string userId,
        string username,
        string ipAddress,
        string userAgent,
        string actionDetails,
        bool isSuccess,
        CancellationToken cancellationToken = default);
}
