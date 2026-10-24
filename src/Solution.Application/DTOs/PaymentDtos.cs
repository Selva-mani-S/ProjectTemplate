namespace Solution.Application.DTOs;

using Solution.Domain.Enums;

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? AcknowledgementNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentProofPath { get; set; }
    public string? AdditionalRemarks { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsDuplicateAckNumber { get; set; }
}

public class SubmitPaymentDto
{
    public int EventId { get; set; }
    public string ServiceNo { get; set; } = string.Empty;
    public bool HasPaid { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.UPI;
    public string? AcknowledgementNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public string? PaymentProofPath { get; set; }
    public string? AdditionalRemarks { get; set; }
}

public class VerifyPaymentDto
{
    public int PaymentId { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public string VerifierName { get; set; } = string.Empty;
}
