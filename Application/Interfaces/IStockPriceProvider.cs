namespace Application.Interfaces;

public interface IStockPriceProvider
{
    Task<decimal> GetCurrentPriceAsync(string symbol, string currency);
}