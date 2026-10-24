namespace Solution.Web.Models;

public class ReportListViewModel
{
    public int ActiveEventId { get; set; }
    public string ActiveEventTitle { get; set; } = string.Empty;
    public List<ReportItemViewModel> Reports { get; set; } = new();
}

public class ReportItemViewModel
{
    public int ReportNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Registry", "Logistics", "Financial"
}
