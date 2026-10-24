namespace Solution.Web.Models;

using System.ComponentModel.DataAnnotations;

public class ActivateAccountViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Account Email")]
    public string Email { get; set; } = string.Empty;

    public string? ServiceNo { get; set; }
    public string? MemberName { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Choose Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class MemberAccountListViewModel
{
    public List<MemberAccountItemDto> Accounts { get; set; } = new();
}

public class MemberAccountItemDto
{
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public string? TradeName { get; set; }
    public bool HasAccount { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public string? ActivationLink { get; set; }
}

public class ProvisionAccountViewModel
{
    [Required]
    public string ServiceNo { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
