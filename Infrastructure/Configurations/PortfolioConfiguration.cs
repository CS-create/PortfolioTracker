using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioTracker.Domain.Entities;

namespace Infrastructure.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.HasKey(p => p.Id) ;
        builder.Property(p => p.Name).IsRequired().HasMaxLength(40);
        builder.HasMany(p => p.Holdings).
            WithOne(h => h.Portfolio).
            HasForeignKey(fk => fk.PortfolioId).
            OnDelete(DeleteBehavior.Cascade);
    }
}