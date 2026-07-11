using Application.Interfaces;
using PortfolioTracker.Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PriceSnapshotRepository : IPriceSnapshotRepository
{
    private readonly AppDbContext _context;
    public PriceSnapshotRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<PriceSnapshot?> GetLatestAsync(string symbol)
    {
        return await _context.PriceSnapshots
            .Where(p => p.Symbol == symbol)
            .OrderByDescending(p => p.FetchedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PriceSnapshot snapshot)
    {
        await _context.PriceSnapshots.AddAsync(snapshot);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}