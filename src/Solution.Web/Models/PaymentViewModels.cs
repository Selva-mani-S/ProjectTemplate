namespace Solution.Web.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Solution.Application.DTOs;
using Solution.Domain.Enums;

public class PaymentSubmissionViewModel
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public decimal ConfiguredFeeAmount { get; set; }

    [Required]
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Have you paid the reunion fee?")]
    public string HasPaidResponse { get; set; } = "No"; // "Yes" or "No"

    [Range(0, 1000000, ErrorMessage = "Amount must be positive.")]
    [Display(Name = "Amount Paid (INR)")]
    public decimal AmountPaid { get; set; }

    [Display(Name = "Payment Mode")]
    public PaymentMode PaymentMode { get; set; } = PaymentMode.UPI;

    [StringLength(100)]
    [Display(Name = "Acknowledgement / Transaction Number")]
    public string? AcknowledgementNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Payment Date")]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [Display(Name = "Upload Payment Proof (Receipt / Screenshot)")]
    public IFormFile? PaymentProofFile { get; set; }

    [StringLength(500)]
    [Display(Name = "Additional Remarks")]
    public string? AdditionalRemarks { get; set; }

    public PaymentDto? ExistingPayment { get; set; }
}

public class PaymentAdminIndexViewModel
{
    public int EventId { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();

    public PaymentStatus? SelectedStatus { get; set; }
    public PaymentMode? SelectedMode { get; set; }
    public string? Search { get; set; }

    // Summary counts
    public int TotalVerifiedCount { get; set; }
    public int TotalPendingCount { get; set; }
    public int TotalRejectedCount { get; set; }
    public decimal TotalVerifiedAmount { get; set; }
}

public class VerifyPaymentSubmissionModel
{
    [Required]
    public int PaymentId { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }
}
