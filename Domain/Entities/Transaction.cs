namespace PortfolioTracker.Domain.Entities;

public enum TransactionType
{
    Buy,
    Sell
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid HoldingId { get; set; }
    public Holding Holding { get; set; } = null!;

    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public DateTime ExecutedAt { get; set; }
}