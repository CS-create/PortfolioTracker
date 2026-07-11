using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioTracker.Domain.Entities;

public class PriceSnapshotConfiguration : IEntityTypeConfiguration<PriceSnapshot>
{
    public void Configure(EntityTypeBuilder<PriceSnapshot> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Symbol).IsRequired();
        builder.Property(p => p.Price).IsRequired();
        builder.HasIndex(p => new { p.Symbol, p.FetchedAt });
    }
}