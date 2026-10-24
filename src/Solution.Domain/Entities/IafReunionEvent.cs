namespace Solution.Domain.Entities;

public class IafReunionEvent
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public DateTime RegistrationDeadline { get; set; }
    public decimal BaseFeeAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<IafReunion> Registrations { get; set; } = new List<IafReunion>();
    public ICollection<IafPayment> Payments { get; set; } = new List<IafPayment>();
}
