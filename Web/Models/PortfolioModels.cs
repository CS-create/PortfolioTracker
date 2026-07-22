namespace Web.Models;

public record PortfolioSummary(Guid Id, string Name);

public record HoldingViewData(
    Guid Id,
    string Symbol,
    decimal Quantity,
    decimal AverageCostBasis,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal UnrealizedGainLoss,
    string Currency);

public record PortfolioDetail(
    Guid Id,
    string Name,
    decimal TotalValue,
    decimal TotalGainLoss,
    List<HoldingViewData> Holdings);

public record CreatePortfolioForm(string Name);