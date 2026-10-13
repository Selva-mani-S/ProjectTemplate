namespace Solution.Infrastructure.Logging;

using Microsoft.Extensions.Logging;
using Solution.Application.Common.Interfaces;
using Solution.Domain.Entities;
using Solution.Infrastructure.Persistence;

public static partial class LogExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Security Event: {EventType} | User: {UserId} ({Username}) | Success: {IsSuccess} | IP: {IpAddress}")]
    public static partial void LogSecurityEvent(
        this ILogger logger,
        string eventType,
        string userId,
        string username,
        bool isSuccess,
        string ipAddress);
}

public class AuditLogger : IAuditLogger
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(AppDbContext dbContext, ILogger<AuditLogger> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogSecurityEventAsync(
        string eventType,
        string userId,
        string username,
        string ipAddress,
        string userAgent,
        string actionDetails,
        bool isSuccess,
        CancellationToken cancellationToken = default)
    {
        // OWASP A09: Ensure zero credentials or PII leaks in log messages
        var auditEntry = new ApplicationAuditLog
        {
            EventType = eventType,
            UserId = userId ?? "Anonymous",
            Username = username ?? "Anonymous",
            IpAddress = ipAddress ?? "Unknown",
            UserAgent = userAgent ?? "Unknown",
            ActionDetails = actionDetails,
            IsSuccess = isSuccess,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditEntry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogSecurityEvent(
            eventType,
            auditEntry.UserId,
            auditEntry.Username,
            isSuccess,
            auditEntry.IpAddress);
    }
}
