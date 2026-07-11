using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<StockPriceOptions>(configuration.GetSection("StockPrice"));

        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPriceSnapshotRepository, PriceSnapshotRepository>();
        services.AddHttpClient<IStockPriceProvider, CachedStockPriceProvider>();

        return services;
    }
}