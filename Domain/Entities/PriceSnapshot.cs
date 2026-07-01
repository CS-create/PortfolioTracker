namespace PortfolioTracker.Domain.Entities;

public class PriceSnapshot
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "DKK";
    public DateTime FetchedAt { get; set; }
}