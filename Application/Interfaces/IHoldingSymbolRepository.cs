namespace Application.Interfaces;

public interface IHoldingSymbolRepository
{
    Task<List<(string Symbol, string Currency)>> GetDistinctSymbolsAsync();
}