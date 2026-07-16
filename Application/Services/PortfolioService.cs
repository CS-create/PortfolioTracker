using Application.DTOs;
using Application.Interfaces;
using PortfolioTracker.Domain.Entities;

namespace Application.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IStockPriceProvider _stockPriceProvider;

    public PortfolioService(
        IPortfolioRepository portfolioRepository,
        IStockPriceProvider stockPriceProvider)
    {
        _portfolioRepository = portfolioRepository;
        _stockPriceProvider = stockPriceProvider;
    }

    public async Task<PortfolioDto> GetPortfolioOverviewAsync(Guid portfolioId, Guid userId)
    {
        var portfolio = await GetOwnedPortfolioAsync(portfolioId, userId);

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

    public async Task<Guid> CreatePortfolioAsync(Guid userId, CreatePortfolioDto dto)
    {
        var portfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _portfolioRepository.AddAsync(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        return portfolio.Id;
    }

    public async Task<Guid> AddHoldingAsync(Guid portfolioId, Guid userId, CreateHoldingDto dto)
    {
        await GetOwnedPortfolioAsync(portfolioId, userId);

        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            Symbol = dto.Symbol,
            Currency = dto.Currency
        };

        await _portfolioRepository.AddHoldingAsync(holding);
        await _portfolioRepository.SaveChangesAsync();

        return holding.Id;
    }

    public async Task AddTransactionAsync(Guid portfolioId, Guid userId, CreateTransactionDto dto)
    {
        var portfolio = await GetOwnedPortfolioAsync(portfolioId, userId);

        var holdingBelongsToPortfolio = portfolio.Holdings.Any(h => h.Id == dto.HoldingId);
        if (!holdingBelongsToPortfolio)
            throw new UnauthorizedAccessException("Holding does not belong to this portfolio");

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

    private async Task<Portfolio> GetOwnedPortfolioAsync(Guid portfolioId, Guid userId)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId)
            ?? throw new KeyNotFoundException("Portfolio not found");

        if (portfolio.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this portfolio");

        return portfolio;
    }
}