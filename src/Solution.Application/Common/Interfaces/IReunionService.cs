namespace Solution.Application.Common.Interfaces;

using Solution.Application.DTOs;
using Solution.Domain.Entities;
using Solution.Domain.Enums;

public interface IReunionService
{
    Task<IafReunionEvent?> GetActiveEventAsync(CancellationToken ct = default);
    Task<List<IafReunionEvent>> GetAllEventsAsync(CancellationToken ct = default);
    Task<bool> SaveEventAsync(ReunionEventDto dto, CancellationToken ct = default);
    Task<ReunionRegistrationDto?> GetRegistrationAsync(int eventId, string serviceNo, CancellationToken ct = default);
    Task<(bool Success, string Message)> RegisterOrUpdateAsync(SubmitReunionRegistrationDto dto, CancellationToken ct = default);
    Task<List<ReunionRegistrationDto>> GetAllRegistrationsAsync(int eventId, CancellationToken ct = default);
    Task<bool> UpdateRegistrationStatusAsync(int reunionId, RegistrationStatus status, CancellationToken ct = default);
}
