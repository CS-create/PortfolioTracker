using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Domain.Entities;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) {}
    
    public DbSet<Portfolio>  Portfolios => Set<Portfolio>();
    public DbSet<Holding> Holdings => Set<Holding>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbContext).Assembly);
    }

}