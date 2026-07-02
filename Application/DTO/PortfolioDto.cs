namespace Application.DTOs;

public record PortfolioDto(
    Guid Id,
    string Name,
    decimal TotalValue,
    decimal TotalGainLoss,
    List<HoldingDto> Holdings
);