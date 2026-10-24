namespace Solution.Application.Common.Interfaces;

using Solution.Application.DTOs;
using Solution.Domain.Enums;

public interface IPaymentService
{
    Task<(bool Success, string Message, int PaymentId)> SubmitPaymentAsync(SubmitPaymentDto dto, CancellationToken ct = default);
    Task<List<PaymentDto>> GetPaymentsAsync(int eventId, PaymentStatus? status = null, PaymentMode? mode = null, string? search = null, CancellationToken ct = default);
    Task<PaymentDto?> GetPaymentByIdAsync(int paymentId, CancellationToken ct = default);
    Task<PaymentDto?> GetMemberPaymentAsync(int eventId, string serviceNo, CancellationToken ct = default);
    Task<(bool Success, string Message)> VerifyPaymentAsync(VerifyPaymentDto dto, CancellationToken ct = default);
    Task<bool> CheckDuplicateAckNumberAsync(string ackNumber, int? excludePaymentId = null, CancellationToken ct = default);
}
