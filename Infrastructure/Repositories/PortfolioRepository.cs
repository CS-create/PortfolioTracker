using Application.Interfaces;
using PortfolioTracker.Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext  _dbContext;
    public PortfolioRepository(AppDbContext dbContext) => _dbContext = dbContext;
    
    public async Task<Portfolio?> GetByIdAsync(Guid id) =>
    await _dbContext.Portfolios
        .Include(p => p.Holdings)
        .ThenInclude(h => h.Transactions)
        .FirstOrDefaultAsync(p => p.Id == id);
            

    public Task<List<Portfolio>> GetByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Portfolio portfolio)
    {
        throw new NotImplementedException();
    }

    public Task AddHoldingAsync(Holding holding)
    {
        throw new NotImplementedException();
    }

    public Task AddTransactionAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}