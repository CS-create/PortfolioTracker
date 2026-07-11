namespace Infrastructure.ExternalServices;

public class StockPriceOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.alphavantage.co";
    public int CacheDurationInMinutes { get; set; } = 15;
}