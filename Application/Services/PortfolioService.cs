using Application.DTOs;
using Application.Interfaces;
using PortfolioTracker.Domain.Entities;

namespace Application.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IStockPriceProvider _stockPriceProvider;

    public PortfolioService(IPortfolioRepository portfolioRepository, IStockPriceProvider stockPriceProvider)
    {
        _portfolioRepository = portfolioRepository;
        _stockPriceProvider = stockPriceProvider;
    }

    public async Task<PortfolioDto> GetPortfolioOverviewAsync(Guid portfolioId)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId)
                        ?? throw new KeyNotFoundException("Portfolio not found");

        var holdingDtos = new List<HoldingDto>();

        foreach (var holding in portfolio.Holdings)
        {
            var currentPrice = await _stockPriceProvider.GetCurrentPriceAsync(
                holding.Symbol, holding.Currency);

            var quantity = holding.GetTotalQuantity();
            var avgCost = holding.GetAverageCostBasis();
            var marketValue = quantity * currentPrice;
            var gainLoss = marketValue - (quantity * avgCost);

            holdingDtos.Add(new HoldingDto(
                holding.Id,
                holding.Symbol,
                quantity,
                avgCost,
                currentPrice,
                marketValue,
                gainLoss,
                holding.Currency
            ));
        }

        return new PortfolioDto(
            portfolio.Id,
            portfolio.Name,
            holdingDtos.Sum(h => h.MarketValue),
            holdingDtos.Sum(h => h.UnrealizedGainLoss),
            holdingDtos
        );
    }

    public async Task AddTransactionAsync(CreateTransactionDto dto)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            HoldingId = dto.HoldingId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            PricePerUnit = dto.PricePerUnit,
            ExecutedAt = dto.ExecutedAt
        };
        await _portfolioRepository.AddTransactionAsync(transaction);
        await _portfolioRepository.SaveChangesAsync();
    }
}