namespace Solution.Web.Models;

using System.ComponentModel.DataAnnotations;
using Solution.Domain.Enums;

public class ReunionRegistrationViewModel
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public decimal BaseFeeAmount { get; set; }
    public DateTime RegistrationDeadline { get; set; }

    [Required]
    public string ServiceNo { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;

    [Required]
    [Range(1, 10, ErrorMessage = "Number of attendees must be between 1 and 10.")]
    [Display(Name = "Total Number of Persons Attending")]
    public int NoOfPersons { get; set; } = 1;

    [StringLength(100)]
    [Display(Name = "Accompanying Persons Details (Spouse/Children Names)")]
    public string? AccompDetails { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Date & Time of Arrival")]
    public DateTime? DateOfArrival { get; set; }

    [StringLength(20)]
    [Display(Name = "Place of Arrival")]
    public string? PlaceOfArrival { get; set; }

    [StringLength(100)]
    [Display(Name = "Accommodation Requirements")]
    public string? Accommodation { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Date & Time of Departure")]
    public DateTime? DateOfDeparture { get; set; }

    [StringLength(50)]
    [Display(Name = "Place of Departure")]
    public string? PlaceOfDeparture { get; set; }

    [Display(Name = "T-Shirt Size")]
    public string? TShirt { get; set; } = "L";

    [Display(Name = "Drinking Preference")]
    public int Drinker { get; set; } // 0 = Non-Drinker, 1 = Drinker

    [Display(Name = "Food Preference")]
    public int VegNonVeg { get; set; } // 0 = Vegetarian, 1 = Non-Vegetarian

    public bool IsClosed { get; set; }
    public RegistrationStatus CurrentStatus { get; set; } = RegistrationStatus.Pending;
}

public class ReunionEventViewModel
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Batch is required.")]
    [StringLength(50)]
    public string Batch { get; set; } = "1986";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Event Start Date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddMonths(3);

    [DataType(DataType.Date)]
    [Display(Name = "Event End Date")]
    public DateTime? EventEndDate { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    [StringLength(250)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Registration Deadline")]
    public DateTime RegistrationDeadline { get; set; } = DateTime.Today.AddMonths(2);

    [Required]
    [Range(0, 1000000, ErrorMessage = "Fee must be non-negative.")]
    [Display(Name = "Base Reunion Fee (INR)")]
    public decimal BaseFeeAmount { get; set; } = 5000.00m;

    [Display(Name = "Is Active Event")]
    public bool IsActive { get; set; } = true;

    [StringLength(2000)]
    public string? Description { get; set; }
}
