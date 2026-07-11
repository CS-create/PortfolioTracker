using PortfolioTracker.Domain.Entities;

namespace Application.Interfaces;

public interface IPriceSnapshotRepository
{
    Task<PriceSnapshot?> GetLatestAsync(string symbol);
    Task AddAsync(PriceSnapshot priceSnapshot);
    Task SaveChangesAsync();
}