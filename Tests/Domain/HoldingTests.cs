using PortfolioTracker.Domain.Entities;

namespace Tests.Domain;

public class HoldingTests
{
    private static Holding CreateHolding(params Transaction[] transactions)
    {
        var holding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            Symbol = "AAPL",
            Currency = "USD",
            Transactions = transactions.ToList()
        };
        return holding;
    }

    [Fact]
    public void GetTotalQuantity_WithSingleBuy_ReturnsThatQuantity()
    {
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100 }
        );

        var result = holding.GetTotalQuantity();

        Assert.Equal(10, result);
    }

    [Fact]
    public void GetTotalQuantity_WithBuyThenSell_ReturnsRemainingQuantity()
    {
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100 },
            new Transaction { Type = TransactionType.Sell, Quantity = 4, PricePerUnit = 120 }
        );

        var result = holding.GetTotalQuantity();

        Assert.Equal(6, result);
    }

    [Fact]
    public void GetTotalQuantity_WithNoTransactions_ReturnsZero()
    {
        var holding = CreateHolding();

        var result = holding.GetTotalQuantity();

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTotalQuantity_SellingMoreThanBought_ReturnsNegative()
    {
        // Documents current behavior — the domain does not currently
        // guard against overselling. Worth knowing this is allowed.
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Buy, Quantity = 5, PricePerUnit = 100 },
            new Transaction { Type = TransactionType.Sell, Quantity = 10, PricePerUnit = 120 }
        );

        var result = holding.GetTotalQuantity();

        Assert.Equal(-5, result);
    }

    [Fact]
    public void GetAverageCostBasis_WithSingleBuy_ReturnsThatPrice()
    {
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100 }
        );

        var result = holding.GetAverageCostBasis();

        Assert.Equal(100, result);
    }

    [Fact]
    public void GetAverageCostBasis_WithMultipleBuysAtDifferentPrices_ReturnsWeightedAverage()
    {
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100 },
            new Transaction { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 200 }
        );

        var result = holding.GetAverageCostBasis();

        // (10*100 + 10*200) / 20 = 150
        Assert.Equal(150, result);
    }

    [Fact]
    public void GetAverageCostBasis_IgnoresSellTransactions()
    {
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100 },
            new Transaction { Type = TransactionType.Sell, Quantity = 5, PricePerUnit = 999 }
        );

        var result = holding.GetAverageCostBasis();

        // Sell price should not affect cost basis
        Assert.Equal(100, result);
    }

    [Fact]
    public void GetAverageCostBasis_WithNoTransactions_ReturnsZero()
    {
        var holding = CreateHolding();

        var result = holding.GetAverageCostBasis();

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetAverageCostBasis_WithOnlySellTransactions_ReturnsZero()
    {
        var holding = CreateHolding(
            new Transaction { Type = TransactionType.Sell, Quantity = 5, PricePerUnit = 100 }
        );

        var result = holding.GetAverageCostBasis();

        Assert.Equal(0, result);
    }
}