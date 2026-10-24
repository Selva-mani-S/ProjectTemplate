namespace Solution.Domain.Entities;

using Solution.Domain.Enums;

public class IafPayment
{
    public int PaymentId { get; set; }
    public int EventId { get; set; }
    public string ServiceNo { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.UPI;
    public string? AcknowledgementNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string? PaymentProofPath { get; set; }
    public string? AdditionalRemarks { get; set; }
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAtUtc { get; set; }
    public string? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public IafReunionEvent? Event { get; set; }
    public IafPersonalData? PersonalData { get; set; }
}
