namespace PortfolioTracker.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Holding> Holdings { get; set; }
    
}