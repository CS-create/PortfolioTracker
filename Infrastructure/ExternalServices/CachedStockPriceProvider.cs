using System.Text.Json;
using Application.Interfaces;
using PortfolioTracker.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices;

public class CachedStockPriceProvider : IStockPriceProvider
{
    private readonly HttpClient _httpClient;
    private readonly IPriceSnapshotRepository _snapshotRepository;
    private readonly StockPriceOptions _options;

    public CachedStockPriceProvider(
        HttpClient httpClient,
        IPriceSnapshotRepository snapshotRepository,
        IOptions<StockPriceOptions> options)
    {
        _httpClient = httpClient;
        _snapshotRepository = snapshotRepository;
        _options = options.Value;
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, string currency)
    {
        var cached = await _snapshotRepository.GetLatestAsync(symbol);

        if (cached is not null &&
            cached.FetchedAt > DateTime.UtcNow.AddMinutes(-_options.CacheDurationInMinutes))
        {
            return cached.Price;
        }

        var price = await FetchFromApiAsync(symbol);

        await _snapshotRepository.AddAsync(new PriceSnapshot
        {
            Id = Guid.NewGuid(),
            Symbol = symbol,
            Price = price,
            Currency = currency,
            FetchedAt = DateTime.UtcNow
        });
        await _snapshotRepository.SaveChangesAsync();

        return price;
    }

    private async Task<decimal> FetchFromApiAsync(string symbol)
    {
        var url = $"{_options.BaseUrl}/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_options.ApiKey}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var priceString = doc.RootElement
            .GetProperty("Global Quote")
            .GetProperty("05. price")
            .GetString();

        return decimal.Parse(priceString!, System.Globalization.CultureInfo.InvariantCulture);
    }
}