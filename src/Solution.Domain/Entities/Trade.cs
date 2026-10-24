namespace Solution.Domain.Entities;

public class Trade
{
    public string TradeId { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;

    public ICollection<IafPersonalData> Members { get; set; } = new List<IafPersonalData>();
}
