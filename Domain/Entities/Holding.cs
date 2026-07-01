namespace PortfolioTracker.Domain.Entities;

public class Holding
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; }
    public string Symbol { get; set; }
    public string Currency { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    
    public decimal GetTotalQuantity()
    {
        return Transactions.Sum(t => t.Type == TransactionType.Buy ? t.Quantity : -t.Quantity);
    }

    public decimal GetAverageCostBasis()
    {
        var buys = Transactions.Where(t => t.Type == TransactionType.Buy).ToList();
        var totalQuantity = buys.Sum(t => t.Quantity);
        if (totalQuantity == 0) 
            return 0;
        var totalCost = buys.Sum(t => t.Quantity * t.PricePerUnit);
        return totalCost / totalQuantity;
    }
}