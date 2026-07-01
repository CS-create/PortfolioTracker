using PortfolioTracker.Domain.Entities;

namespace Application.Interfaces;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(Guid id);
    Task<List<Portfolio>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Portfolio portfolio);
    Task AddHoldingAsync(Holding holding);
    Task AddTransactionAsync(Transaction transaction);
    Task SaveChangesAsync();
    
}