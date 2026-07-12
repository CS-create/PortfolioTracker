using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioTracker.Domain.Entities;

namespace Infrastructure.Configurations;

public class PriceSnapshotConfiguration : IEntityTypeConfiguration<PriceSnapshot>
{
    public void Configure(EntityTypeBuilder<PriceSnapshot> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(18,4)");
        builder.HasIndex(p => new { p.Symbol, p.FetchedAt });
    }
}