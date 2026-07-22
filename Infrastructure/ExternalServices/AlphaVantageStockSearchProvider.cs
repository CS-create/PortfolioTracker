using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices;

public class AlphaVantageStockSearchProvider : IStockSearchProvider
{
    private readonly HttpClient _httpClient;
    private readonly StockPriceOptions _options;

    public AlphaVantageStockSearchProvider(HttpClient httpClient, IOptions<StockPriceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<List<StockSearchResult>> SearchAsync(string query)
    {
        var url = $"{_options.BaseUrl}/query?function=SYMBOL_SEARCH&keywords={Uri.EscapeDataString(query)}&apikey={_options.ApiKey}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(rawJson);

        var results = new List<StockSearchResult>();

        if (!doc.RootElement.TryGetProperty("bestMatches", out var matches))
        {
            return results;
        }

        foreach (var match in matches.EnumerateArray())
        {
            var symbol = match.GetProperty("1. symbol").GetString() ?? "";
            var name = match.GetProperty("2. name").GetString() ?? "";
            var region = match.GetProperty("4. region").GetString() ?? "";
            var currency = match.GetProperty("8. currency").GetString() ?? "";

            results.Add(new StockSearchResult(symbol, name, region, currency));
        }

        return results;
    }
}