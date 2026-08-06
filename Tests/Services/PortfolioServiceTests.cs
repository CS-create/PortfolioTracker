using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Moq;
using PortfolioTracker.Domain.Entities;
using Xunit;

namespace Tests.Services;

public class PortfolioServiceTests
{
    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<IStockPriceProvider> _stockPriceProvider = new();
    private readonly PortfolioService _sut;

    public PortfolioServiceTests()
    {
        _sut = new PortfolioService(_portfolioRepository.Object, _stockPriceProvider.Object);
    }

    [Fact]
    public async Task CreatePortfolioAsync_AddsPortfolioAndSaves()
    {
        var userId = Guid.NewGuid();
        var dto = new CreatePortfolioDto("My Portfolio");

        var resultId = await _sut.CreatePortfolioAsync(userId, dto);

        _portfolioRepository.Verify(r => r.AddAsync(It.Is<Portfolio>(
            p => p.UserId == userId && p.Name == "My Portfolio")), Times.Once);
        _portfolioRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task GetPortfolioOverviewAsync_OwnedPortfolio_ReturnsCorrectCalculations()
    {
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            Symbol = "AAPL",
            Currency = "USD",
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100 }
            }
        };

        var portfolio = new Portfolio
        {
            Id = portfolioId,
            UserId = userId,
            Name = "Test",
            Holdings = new List<Holding> { holding }
        };

        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);
        _stockPriceProvider.Setup(p => p.GetCurrentPriceAsync("AAPL", "USD")).ReturnsAsync(150m);

        var result = await _sut.GetPortfolioOverviewAsync(portfolioId, userId);

        Assert.Equal(portfolioId, result.Id);
        Assert.Single(result.Holdings);
        Assert.Equal(10, result.Holdings[0].Quantity);
        Assert.Equal(100, result.Holdings[0].AverageCostBasis);
        Assert.Equal(150, result.Holdings[0].CurrentPrice);
        Assert.Equal(1500, result.Holdings[0].MarketValue);
        Assert.Equal(500, result.Holdings[0].UnrealizedGainLoss); // 1500 - (10*100)
        Assert.Equal(1500, result.TotalValue);
        Assert.Equal(500, result.TotalGainLoss);
    }

    [Fact]
    public async Task GetPortfolioOverviewAsync_PortfolioDoesNotExist_ThrowsKeyNotFound()
    {
        var portfolioId = Guid.NewGuid();
        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId))
            .ReturnsAsync((Portfolio?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetPortfolioOverviewAsync(portfolioId, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPortfolioOverviewAsync_UserDoesNotOwnPortfolio_ThrowsUnauthorized()
    {
        var portfolioId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var portfolio = new Portfolio
        {
            Id = portfolioId,
            UserId = ownerId,
            Name = "Test",
            Holdings = new List<Holding>()
        };

        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.GetPortfolioOverviewAsync(portfolioId, otherUserId));
    }

    [Fact]
    public async Task AddHoldingAsync_OwnedPortfolio_AddsHoldingAndSaves()
    {
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var dto = new CreateHoldingDto("MSFT", "USD");

        var portfolio = new Portfolio
        {
            Id = portfolioId,
            UserId = userId,
            Name = "Test",
            Holdings = new List<Holding>()
        };

        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);

        var resultId = await _sut.AddHoldingAsync(portfolioId, userId, dto);

        _portfolioRepository.Verify(r => r.AddHoldingAsync(It.Is<Holding>(
            h => h.Symbol == "MSFT" && h.Currency == "USD" && h.PortfolioId == portfolioId)), Times.Once);
        _portfolioRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task AddHoldingAsync_UserDoesNotOwnPortfolio_ThrowsUnauthorized()
    {
        var portfolioId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var portfolio = new Portfolio { Id = portfolioId, UserId = ownerId, Name = "Test", Holdings = new List<Holding>() };
        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.AddHoldingAsync(portfolioId, otherUserId, new CreateHoldingDto("MSFT", "USD")));
    }

    [Fact]
    public async Task AddTransactionAsync_HoldingBelongsToPortfolio_AddsTransactionAndSaves()
    {
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var holdingId = Guid.NewGuid();

        var holding = new Holding { Id = holdingId, PortfolioId = portfolioId, Symbol = "AAPL", Currency = "USD", Transactions = new List<Transaction>() };
        var portfolio = new Portfolio { Id = portfolioId, UserId = userId, Name = "Test", Holdings = new List<Holding> { holding } };

        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);

        var dto = new CreateTransactionDto(holdingId, TransactionType.Buy, 5, 200, DateTime.UtcNow);

        await _sut.AddTransactionAsync(portfolioId, userId, dto);

        _portfolioRepository.Verify(r => r.AddTransactionAsync(It.Is<Transaction>(
            t => t.HoldingId == holdingId && t.Quantity == 5 && t.PricePerUnit == 200)), Times.Once);
        _portfolioRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddTransactionAsync_HoldingDoesNotBelongToPortfolio_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var unrelatedHoldingId = Guid.NewGuid();

        var portfolio = new Portfolio { Id = portfolioId, UserId = userId, Name = "Test", Holdings = new List<Holding>() };
        _portfolioRepository.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);

        var dto = new CreateTransactionDto(unrelatedHoldingId, TransactionType.Buy, 5, 200, DateTime.UtcNow);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.AddTransactionAsync(portfolioId, userId, dto));
    }

    [Fact]
    public async Task GetPortfoliosForUserAsync_ReturnsSummariesForUsersPortfolios()
    {
        var userId = Guid.NewGuid();
        var portfolios = new List<Portfolio>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Portfolio A" },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Portfolio B" }
        };

        _portfolioRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(portfolios);

        var result = await _sut.GetPortfoliosForUserAsync(userId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Portfolio A");
        Assert.Contains(result, p => p.Name == "Portfolio B");
    }
}