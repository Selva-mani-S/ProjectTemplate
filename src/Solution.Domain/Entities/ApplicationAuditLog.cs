namespace Solution.Domain.Entities;

using Solution.Domain.Common;

public class ApplicationAuditLog : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string ActionDetails { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}
