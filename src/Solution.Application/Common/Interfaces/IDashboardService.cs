namespace Solution.Application.Common.Interfaces;

using Solution.Application.DTOs;

public interface IDashboardService
{
    Task<AdminDashboardStatsDto> GetAdminStatsAsync(int eventId, CancellationToken ct = default);
    Task<MemberDashboardStatsDto> GetMemberStatsAsync(string serviceNo, CancellationToken ct = default);
}
