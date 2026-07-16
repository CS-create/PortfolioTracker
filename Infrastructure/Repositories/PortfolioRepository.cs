using Application.Interfaces;
using PortfolioTracker.Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext _dbContext;

    public PortfolioRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<Portfolio?> GetByIdAsync(Guid id) =>
        await _dbContext.Portfolios
            .Include(p => p.Holdings)
            .ThenInclude(h => h.Transactions)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Portfolio>> GetByUserIdAsync(Guid userId) =>
        await _dbContext.Portfolios
            .Where(p => p.UserId == userId)
            .Include(p => p.Holdings)
            .ToListAsync();

    public async Task AddAsync(Portfolio portfolio) =>
        await _dbContext.Portfolios.AddAsync(portfolio);

    public async Task AddHoldingAsync(Holding holding) =>
        await _dbContext.Holdings.AddAsync(holding);

    public async Task AddTransactionAsync(Transaction transaction) =>
        await _dbContext.Transactions.AddAsync(transaction);

    public async Task SaveChangesAsync() =>
        await _dbContext.SaveChangesAsync();
}