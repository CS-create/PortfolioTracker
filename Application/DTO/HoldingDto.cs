namespace Application.DTOs;

public record HoldingDto(
    Guid Id,
    string Symbol,
    decimal Quantity,
    decimal AverageCostBasis,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal UnrealizedGainLoss,
    string Currency
);