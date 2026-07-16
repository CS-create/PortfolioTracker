using Application.Interfaces;
using Infrastructure.ExternalServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundServices;

public class PriceRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PriceRefreshBackgroundService> _logger;
    private readonly StockPriceOptions _options;

    public PriceRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<PriceRefreshBackgroundService> logger,
        IOptions<StockPriceOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshPricesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Price refresh cycle failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.CacheDurationInMinutes), stoppingToken);
        }
    }

    private async Task RefreshPricesAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var symbolRepository = scope.ServiceProvider.GetRequiredService<IHoldingSymbolRepository>();
        var stockPriceProvider = scope.ServiceProvider.GetRequiredService<IStockPriceProvider>();

        var symbols = await symbolRepository.GetDistinctSymbolsAsync();

        _logger.LogInformation("Refreshing prices for {Count} symbols", symbols.Count);

        foreach (var (symbol, currency) in symbols)
        {
            try
            {
                await stockPriceProvider.GetCurrentPriceAsync(symbol, currency);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh price for {Symbol}", symbol);
            }
        }
    }
}