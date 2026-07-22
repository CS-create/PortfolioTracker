namespace Application.Interfaces;

public record StockSearchResult(string Symbol, string Name, string Region, string Currency);

public interface IStockSearchProvider
{
    Task<List<StockSearchResult>> SearchAsync(string query);
}