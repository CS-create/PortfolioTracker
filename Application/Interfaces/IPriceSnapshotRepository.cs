using PortfolioTracker.Domain.Entities;

namespace Application.Interfaces;

public class IPriceSnapshotRepository
{
    Task<PriceSnapshot?> GetLastestAsync(string symbol);
    Task AddAsync(PriceSnapshot priceSnapshot);
    Task SaveChangesAsync();
}