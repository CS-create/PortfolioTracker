using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HoldingSymbolRepository : IHoldingSymbolRepository
{
    private readonly AppDbContext _context;

    public HoldingSymbolRepository(AppDbContext context) => _context = context;

    public async Task<List<(string Symbol, string Currency)>> GetDistinctSymbolsAsync()
    {
        return await _context.Holdings
            .Select(h => new { h.Symbol, h.Currency })
            .Distinct()
            .Select(x => new ValueTuple<string, string>(x.Symbol, x.Currency))
            .ToListAsync();
    }
}