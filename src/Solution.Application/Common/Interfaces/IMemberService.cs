namespace Solution.Application.Common.Interfaces;

using Solution.Application.DTOs;
using Solution.Domain.Entities;

public interface IMemberService
{
    Task<(List<MemberSummaryDto> Members, int TotalCount)> GetMembersAsync(
        string? search,
        string? tradeId,
        string? stateId,
        bool? unactivatedOnly,
        bool? unregisteredOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<MemberDetailsDto?> GetMemberDetailsAsync(string serviceNo, CancellationToken ct = default);
    Task<bool> CreateMemberAsync(CreateMemberDto dto, CancellationToken ct = default);
    Task<bool> UpdateMemberAsync(CreateMemberDto dto, CancellationToken ct = default);
    Task<bool> DeleteMemberAsync(string serviceNo, CancellationToken ct = default);
    Task<bool> AddFamilyMemberAsync(FamilyMemberDto dto, CancellationToken ct = default);
    Task<bool> RemoveFamilyMemberAsync(int familyMemberId, CancellationToken ct = default);
    Task<bool> SavePhotoAsync(string serviceNo, int photoIndex, byte[] photoBytes, CancellationToken ct = default);
    Task<byte[]?> GetPhotoAsync(string serviceNo, int photoIndex, CancellationToken ct = default);
    Task<List<Trade>> GetTradesAsync(CancellationToken ct = default);
    Task<List<State>> GetStatesAsync(CancellationToken ct = default);
}
