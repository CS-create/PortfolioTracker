using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioTracker.Domain.Entities;

namespace Infrastructure.Configurations;

public class HoldingConfiguration : IEntityTypeConfiguration<Holding>
{
    public void Configure(EntityTypeBuilder<Holding> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(h => h.Currency).IsRequired().HasMaxLength(3);
        
        builder.HasMany(h => h.Transactions)
            .WithOne(t => t.Holding)
            .HasForeignKey(t => t.HoldingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(h => new { h.PortfolioId, h.Symbol }).IsUnique();
    }
}