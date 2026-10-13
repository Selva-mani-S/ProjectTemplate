namespace Solution.Web.Models;

using System.ComponentModel.DataAnnotations;

public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class UserManagementViewModel
{
    public List<UserSummaryDto> Users { get; set; } = new();
    public string? SelectedDepartmentFilter { get; set; }
    public List<string> AvailableDepartments { get; set; } = new();
}

public class CreateUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string Role { get; set; } = "User";
}

public class EditUserDepartmentViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;
}
