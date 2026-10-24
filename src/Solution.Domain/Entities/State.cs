namespace Solution.Domain.Entities;

public class State
{
    public string StateId { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;

    public ICollection<IafPersonalData> Members { get; set; } = new List<IafPersonalData>();
}
