namespace Solution.Web.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Solution.Application.DTOs;
using Solution.Domain.Entities;

public class MemberRegistryIndexViewModel
{
    public List<MemberSummaryDto> Members { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public string? Search { get; set; }
    public string? TradeId { get; set; }
    public string? StateId { get; set; }
    public bool? UnactivatedOnly { get; set; }
    public bool? UnregisteredOnly { get; set; }

    public List<Trade> Trades { get; set; } = new();
    public List<State> States { get; set; } = new();
}

public class MemberFormViewModel
{
    [Required(ErrorMessage = "Service Number is required.")]
    [StringLength(6, ErrorMessage = "Service Number cannot exceed 6 characters.")]
    [Display(Name = "Service Number")]
    public string ServiceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? Dob { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Demise (if applicable)")]
    public DateTime? Dod { get; set; }

    [Display(Name = "Trade")]
    public string? TradeId { get; set; }

    [StringLength(50)]
    [Display(Name = "Address Line 1")]
    public string? Address1 { get; set; }

    [StringLength(50)]
    [Display(Name = "Address Line 2")]
    public string? Address2 { get; set; }

    [Display(Name = "State")]
    public string? StateId { get; set; }

    [StringLength(13)]
    [Phone]
    [Display(Name = "Mobile Number")]
    public string? MobileNo { get; set; }

    [StringLength(150)]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string? EmailId { get; set; }

    [StringLength(250)]
    [Display(Name = "Technical Skills")]
    public string? TechnicalSkills { get; set; }

    [StringLength(250)]
    [Display(Name = "Places Worked")]
    public string? PlacesWorked { get; set; }

    [StringLength(250)]
    [Display(Name = "Additional Details")]
    public string? AdditionalDetails { get; set; }

    [StringLength(200)]
    [Display(Name = "Achievements")]
    public string? Achievements { get; set; }

    [StringLength(200)]
    [Display(Name = "VRS Reason")]
    public string? VrsReason { get; set; }

    public bool IsEdit { get; set; }
    public List<Trade> AvailableTrades { get; set; } = new();
    public List<State> AvailableStates { get; set; } = new();
}

public class MemberImportViewModel
{
    [Required(ErrorMessage = "Please select an Excel (.xlsx) or CSV (.csv) file.")]
    public IFormFile? File { get; set; }

    public ImportPreviewDto? Preview { get; set; }
}

public class AddFamilyMemberViewModel
{
    [Required]
    public string ServiceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Member name is required.")]
    [StringLength(100)]
    public string MemberName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Relationship { get; set; }

    [StringLength(1)]
    public string? Gender { get; set; }

    [StringLength(100)]
    public string? Occupation { get; set; }

    [StringLength(200)]
    public string? Remarks { get; set; }
}
