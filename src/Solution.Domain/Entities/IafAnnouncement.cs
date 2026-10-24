namespace Solution.Domain.Entities;

public class IafAnnouncement
{
    public int AnnouncementId { get; set; }
    public int? EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
    public string AuthorName { get; set; } = "Organizing Committee";

    public IafReunionEvent? Event { get; set; }
}
