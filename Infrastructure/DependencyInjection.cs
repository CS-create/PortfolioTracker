using Application.Interfaces;
using Application.Services;
using Infrastructure.Auth;
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
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddHttpClient<IStockSearchProvider, AlphaVantageStockSearchProvider>();

        return services;
    }
}